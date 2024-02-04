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

namespace WebService.Services.Inference.GateId;

public class GateIdContext : WorkerThread<FrameData>, IServiceContext 
{
    public IServiceContext.ServiceState State { get; set; }

    private const int MAX_QUEUE_CAPACITY = 20;
    
    private const int POINTS_COUNT_PER_FRAME = 128;
    private const int MIN_POINTS_FOR_VALID_FRAME = 7;
    private const int MAX_INVALID_FRAMES = 10;

    private GaitIdWindowBuilder windowBuilder;
    private GateIdPredictions predictions;
    private string modelName;
    
    public GateIdContext(Radar radar, string modelName, int requiredWindowSize, int windowShiftSize, int minRequiredHitCount, int majorityWindowSize) : base("GateIdContext", MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;
        windowBuilder = new GaitIdWindowBuilder(requiredWindowSize, windowShiftSize, POINTS_COUNT_PER_FRAME, MIN_POINTS_FOR_VALID_FRAME, MAX_INVALID_FRAMES);
        predictions = new GateIdPredictions(radar.RadarWebSocket, minRequiredHitCount, majorityWindowSize);
        this.modelName = modelName;
    }

    public void HandleFrame(FrameData frameData)
    {
        Enqueue(frameData);
    }

    private async Task RunInference(Dictionary<byte, GateIdRequest> readyWindows)
    {
        foreach (var trackId in readyWindows.Keys)
        {
            GateIdRequest predictRequest = readyWindows[trackId];
            string requestJsonString = JsonSerializer.Serialize(predictRequest);
            string responseJsonString = await InferenceServiceClient.Instance.Predict(modelName, requestJsonString);
            GateIdResponse response = JsonSerializer.Deserialize<GateIdResponse>(responseJsonString)!;
            //System.Console.WriteLine($"Gate Id - Track-{trackId} => {response.Label} [ {(response.Accuracy * 100):0.00} % ]");

            predictions.UpdateTrackPrediction(trackId, response.Label, response.Confidence);   
        }
    }

    protected override async Task DoWork(FrameData frame)
    {
        predictions.RemoveLostTracks(frame);

        windowBuilder.AddFrame(frame);

        Dictionary<byte, GateIdRequest> readyWindows = windowBuilder.PullReadyWindows();

        try
        {
            await RunInference(readyWindows);
        }
        catch (Exception ex)
        {
            State = IServiceContext.ServiceState.Error;
            System.Console.WriteLine("Error: failed running GateId inference: " + ex.Message);
            return;
        }

        predictions.PublishPredictions();
    }
}