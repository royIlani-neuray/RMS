/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.RadarLogic.IPRadar;

public sealed class ConfigScriptUtils {

    public static void UpdateSensorPosition(List<string> configScript, RadarSettings.SensorPositionParams radarPosition)
    {
        string command = $"sensorPosition {radarPosition.HeightMeters} {radarPosition.AzimuthTiltDegrees} {radarPosition.ElevationTiltDegrees}";

        for (int i = 0; i < configScript.Count; i++)
        {
            if (configScript[i].StartsWith("sensorPosition"))
            {
                configScript[i] = command;
                break;
            }
        }
    }

    public static void UpdateRadarCalibration(List<string> configScript, string? calibrationSettings)
    {
        if (String.IsNullOrWhiteSpace(calibrationSettings))
        {
            calibrationSettings = "0.0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0"; // default
        }

        try
        {
            // basic validation
            List<string> splitValues = calibrationSettings.Trim().Replace("  "," ").Split(' ').ToList();
            splitValues.ConvertAll<float>(val => float.Parse(val));
        }
        catch 
        {
            throw new BadRequestException("Invalid radar calibration values.");
        }

        string command = $"compRangeBiasAndRxChanPhase {calibrationSettings}";

        for (int i = 0; i < configScript.Count; i++)
        {
            if (configScript[i].StartsWith("compRangeBiasAndRxChanPhase"))
            {
                configScript[i] = command;
                break;
            }
        }
    }

    private static void UpdateBoundaryBox(List<string> configScript, string boxType, RadarSettings.BoundaryBoxParams box)
    {
        string command = $"{boxType} {box.X_Min_Meters} {box.X_Max_Meters} {box.Y_Min_Meters} {box.Y_Max_Meters} {box.Z_Min_Meters} {box.Z_Max_Meters}";

        for (int i = 0; i < configScript.Count; i++)
        {
            if (configScript[i].StartsWith(boxType))
            {
                configScript[i] = command;
                break;
            }
        }
    }

    public static void UpdateBoundaryBox(List<string> configScript, RadarSettings.BoundaryBoxParams boundaryBox)
    {
        UpdateBoundaryBox(configScript, "boundaryBox", boundaryBox);
    }

    public static void UpdateStaticBoundaryBox(List<string> configScript, RadarSettings.BoundaryBoxParams staticBoundaryBox)
    {
        UpdateBoundaryBox(configScript, "staticBoundaryBox", staticBoundaryBox);
    }
}