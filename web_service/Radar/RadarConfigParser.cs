using WebService.Entites;

namespace WebService.Radar;

public class RadarConfigParser
{
    private RadarSettings? radarSettings;

    private List<(string, Action<List<float>>)> ParseMethods;

    public RadarConfigParser(List<string> configScript)
    {
        ParseMethods = new List<(string, Action<List<float>>)>() 
        {
            ("sensorPosition", ParseSensorPosition),
            ("boundaryBox", ParseBoundaryBox),
            ("staticBoundaryBox", ParseStaticBoundaryBox),
            ("presenceBoundaryBox", ParsePresenseBoundryBox),
            ("allocationParam", ParseAllocationParams),
            ("gatingParam", ParseGatingParams),
            //("profileCfg", ParseDetectionParams)
        };

        ParseConfig(configScript);
    }

    private void ParseConfig(List<string> configScript)
    {
        List<float> configParams;

        if (configScript.Count == 0)
            return;
        
        this.radarSettings = new RadarSettings();

        foreach (var parseMethod in ParseMethods)
        {
            var key = parseMethod.Item1;
            var action = parseMethod.Item2;

            //System.Console.WriteLine("Parsing config line: " + key);
            if (TryGetConfigParams(configScript, key, out configParams))
                action(configParams);
        }
    }

    private void ParseSensorPosition(List<float> configParams)
    {
        this.radarSettings!.SensorPosition = new RadarSettings.SensorPositionParams() {
            HeightMeters = configParams[0],
            AzimuthTiltDegrees = configParams[1],
            ElevationTiltDegrees = configParams[2],
        };
    }

    private void ParseBoundaryBox(List<float> configParams)
    {
        this.radarSettings!.BoundaryBox = new RadarSettings.BoundaryBoxParams() {
            X_Min_Meters = configParams[0],
            X_Max_Meters = configParams[1],
            Y_Min_Meters = configParams[2],
            Y_Max_Meters = configParams[3],
            Z_Min_Meters = configParams[4],
            Z_Max_Meters = configParams[5]
        };
    }

    private void ParseStaticBoundaryBox(List<float> configParams)
    {
        this.radarSettings!.StaticBoundaryBox = new RadarSettings.BoundaryBoxParams() {
            X_Min_Meters = configParams[0],
            X_Max_Meters = configParams[1],
            Y_Min_Meters = configParams[2],
            Y_Max_Meters = configParams[3],
            Z_Min_Meters = configParams[4],
            Z_Max_Meters = configParams[5]
        };        
    }

    private void ParsePresenseBoundryBox(List<float> configParams)
    {
        this.radarSettings!.PresenceBoundaryBox = new RadarSettings.BoundaryBoxParams() {
            X_Min_Meters = configParams[0],
            X_Max_Meters = configParams[1],
            Y_Min_Meters = configParams[2],
            Y_Max_Meters = configParams[3],
            Z_Min_Meters = configParams[4],
            Z_Max_Meters = configParams[5]
        };        
    }

    private void ParseAllocationParams(List<float> configParams)
    {
        this.radarSettings!.AllocationParams = new RadarSettings.AllocationParameters() {
            SNRThreshold = configParams[0],
            SNRThresholdObscured = configParams[1],
            VelocityThreshold = configParams[2],
            PointsThreshold = configParams[3],
            MaxDistanceThreshold = configParams[4],
            MaxVeloctyThreshold = configParams[5]
        };
    }

    private void ParseGatingParams(List<float> configParams)
    {
        this.radarSettings!.GatingParams = new RadarSettings.GatingParameters() {
            Gain = configParams[0],
            LimitWidth = configParams[1],
            LimitDepth = configParams[2],
            LimitHeight = configParams[3],
            LimitVelocity = configParams[4]
        };
    }

    private void ParseDetectionParams(List<float> configParams)
    {
        
    }

    private bool TryGetConfigParams(List<string> configScript, string configKey, out List<float> configParams)
    {
        configParams = new List<float>();
        var line = configScript.FirstOrDefault(cfgLine => cfgLine.ToLower().StartsWith(configKey.ToLower()));

        if (line == null)
            return false;
        
        List<string> splitValues = line.Trim().Split(' ').ToList();
        splitValues.RemoveAt(0); // remove the key
        configParams = splitValues.ConvertAll<float>(val => float.Parse(val));
        return true;
    }

    public RadarSettings? GetRadarSettings()
    {
        return radarSettings;
    }

}