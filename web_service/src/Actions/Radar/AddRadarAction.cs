/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Entites;
using WebService.Events;
using WebService.Radar;
using WebService.Utils;

namespace WebService.Actions.Radar;

public class AddRadarDeviceArgs 
{
    [JsonPropertyName("device_id")]
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
    AddRadarDeviceArgs args;

    public AddRadarAction(AddRadarDeviceArgs args)
    {
        this.args = args;
    }

    public void Run()
    {
        args.Validate();

        System.Console.WriteLine($"Registering radar device - {args.Id}");

        RadarDevice device = new RadarDevice();
        device.Id = args.Id;
        device.Name = args.Name;
        device.Description = args.Description;
        device.SendTracksReport = args.SendTracksReport;

        if (!String.IsNullOrEmpty(args.TemplateId))
        {
            var template = TemplateContext.Instance.GetTemplate(args.TemplateId);          
            device.ConfigScript = new List<string>(template.ConfigScript);
            ConfigScriptUtils.UpdateSensorPosition(device.ConfigScript, args.RadarPosition!);
            ConfigScriptUtils.UpdateRadarCalibration(device.ConfigScript, args.RadarCalibration);
            RadarConfigParser configParser = new RadarConfigParser(device.ConfigScript);
            device.radarSettings = configParser.GetRadarSettings();  
        }

        try
        {
            device.deviceMapping = DeviceMapper.Instance.GetMappedDevice(device.Id);
        }
        catch {}
        
        RadarContext.Instance.AddDevice(device);

        System.Console.WriteLine($"Radar device registered.");
        
        RMSEvents.Instance.RadarDeviceAddedEvent(device.Id);

        if (args.Enabled)
        {
            Task enableDeviceTask = new Task(() => 
            {
                try
                {
                    var action = new EnableRadarAction(device.Id);
                    action.Run();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error: could not enable radar device! - {ex.Message}");
                }
            });
            enableDeviceTask.Start();
        }
    }
}