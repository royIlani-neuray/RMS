/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
using WebService.Utils;
using WebService.RadarLogic.Tracking;
using WebService.Entites;
using Serilog;

namespace WebService.Services.Inference.SmartFanGestures;

public class SmartFanContext : WorkerThread<FrameData>, IServiceContext 
{
    public IServiceContext.ServiceState State { get; set; }

    private const int MAX_QUEUE_CAPACITY = 20;
    
    private const int POINTS_COUNT_PER_FRAME = 32;
    private const int MIN_POINTS_FOR_VALID_FRAME = 7;
    private const int MAX_INVALID_FRAMES = 19;

    private SmartFanWindowBuilder windowBuilder;
    private SmartFanGesturesPredictions predictions;
    private string modelName;
    
    public SmartFanContext(Radar radar, string modelName, int requiredWindowSize, int windowShiftSize, int minRequiredHitCount, int majorityWindowSize) : base("SmartFanContext", MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;
        windowBuilder = new SmartFanWindowBuilder(requiredWindowSize, windowShiftSize, POINTS_COUNT_PER_FRAME, MIN_POINTS_FOR_VALID_FRAME, MAX_INVALID_FRAMES);
        predictions = new SmartFanGesturesPredictions(radar.RadarWebSocket, minRequiredHitCount, majorityWindowSize);
        this.modelName = modelName;

        // Log.Debug("\n*** DEBUG: SmartFanContext ***");
        // Log.Debug($"model name: {modelName}");
        // Log.Debug($"required Window Size: {requiredWindowSize}");
        // Log.Debug($"window Shift Size: {windowShiftSize}");
        // Log.Debug($"required Points Per Frame: {POINTS_COUNT_PER_FRAME}");
        // Log.Debug($"min Points For Valid Frame: {MIN_POINTS_FOR_VALID_FRAME}");
        // Log.Debug($"max Invalid Frames: {MAX_INVALID_FRAMES}");
        // Log.Debug($"minRequiredHitCount: {minRequiredHitCount}");
        // Log.Debug($"majorityWindowSize: {majorityWindowSize}\n");
    }

    public void HandleFrame(FrameData frameData)
    {
        Enqueue(frameData);
    }

    private async Task RunInference(Dictionary<byte, SmartFanGestureRequest> readyWindows)
    {
        foreach (var trackId in readyWindows.Keys)
        {
            SmartFanGestureRequest predictRequest = readyWindows[trackId];
            string requestJsonString = JsonSerializer.Serialize(predictRequest);
            string responseJsonString = await InferenceServiceClient.Instance.Predict(modelName, requestJsonString);
            SmartFanGestureResponse response = JsonSerializer.Deserialize<SmartFanGestureResponse>(responseJsonString)!;
            //Log.Debug($"Gate Id - Track-{trackId} => {response.Label} [ {(response.Accuracy * 100):0.00} % ]");

            predictions.UpdateTrackPrediction(trackId, response.Label, response.Confidence);   
        }
    }

    protected override async Task DoWork(FrameData frame)
    {
        predictions.RemoveLostTracks(frame);

        windowBuilder.AddFrame(frame);

        Dictionary<byte, SmartFanGestureRequest> readyWindows = windowBuilder.PullReadyWindows();

        try
        {
            await RunInference(readyWindows);
        }
        catch (Exception ex)
        {
            State = IServiceContext.ServiceState.Error;
            Log.Error("failed running SmartFanGesture inference", ex);
            return;
        }

        predictions.PublishPredictions();
    }
}