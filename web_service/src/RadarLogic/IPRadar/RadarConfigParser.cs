/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Serilog;
using WebService.Entites;

namespace WebService.RadarLogic.IPRadar;

public class RadarConfigParser
{
    private const int LIGHT_SPEED_METER_PER_SEC = 299792458;
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
            ("compRangeBiasAndRxChanPhase", ParseRadarCalibration)
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

            // Log.Debug("Parsing config line: " + key);
            if (TryGetConfigParams(configScript, key, out configParams))
                action(configParams);
        }

        // detection params require 'frameCfg' 'profileCfg' and 'channelCfg'
        List<float> profileConfig, frameConfig, channelConfig;
        if (TryGetConfigParams(configScript, "frameCfg", out frameConfig) && 
            TryGetConfigParams(configScript, "profileCfg", out profileConfig) &&
            TryGetConfigParams(configScript, "channelCfg", out channelConfig))
        {
            ParseDetectionParams(configScript, frameConfig, profileConfig, channelConfig);
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

    private void ParseRadarCalibration(List<float> configParams)
    {
        this.radarSettings!.RadarCalibration = String.Join(" ", configParams);
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

    private int CountBits(uint value)
    {
        int count = 0;
        while (value != 0)
        {
            uint lsb = value & 1;

            if (lsb > 0)
                count++;

            value >>= 1;
        }

        return count;
    }

    private void GetAntennasCount(List<float> channelConfig, out int TxCount, out int RxCount)
    {
        uint rxChannelEn = (uint) channelConfig[0];
        uint txChannelEn = (uint) channelConfig[1];

        TxCount = CountBits(txChannelEn);
        RxCount = CountBits(rxChannelEn);

        // Log.Debug($"TX Count: {TxCount}, RX Count: {RxCount}");
    }

    private void GetDopplerResolution(int chirpCount, double bandwidth, float startFreq, float rampEndTime, float idleTime, float chirpLoops, int txCount, out double dopplerResolution, out double maxVelocity)
    {
        var tc = (idleTime * 1e-6 + rampEndTime * 1e-6) * chirpCount;
        var lda = 3e8 / (startFreq * 1e9);
        maxVelocity = lda / (4 * tc);
        dopplerResolution = lda / (2 * tc * chirpLoops * txCount);
    }

    private double GetMaxRange(float digOutSampleRate, float freqSlopeConst)
    {
        // Log.Debug($"digOutSampleRate: {digOutSampleRate}");
        // Log.Debug($"freqSlopeConst: {freqSlopeConst}");

        var freqSlopeHzPerSec = freqSlopeConst * 1e12;
        return (digOutSampleRate * 1e3 * 0.9 * 3e8) / (2 * freqSlopeHzPerSec);
    }

    private void CalcRangeResolution(int numAdcSamples, float digOutSampleRate, float freqSlopeConst, out double rangeResolution, out double bandwidth)
    {
        double adcSamplePerioduSec = 1000.0 / digOutSampleRate * numAdcSamples;
        bandwidth = freqSlopeConst * adcSamplePerioduSec * 1e6;
        rangeResolution = LIGHT_SPEED_METER_PER_SEC / (2.0 * bandwidth);
    }

    private int GetFrameSizeBytes(float chirpLoops, int txCount, int rxCount, int numAdcSamples)
    {
        int sampleSizeBytes = 4; // hard coded for now
        return (int) (chirpLoops * txCount * numAdcSamples * rxCount * sampleSizeBytes);
    }

    // frameCfg <chirpStartIndex> <chirpEndIndex> <chirpLoops> <numberOfFrames> <framePeriodicity> <triggerSelect> <triggerDelay>
    // profileCfg <profileId> <startFreq> <idleTime> <adcStartTime> <rampEndTime> <txOutPower> <txPhaseShifter> <freqSlopeConst> <txStartTime> <numAdcSamples> <digOutSampleRate> <hpfCornerFreq1> <hpfCornerFreq2> <rxGain>  
    // channelCfg <rxChannelEn> <txChannelEn> <cascading>
    private void ParseDetectionParams(List<string> configScript, List<float> frameConfig,  List<float> profileConfig, List<float> channelConfig)
    {
        int txCount, rxCount;
        double rangeResolution, bandwidth;
        double dopplerResolution, maxVelocity;

        float frameRate = 1000 / frameConfig[4];
        float chirpLoops = frameConfig[2];

        float startFreq = profileConfig[1];
        float idleTime = profileConfig[2];
        float rampEndTime = profileConfig[4];
        float freqSlopeConst = profileConfig[7];
        int numAdcSamples = (int) profileConfig[9];
        float digOutSampleRate = profileConfig[10];

        int chirpCount = configScript.Count(cfgLine => cfgLine.StartsWith("chirpCfg"));

        CalcRangeResolution(numAdcSamples, digOutSampleRate, freqSlopeConst, out rangeResolution, out bandwidth);
        GetAntennasCount(channelConfig, out txCount, out rxCount);
        
        GetDopplerResolution(chirpCount, bandwidth, startFreq, rampEndTime, idleTime, chirpLoops, txCount, out dopplerResolution, out maxVelocity);
        double maxRange = GetMaxRange(digOutSampleRate, freqSlopeConst);
        int frameSize = GetFrameSizeBytes(chirpLoops, txCount, rxCount, numAdcSamples);

        // Log.Debug($"RangeRes: {rangeResolution}");
        // Log.Debug($"DoppplerRes: {dopplerResolution}");
        // Log.Debug($"Max Velocity: {maxVelocity}");
        // Log.Debug($"Max Range: {maxRange}");

        this.radarSettings!.DetectionParams = new RadarSettings.DetectionParameters() {
            FrameRate = frameRate,
            FrameSize = frameSize,
            TxCount = txCount,
            RxCount = rxCount,
            RangeResolution = (float) rangeResolution,
            VelocityResolution = (float) dopplerResolution,
            MaxRange = (float) maxRange,
            MaxVelocity = (float) maxVelocity
        };
    }

    private bool TryGetConfigParams(List<string> configScript, string configKey, out List<float> configParams)
    {
        configParams = new List<float>();
        var line = configScript.FirstOrDefault(cfgLine => cfgLine.ToLower().StartsWith(configKey.ToLower()));

        if (line == null)
            return false;
        
        List<string> splitValues = line.Trim().Replace("  "," ").Split(' ').ToList();
        splitValues.RemoveAt(0); // remove the key
        configParams = splitValues.ConvertAll<float>(val => float.Parse(val));
        return true;
    }

    public RadarSettings? GetRadarSettings()
    {
        return radarSettings;
    }

}