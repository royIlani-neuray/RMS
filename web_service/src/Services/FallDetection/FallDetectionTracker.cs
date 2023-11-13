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

namespace WebService.Services.FallDetection;

public class FallDetectionTracker 
{
    private class FallData
    {
        [JsonPropertyName("track_id")]
        public uint TrackId {get; set;}

        public Queue<float> LastHeights = new Queue<float>();
        public DateTime lastDetectionTime;
    }

    private List<FallData> tracksFallData {get; set;}

    private float fallingThreshold;
    private int minTrackingDurationFrames;
    private int maxTrackingDurationFrames;
    private float alertCooldownSeconds;

    private RadarWebSocketServer deviceWebSocketsServer;

    public FallDetectionTracker(RadarWebSocketServer deviceWebSocketsServer, float fallingThreshold, int minTrackingDurationFrames, int maxTrackingDurationFrames, float alertCooldownSeconds)
    {
        tracksFallData = new List<FallData>();

        this.fallingThreshold = fallingThreshold;
        this.minTrackingDurationFrames = minTrackingDurationFrames;
        this.maxTrackingDurationFrames = maxTrackingDurationFrames;
        this.alertCooldownSeconds = alertCooldownSeconds;

        this.deviceWebSocketsServer = deviceWebSocketsServer;

        // Console.WriteLine($"** Debug:");
        // Console.WriteLine($"** Debug: FALL DETECTION PARAMS:");
        // Console.WriteLine($"** Debug: fallingThreshold: {fallingThreshold}");
        // Console.WriteLine($"** Debug: minTrackingDurationFrames: {minTrackingDurationFrames}");
        // Console.WriteLine($"** Debug: maxTrackingDurationFrames: {maxTrackingDurationFrames}");
        // Console.WriteLine($"** Debug: alertCooldownSeconds: {alertCooldownSeconds}");
        // Console.WriteLine($"** Debug:");

    }

    private void RemoveOldTargets(List<FrameData.TargetHeight> targets)
    {
        List<uint> frameTargetsIds = targets.ConvertAll(target => target.TargetId);

        List<FallData> updatedList = new List<FallData>();

        foreach (var fallData in tracksFallData)
        {
            if (frameTargetsIds.Contains(fallData.TrackId))
            {
                updatedList.Add(fallData);
            }
        }

        tracksFallData = updatedList;
    }

    private void UpdateFallData(FallData fallData, float currentHeight)
    {
        // Console.WriteLine($"** Debug: Track-{fallData.TrackId}, Height: {currentHeight:0.00} meters");

        if (fallData.lastDetectionTime.AddSeconds(alertCooldownSeconds) > DateTime.UtcNow)
        {
            // we are in cooldown, don't update the fall data for this track.
            return;
        }

        fallData.LastHeights.Enqueue(currentHeight);

        if (fallData.LastHeights.Count > maxTrackingDurationFrames)
            fallData.LastHeights.Dequeue();

        if (fallData.LastHeights.Count < minTrackingDurationFrames)
            return;
        
        float averageHeight = fallData.LastHeights.Average();

        if (currentHeight < (fallingThreshold * averageHeight))
        {
            // fall detected!
            fallData.lastDetectionTime = DateTime.UtcNow;
            fallData.LastHeights.Clear();
            
            //Console.WriteLine($"****** FALL DETECTED!!!!!! Track {fallData.TrackId} ***********");
            deviceWebSocketsServer.SendFallDetectionData(fallData);
        }
        
    }

    public void HandleFrame(FrameData frame)
    {
        RemoveOldTargets(frame.TargetsHeightList);

        // add and update fall data according to latest frame
        foreach (var target in frame.TargetsHeightList)
        {
            FallData? fallData = tracksFallData.FirstOrDefault(trackData => trackData.TrackId == target.TargetId);

            if (fallData == null)
            {
                // add new entry
                fallData = new FallData();
                fallData.TrackId = target.TargetId;
                fallData.lastDetectionTime = DateTime.UtcNow.AddSeconds(-alertCooldownSeconds);
                tracksFallData.Add(fallData);
            }

            // update fall data according to the new target info
            UpdateFallData(fallData, target.MaxZ);
        }
        
    }

}