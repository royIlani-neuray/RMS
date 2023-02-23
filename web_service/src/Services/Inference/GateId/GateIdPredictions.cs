/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.RadarLogic.Tracking;
using WebService.WebSockets;
using WebService.Services.Inference.Utils;

namespace WebService.Services.Inference.GateId;

public class GateIdPredictions 
{
    public const string InvalidGateIdPrediction = "Unknown";
    public const float DEFAULT_CONFIDENCE_THRESHOLD = 0.50F;

    private class Prediction
    {
        [JsonPropertyName("track_id")]
        public uint TrackId {get; set;}

        [JsonPropertyName("identity")]
        public String Identity {get; set;}

        public LabelHitMissPredictor hitMissPredictor;

        public Prediction(uint trackId, int requiredHitCount, int requiredMissCount)
        {
            TrackId = trackId;
            Identity = String.Empty;
            hitMissPredictor = new LabelHitMissPredictor(requiredHitCount, requiredMissCount, GateIdPredictions.InvalidGateIdPrediction);
        }
    }

    private Dictionary<uint, Prediction> predictions;
    private RadarWebSocketServer deviceWebSocketsServer;

    private int requiredHitCount;
    private int requiredMissCount;
    
    public float ConfidenceThreshold {get; set;}

    public GateIdPredictions(RadarWebSocketServer deviceWebSocketsServer, int requiredHitCount, int requiredMissCount)
    {
        predictions = new Dictionary<uint, Prediction>();
        this.deviceWebSocketsServer = deviceWebSocketsServer;
        this.requiredHitCount = requiredHitCount;
        this.requiredMissCount = requiredMissCount;
        ConfidenceThreshold = DEFAULT_CONFIDENCE_THRESHOLD;
    }

    public void RemoveLostTracks(FrameData frame)
    {
        List<uint> deadTracks = new List<uint>();

        foreach (var trackId in predictions.Keys)
        {
            if (!frame.TracksList.Exists(track => track.TrackId == trackId))
                deadTracks.Add(trackId);
        }

        foreach (var deadTrackId in deadTracks)
        {
            predictions.Remove(deadTrackId);
        }
    }

    public void UpdateTrackPrediction(byte trackId, string identity, float confidence)
    {
        if (confidence < ConfidenceThreshold)
            return;

        if (!predictions.ContainsKey(trackId))
        {
            predictions.Add(trackId, new Prediction(trackId, requiredHitCount, requiredMissCount));
        }

        predictions[trackId].hitMissPredictor.UpdatePrediction(identity);
        predictions[trackId].Identity = predictions[trackId].hitMissPredictor.GetPrediction();
    }

    public void PublishPredictions()
    {
        deviceWebSocketsServer.SendGateIdPredictions(predictions.Values.ToList());
    }
}