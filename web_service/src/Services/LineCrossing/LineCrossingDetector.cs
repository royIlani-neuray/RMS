/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
using WebService.Utils;
using WebService.RadarLogic.Tracking;
using WebService.Entites;

namespace WebService.Services.LineCrossing;


public class LineCrossingDetector
{
    private (double x1, double y1) lineStart;
    private (double x2, double y2) lineEnd;
    private (double x, double y)? previousPosition = null;
    public LineSettings Line { get; set; }

    public LineCrossingDetector(LineSettings line)
    {
        Line = line;
        lineStart = (line.Start.X, line.Start.Y);
        lineEnd = (line.End.X, line.End.Y);
    }

    public string? CheckForCrossing((double x, double y) currentPosition)
    {
        if (previousPosition == null)
        {
            previousPosition = currentPosition;
            return null;
        }

        var previousSign = GetRelativePosition(previousPosition.Value, lineStart, lineEnd);
        var currentSign = GetRelativePosition(currentPosition, lineStart, lineEnd);

        // Check if signs are different and the segments actually intersect
        if (previousSign != currentSign && previousSign != 0 && IsIntersecting(previousPosition.Value, currentPosition))
        {
            previousPosition = currentPosition;
            return currentSign > 0 ? "out" : "in";
        }

        previousPosition = currentPosition;
        return null;
    }

    private int GetRelativePosition((double x, double y) point, (double x1, double y1) lineStart, (double x2, double y2) lineEnd)
    {
        double crossProduct = (lineEnd.x2 - lineStart.x1) * (point.y - lineStart.y1) -
                              (lineEnd.y2 - lineStart.y1) * (point.x - lineStart.x1);

        return crossProduct > 0 ? 1 : (crossProduct < 0 ? -1 : 0);
    }

    private bool IsIntersecting((double x, double y) previousPosition, (double x, double y) currentPosition)
    {
        // Line segment of the track's movement
        double p0_x = previousPosition.x, p0_y = previousPosition.y;
        double p1_x = currentPosition.x, p1_y = currentPosition.y;

        // Line segment we are checking against
        double p2_x = lineStart.x1, p2_y = lineStart.y1;
        double p3_x = lineEnd.x2, p3_y = lineEnd.y2;

        // Check for intersection using vector cross products
        double s1_x = p1_x - p0_x, s1_y = p1_y - p0_y;
        double s2_x = p3_x - p2_x, s2_y = p3_y - p2_y;

        double s = (-s1_y * (p0_x - p2_x) + s1_x * (p0_y - p2_y)) / (-s2_x * s1_y + s1_x * s2_y);
        double t = ( s2_x * (p0_y - p2_y) - s2_y * (p0_x - p2_x)) / (-s2_x * s1_y + s1_x * s2_y);

        // If s and t are within the range [0, 1], then the segments intersect
        return s >= 0 && s <= 1 && t >= 0 && t <= 1;
    }
}

