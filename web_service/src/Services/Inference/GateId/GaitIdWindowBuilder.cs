/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.RadarLogic.Tracking;
using WebService.Services.Inference.Utils;

namespace WebService.Services.Inference.GateId;

public class GaitIdWindowBuilder : TracksWindowBuilder
{
    public GaitIdWindowBuilder(int requiredWindowSize, int windowShiftSize, int numPointsPerFrame, int minRequiredPointsInFrame, int maxInvalidFramesInWindow)
        : base(requiredWindowSize, windowShiftSize, numPointsPerFrame, minRequiredPointsInFrame, maxInvalidFramesInWindow) { }
    
        private GateIdRequest CreateGateIdRequest(byte trackId)
    {
        GateIdRequest request = new GateIdRequest();

        var framesList = tracksWindows[trackId].windowPoints.ToList();
        framesList.Reverse();

        foreach (var framePoints in framesList)
        {
            GateIdRequest.GateIdFrameInput frameInput = new GateIdRequest.GateIdFrameInput();

            if (framePoints.Count > numPointsPerFrame)
            {
                framePoints.RemoveRange(numPointsPerFrame, framePoints.Count - numPointsPerFrame);
            }
            else if (framePoints.Count < numPointsPerFrame)
            {
                PadFramePointsList(framePoints);
            }

            foreach (var point in framePoints)
            {
                frameInput.xAxis.Add(point.PositionX);
                frameInput.yAxis.Add(point.PositionY);
                frameInput.zAxis.Add(point.PositionZ);
                frameInput.Velocity.Add(point.Doppler);
                frameInput.Intensity.Add(point.SNR);
            }

            request.Frames.Add(frameInput);
        }

        return request;
    }

    public Dictionary<byte, GateIdRequest> PullReadyWindows()
    {
        Dictionary<byte, GateIdRequest> readyWindows = new Dictionary<byte, GateIdRequest>();

        foreach (var trackId in tracksWindows.Keys)
        {
            if (tracksWindows[trackId].windowPoints.Count == requiredWindowSize)
            {
                // track window is ready, convert it to a Gate Id request format
                
                readyWindows.Add(trackId, CreateGateIdRequest(trackId));
                
                // we want to reuse some of the window frames for the next inference, so instead of clearing it we
                // only remove the first 'windowShiftSize' frames.
                ShiftTrackWindow(trackId);
            }
        }

        return readyWindows;
    }
}