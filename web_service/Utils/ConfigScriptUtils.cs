using WebService.Entites;

namespace WebService.Utils;

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