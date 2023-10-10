/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.RadarLogic.Tracking;
using WebService.Services.Inference.Utils;

namespace WebService.Services.Inference.SmartFanGestures;

public class SmartFanWindowBuilder : TracksWindowBuilder
{
    public SmartFanWindowBuilder(int requiredWindowSize, int windowShiftSize, int numPointsPerFrame, int minRequiredPointsInFrame, int maxInvalidFramesInWindow)
        : base(requiredWindowSize, windowShiftSize, numPointsPerFrame, minRequiredPointsInFrame, maxInvalidFramesInWindow) { }
    

    private SmartFanGestureRequest CreateSmartFanGestureRequest(byte trackId)
    {
        SmartFanGestureRequest request = new SmartFanGestureRequest();

        var framesList = tracksWindows[trackId].windowPoints.ToList();
        framesList.Reverse();

        foreach (var framePoints in framesList)
        {
            SmartFanGestureRequest.FrameInput frameInput = new SmartFanGestureRequest.FrameInput();

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
                // input is in cartesian points at radar body
                frameInput.Azimuth.Add(point.Azimuth);
                frameInput.Elevation.Add(point.Elevation);
                frameInput.Range.Add(point.Range);
                frameInput.Velocity.Add(point.Doppler);
                frameInput.Intensity.Add(point.SNR);
            }

            request.Frames.Add(frameInput);
        }

        return request;
    }

    public Dictionary<byte, SmartFanGestureRequest> PullReadyWindows()
    {
        Dictionary<byte, SmartFanGestureRequest> readyWindows = new Dictionary<byte, SmartFanGestureRequest>();

        foreach (var trackId in tracksWindows.Keys)
        {
            if (tracksWindows[trackId].windowPoints.Count == requiredWindowSize)
            {
                // track window is ready, convert it to a request format
                
                readyWindows.Add(trackId, CreateSmartFanGestureRequest(trackId));
                
                // we want to reuse some of the window frames for the next inference, so instead of clearing it we
                // only remove the first 'windowShiftSize' frames.
                
                Stack<List<FrameData.Point>> reversedStack = new Stack<List<FrameData.Point>>(tracksWindows[trackId].windowPoints);

                for (int i=0; i < windowShiftSize; i++)
                {
                    reversedStack.Pop();
                }

                tracksWindows[trackId].windowPoints = new Stack<List<FrameData.Point>>(reversedStack);
            }
        }

        return readyWindows;
    }
}