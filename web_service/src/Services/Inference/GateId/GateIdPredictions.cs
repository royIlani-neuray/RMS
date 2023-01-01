/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Tracking;
using WebService.WebSockets;

namespace WebService.Services.Inference.GateId;

public class GateIdPredictions 
{
    private class Prediction
    {
        [JsonPropertyName("track_id")]
        public uint TrackId {get; set;}

        [JsonPropertyName("identity")]
        public String Identity {get; set;}

        [JsonPropertyName("accuracy")]
        public float Accuracy {get; set;}

        public Prediction(uint trackId)
        {
            TrackId = trackId;
            Identity = String.Empty;
        }
    }

    private Dictionary<uint, Prediction> predictions;
    private DeviceWebSocketServer deviceWebSocketsServer;

    public GateIdPredictions(DeviceWebSocketServer deviceWebSocketsServer)
    {
        predictions = new Dictionary<uint, Prediction>();
        this.deviceWebSocketsServer = deviceWebSocketsServer;
    }

    public void RemoveLostTracks(FrameData frame)
    {
        List<uint> deadTracks = new List<uint>();

        foreach (var trackId in predictions.Keys)
        {
            if (!frame.tracksList.Exists(track => track.TrackId == trackId))
                deadTracks.Add(trackId);
        }

        foreach (var deadTrackId in deadTracks)
        {
            predictions.Remove(deadTrackId);
        }
    }

    public void UpdateTrackPrediction(byte trackId, string identity, float accuracy)
    {
        if (!predictions.ContainsKey(trackId))
        {
            predictions.Add(trackId, new Prediction(trackId));
        }

        predictions[trackId].Identity = identity;
        predictions[trackId].Accuracy = accuracy;
    }

    public void PublishPredictions()
    {
        deviceWebSocketsServer.SendGateIdPredictions(predictions.Values.ToList());
    }
}