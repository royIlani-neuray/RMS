/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.RadarLogic.IPRadar;
using WebService.Context;
using WebService.Utils;
using System.Text.Json.Serialization;
using WebService.Actions.Radars;


namespace WebService.Actions.Radars;

public class SetRadarConfigArgs
{
    [JsonPropertyName("config")]
    public List<string>? Config { get; set; }

    [JsonPropertyName("template_id")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("sensor_position")]
    public RadarSettings.SensorPositionParams? SensorPosition {get; set;} = null;

    [JsonPropertyName("radar_calibration")]
    public string RadarCalibration { get; set; } = String.Empty;

    [JsonPropertyName("boundary_box")]
    public RadarSettings.BoundaryBoxParams? BoundaryBox {get; set;} = null;

    [JsonPropertyName("static_boundary_box")]
    public RadarSettings.BoundaryBoxParams? StaticBoundaryBox {get; set;} = null;


    public void Validate()
    {
        if ((Config == null) && (TemplateId == null))
            throw new BadRequestException("missing radar configuration / template id");

        if (TemplateId != null)
        {
            if (SensorPosition == null)
                throw new BadRequestException("sensor_position must be provided with radar config");

            if (BoundaryBox == null)
                throw new BadRequestException("boundary_box must be provided with radar config");

            if (StaticBoundaryBox == null)
                throw new BadRequestException("static_boundary_box must be provided with radar config");
        }
        
    }
}

public class SetRadarConfigAction : RadarAction 
{
    private SetRadarConfigArgs args;
    public SetRadarConfigAction(string deviceId, SetRadarConfigArgs args) : base(deviceId) 
    {
        this.args = args;
    }

    protected override void RunRadarAction(Radar radar)
    {
        List<string> configScript;
        Console.WriteLine($"Setting radar config for device - {radar.Id}");

        if (!string.IsNullOrWhiteSpace(args.TemplateId))
        {
            var template = TemplateContext.Instance.GetTemplate(args.TemplateId);
            configScript = new List<string>(template.ConfigScript);
            ConfigScriptUtils.UpdateSensorPosition(configScript, args.SensorPosition!);
            ConfigScriptUtils.UpdateRadarCalibration(configScript, args.RadarCalibration);
            ConfigScriptUtils.UpdateBoundaryBox(configScript,args.BoundaryBox!);
            ConfigScriptUtils.UpdateStaticBoundaryBox(configScript,args.StaticBoundaryBox!);
        }
        else
        {
            configScript = args.Config!;
        }

        try
        {
            RadarConfigParser configParser = new RadarConfigParser(configScript);
            radar.radarSettings = configParser.GetRadarSettings();
        }
        catch
        {
            throw new Exception($"Error: could not parse config script for device - {radar.Id}. make sure the config is valid.");
        }

        radar.ConfigScript = configScript;

        var disconnectAction = new DisconnectRadarAction(radar);
        disconnectAction.Run();

        var connectAction = new ConnectRadarAction(radar);
        connectAction.Run(); 
    }
}