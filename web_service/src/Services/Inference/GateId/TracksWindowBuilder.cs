/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Tracking;

namespace WebService.Services.Inference.GateId;


/// <summary>
///     The TracksWindowBuilder class is used to build a collection of track windows from a series of radar frames.
///     Each track window consists of a list of frame points belonging to a specific track.
///     When a track window contains points from enough frames, it can be used for creating a Gate Id request.
/// </summary>
public class TracksWindowBuilder 
{
    private const byte MAX_TRACK_ID_NUMBER = 252;
    private const int MIN_TARCK_POINTS_IN_VALID_FRAME = 7;
    private const int MAX_INVALID_FRAMES = 10;
    private const int REQUIRED_POINTS_PER_FRAME = 128;

    private class TrackWindow 
    {
        public Stack<List<FrameData.Point>> windowPoints = new Stack<List<FrameData.Point>>();
        public int invalidFramesCount;
    }

    private Dictionary<byte, TrackWindow> tracksWindows;
    private FrameData? lastFrame;

    private int requiredWindowSize;

    public TracksWindowBuilder(int requiredWindowSize)
    {
        tracksWindows = new Dictionary<byte, TrackWindow>(); // Key: track Id , Value: window data
        lastFrame = null;
        this.requiredWindowSize = requiredWindowSize;
    }

    private void CreateNewWindows(List<byte> targetIndexList)
    {
        HashSet<byte> trackIds = new HashSet<byte>(targetIndexList);
        trackIds.RemoveWhere(trackId => trackId > MAX_TRACK_ID_NUMBER);
        foreach (var trackId in trackIds)
        {
            if (!tracksWindows.ContainsKey(trackId))
                tracksWindows.Add(trackId, new TrackWindow());
        }
    }

    private void CheckInvalidFrames()
    {
        List<byte> windowsToRemove = new List<byte>();

        foreach (var trackId in tracksWindows.Keys)
        {
            var window = tracksWindows[trackId];
            var lastFramePoints = window.windowPoints.Peek();

            if (lastFramePoints.Count < MIN_TARCK_POINTS_IN_VALID_FRAME)
            {
                // this frame is invalid (not enough points for this track)
                window.windowPoints.Pop();

                if (window.windowPoints.TryPeek(out lastFramePoints))
                {
                    window.invalidFramesCount++;

                    if (window.invalidFramesCount > MAX_INVALID_FRAMES)
                    {
                        // mark for delete
                        windowsToRemove.Add(trackId);
                        continue;
                    }

                    // in case of a bad frame we duplicate the last frame point list
                    window.windowPoints.Push(lastFramePoints);
                }
            }
        }

        // remove tracks with invalid windows
        foreach (var trackId in windowsToRemove)
        {
            tracksWindows.Remove(trackId);
        }
    }

    public void AddFrame(FrameData frame)
    {
        // since TargetsIndexList in a frame refers to the previous frame, we must save the last frame
        // and build the window based on that point cloud.
        if (lastFrame == null)
        {
            lastFrame = frame;
            return;
        }

        // create windows for new tracks - o(NumOfPoints)
        CreateNewWindows(frame.TargetsIndexList);

        // create a new point list for each window
        foreach (var window in tracksWindows.Values)
        {
            window.windowPoints.Push(new List<FrameData.Point>());
        }

        if ((lastFrame.FrameNumber + 1) == frame.FrameNumber)
        {
            // fill the points in the relevant windows
            for (int index = 0; index < frame.TargetsIndexList.Count; index ++)
            {
                if (frame.TargetsIndexList[index] > MAX_TRACK_ID_NUMBER) // point is not assocaited to a track. 
                    continue;

                byte trackId = frame.TargetsIndexList[index];

                var trackPoint = lastFrame.PointsList[index];
                tracksWindows[trackId].windowPoints.Peek().Add(trackPoint);
            }
        }
        else
        {
            System.Console.WriteLine("Warning: lost frames! ignoring last frame points!");
        }

        // check invalid frames per track
        CheckInvalidFrames();

        lastFrame = frame;
    }

    private void PadFramePointsList(List<FrameData.Point> framePoints)
    {
        // duplicate the existing points until there are enough points in the list

        while (framePoints.Count < REQUIRED_POINTS_PER_FRAME)
        {
            framePoints.AddRange(framePoints);
        }

        framePoints.RemoveRange(REQUIRED_POINTS_PER_FRAME, framePoints.Count - REQUIRED_POINTS_PER_FRAME);
    }

    private GateIdRequest CreateGateIdRequest(byte trackId)
    {
        GateIdRequest request = new GateIdRequest();

        var framesList = tracksWindows[trackId].windowPoints.ToList();
        framesList.Reverse();

        foreach (var framePoints in framesList)
        {
            GateIdRequest.GateIdFrameInput frameInput = new GateIdRequest.GateIdFrameInput();

            if (framePoints.Count > REQUIRED_POINTS_PER_FRAME)
            {
                framePoints.RemoveRange(REQUIRED_POINTS_PER_FRAME, framePoints.Count - REQUIRED_POINTS_PER_FRAME);
            }
            else if (framePoints.Count < REQUIRED_POINTS_PER_FRAME)
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
                // track window is ready, convert it to a Gate Id request format and clear the window.
                
                readyWindows.Add(trackId, CreateGateIdRequest(trackId));
                tracksWindows.Remove(trackId);
            }

            //System.Console.WriteLine($"debug: pulled window for track id: {trackId}");
        }

        return readyWindows;
    }
}