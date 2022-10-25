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
}