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

        [JsonPropertyName("fall_detected")]
        public bool fallDetected {get; set;} = false;

        public Queue<float> LastHeights = new Queue<float>();
        public DateTime detectionTime;
    }

    private List<FallData> tracksFallData {get; set;}

    private float fallingThreshold;
    private int minTrackingDurationFrames;
    private int maxTrackingDurationFrames;
    private float alertDurationSeconds;

    private RadarWebSocketServer deviceWebSocketsServer;

    public FallDetectionTracker(RadarWebSocketServer deviceWebSocketsServer, float fallingThreshold, int minTrackingDurationFrames, int maxTrackingDurationFrames, float alertDurationSeconds)
    {
        tracksFallData = new List<FallData>();

        this.fallingThreshold = fallingThreshold;
        this.minTrackingDurationFrames = minTrackingDurationFrames;
        this.maxTrackingDurationFrames = maxTrackingDurationFrames;
        this.alertDurationSeconds = alertDurationSeconds;

        this.deviceWebSocketsServer = deviceWebSocketsServer;

        // Console.WriteLine($"** Debug:");
        // Console.WriteLine($"** Debug: FALL DETECTION PARAMS:");
        // Console.WriteLine($"** Debug: fallingThreshold: {fallingThreshold}");
        // Console.WriteLine($"** Debug: minTrackingDurationFrames: {minTrackingDurationFrames}");
        // Console.WriteLine($"** Debug: maxTrackingDurationFrames: {maxTrackingDurationFrames}");
        // Console.WriteLine($"** Debug: alertDurationSeconds: {alertDurationSeconds}");
        // Console.WriteLine($"** Debug:");

    }

    private void RemoveOldTargets(List<FrameData.TargetHeight> targets)
    {
        List<uint> frameTargetsIds = targets.ConvertAll(target => target.targetId);

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
        //Console.WriteLine($"** Debug: Track-{fallData.TrackId}, Height: {currentHeight:0.00} meters");

        fallData.LastHeights.Enqueue(currentHeight);

        if (fallData.LastHeights.Count > maxTrackingDurationFrames)
            fallData.LastHeights.Dequeue();

        if (fallData.LastHeights.Count < minTrackingDurationFrames)
            return;
        
        float averageHeight = fallData.LastHeights.Average();

        //if (!fallData.fallDetected)
        //    Console.WriteLine($"** Debug: average height: {averageHeight:0.00}, threashold: {fallingThreshold:0.00}");

        if (currentHeight < (fallingThreshold * averageHeight))
        {
            if (fallData.fallDetected == false)
            {
                // fall detected!
                fallData.fallDetected = true;
                fallData.detectionTime = DateTime.UtcNow;

                //Console.WriteLine("****** FALL DETECTED!!!!!! ***********");
            }
        }

        if (fallData.fallDetected && (fallData.detectionTime.AddSeconds(alertDurationSeconds) < DateTime.UtcNow))
        {
            fallData.fallDetected = false;
        }
        

    }

    public void HandleFrame(FrameData frame)
    {
        RemoveOldTargets(frame.TargetsHeightList);

        // add and update fall data according to latest frame
        foreach (var target in frame.TargetsHeightList)
        {
            FallData? fallData = tracksFallData.FirstOrDefault(trackData => trackData.TrackId == target.targetId);

            if (fallData == null)
            {
                // add new entry
                fallData = new FallData();
                fallData.TrackId = target.targetId;
                tracksFallData.Add(fallData);
            }

            // update fall data according to the new target info
            UpdateFallData(fallData, target.maxZ);
        }
        
    }

    public void PublishFallDetectionStatus()
    {
        deviceWebSocketsServer.SendFallDetectionData(tracksFallData);
    }
}