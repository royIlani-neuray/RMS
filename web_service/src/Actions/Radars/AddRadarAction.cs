/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using Serilog;
using WebService.Context;
using WebService.Entites;
using WebService.Events;
using WebService.RadarLogic.IPRadar;
using WebService.Utils;

namespace WebService.Actions.Radars;

public class AddRadarArgs 
{
    [JsonPropertyName("radar_id")]
    public string Id { get; set; } = String.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = String.Empty;

    [JsonPropertyName("template_id")]
    public string TemplateId { get; set; } = String.Empty;

    [JsonPropertyName("send_tracks_report")]
    public bool SendTracksReport { get; set; } = false;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("remote_network")]
    public bool RadarOnRemoteNetwork { get; set; } = false;

    [JsonPropertyName("radar_position")]
    public RadarSettings.SensorPositionParams? RadarPosition { get; set; }

    [JsonPropertyName("radar_calibration")]
    public string RadarCalibration { get; set; } = String.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new HttpRequestException("Missing radar name.");
        if (string.IsNullOrWhiteSpace(Id))
            throw new HttpRequestException("Missing radar id.");
        if (!Guid.TryParse(Id, out _))
            throw new BadRequestException("invalid device id provided.");            
        if (!String.IsNullOrEmpty(TemplateId) && !Guid.TryParse(TemplateId, out _))
            throw new BadRequestException("invalid template id provided."); 
        if (RadarPosition == null)
            throw new BadRequestException("Radar position must be provided.");
    }
}

public class AddRadarAction : IAction 
{
    AddRadarArgs args;

    public AddRadarAction(AddRadarArgs args)
    {
        this.args = args;
    }

    public void Run()
    {
        args.Validate();

        Log.Information($"Registering radar device - {args.Id}");

        Radar radar = new Radar();
        radar.Id = args.Id;
        radar.Name = args.Name;
        radar.Description = args.Description;
        radar.SendTracksReport = args.SendTracksReport;
        radar.RadarOnRemoteNetwork = args.RadarOnRemoteNetwork;

        if (!String.IsNullOrEmpty(args.TemplateId))
        {
            var template = TemplateContext.Instance.GetTemplate(args.TemplateId);          
            radar.ConfigScript = new List<string>(template.ConfigScript);
            ConfigScriptUtils.UpdateSensorPosition(radar.ConfigScript, args.RadarPosition!);
            ConfigScriptUtils.UpdateRadarCalibration(radar.ConfigScript, args.RadarCalibration);
            RadarConfigParser configParser = new RadarConfigParser(radar.ConfigScript);
            radar.radarSettings = configParser.GetRadarSettings();  
        }

        try
        {
            radar.DeviceMapping = RadarDeviceMapper.Instance.GetMappedDevice(radar.Id);
            radar.RadarOnRemoteNetwork = radar.DeviceMapping.remoteDevice;
        }
        catch {}
        
        RadarContext.Instance.AddRadar(radar);

        radar.Log.Information("Radar device registered.");
        
        RMSEvents.Instance.RadarAddedEvent(radar.Id);

        if (args.Enabled)
        {
            Task enableRadarTask = new Task(() => 
            {
                try
                {
                    var action = new EnableRadarAction(radar.Id);
                    action.Run();
                }
                catch (Exception ex)
                {
                    radar.Log.Error("could not enable radar device!", ex);
                }
            });
            enableRadarTask.Start();
        }
    }
}