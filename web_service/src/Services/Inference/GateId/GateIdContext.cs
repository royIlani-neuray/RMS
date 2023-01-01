/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
using WebService.Utils;
using WebService.Tracking;
using WebService.Entites;

namespace WebService.Services.Inference.GateId;

public class GateIdContext : WorkerThread<FrameData>, IServiceContext 
{
    public IServiceContext.ServiceState State { get; set; }

    private const int MAX_QUEUE_CAPACITY = 5;

    private TracksWindowBuilder tracksWindowBuilder;
    private GateIdPredictions predictions;
    private string modelName;
    
    public GateIdContext(RadarDevice device, string modelName, int requiredWindowSize) : base("GateIdContext", MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;
        tracksWindowBuilder = new TracksWindowBuilder(requiredWindowSize);
        predictions = new GateIdPredictions(device.DeviceWebSocket);
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

            predictions.UpdateTrackPrediction(trackId, response.Label, response.Accuracy);   
        }
    }

    protected override async Task DoWork(FrameData frame)
    {
        predictions.RemoveLostTracks(frame);

        tracksWindowBuilder.AddFrame(frame);

        Dictionary<byte, GateIdRequest> readyWindows = tracksWindowBuilder.PullReadyWindows();

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