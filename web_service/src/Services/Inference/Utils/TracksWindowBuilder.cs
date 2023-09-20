/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.RadarLogic.Tracking;

namespace WebService.Services.Inference.Utils;


/// <summary>
///     The TracksWindowBuilder class is used to build a collection of track windows from a series of radar frames.
///     Each track window consists of a list of frame points belonging to a specific track.
///     When a track window contains points from enough frames, it can be used for creating an inference request.
/// </summary>
public class TracksWindowBuilder 
{
    protected const byte MAX_TRACK_ID_NUMBER = 252;

    protected class TrackWindow 
    {
        public Stack<List<FrameData.Point>> windowPoints = new Stack<List<FrameData.Point>>();
        public int invalidFramesCount;
    }

    protected Dictionary<byte, TrackWindow> tracksWindows;
    protected int requiredWindowSize;           // number of frames needed in order to form a window (inference sample)
    protected int numPointsPerFrame;            // the number of points in each frame            
    protected int minRequiredPointsInFrame;     // minimum points in frame to be considered valid (will be padded to 'numPointsPerFrame')
    protected int windowShiftSize;              // number of frames to shift (drop) when building the next window
    protected int maxInvalidFramesInWindow;     // maximum invalid frames that are allowed in a valid window 

    private FrameData? lastFrame;

    public TracksWindowBuilder(int requiredWindowSize, int windowShiftSize, int numPointsPerFrame, int minRequiredPointsInFrame, int maxInvalidFramesInWindow)
    {
        tracksWindows = new Dictionary<byte, TrackWindow>(); // Key: track Id , Value: window data
        lastFrame = null;
        this.requiredWindowSize = requiredWindowSize;
        this.windowShiftSize = windowShiftSize;
        this.numPointsPerFrame = numPointsPerFrame;
        this.minRequiredPointsInFrame = minRequiredPointsInFrame;
        this.maxInvalidFramesInWindow = maxInvalidFramesInWindow;

        if (windowShiftSize > requiredWindowSize)
            throw new Exception("Error: window shift size cannot exceed the actual window size");
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

            if (lastFramePoints.Count < minRequiredPointsInFrame)
            {
                // this frame is invalid (not enough points for this track)
                window.windowPoints.Pop();

                if (window.windowPoints.TryPeek(out lastFramePoints))
                {
                    window.invalidFramesCount++;

                    if (window.invalidFramesCount > maxInvalidFramesInWindow)
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

    protected void PadFramePointsList(List<FrameData.Point> framePoints)
    {
        // duplicate the existing points until there are enough points in the list

        while (framePoints.Count < numPointsPerFrame)
        {
            framePoints.AddRange(framePoints);
        }

        framePoints.RemoveRange(numPointsPerFrame, framePoints.Count - numPointsPerFrame);
    }


}