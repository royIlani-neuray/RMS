/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/

namespace WebService.Utils;

/// <summary>
/// A class that limits the rate at which an action is run.
/// </summary>
public class FrameRateLimiter
{
    private int frameRateFPS;
    private DateTime lastRunTime;

    /// <summary>
    /// Creates a new FrameRateLimiter with the specified frame rate.
    /// </summary>
    /// <param name="frameRate">The number of times per second that the action should be run.</param>
    public FrameRateLimiter(int frameRate)
    {
        frameRateFPS = frameRate;
        lastRunTime = DateTime.Now;
    }

    /// <summary>
    /// Runs the specified action, but only if enough time has passed since the last run.
    /// </summary>
    /// <param name="action">The action to run.</param>
    public void Run(Action action)
    {
        // Check if enough time has passed since the last run
        var currentTime = DateTime.Now;
        var timeSinceLastRun = (currentTime - lastRunTime).TotalMilliseconds;

        if (timeSinceLastRun >= 1000 / frameRateFPS)
        {
            // Enough time has passed, so run the action
            action();
            lastRunTime = currentTime;
        }
    }
}