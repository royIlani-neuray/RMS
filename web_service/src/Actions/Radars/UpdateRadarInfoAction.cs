/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Entites;
using WebService.RadarLogic;

namespace WebService.Actions.Radars;

public class UpdateRadarInfoArgs
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = String.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new HttpRequestException("Missing radar name.");           
    }
}

public class UpdateRadarInfoAction : RadarAction 
{
    private UpdateRadarInfoArgs args;

    public UpdateRadarInfoAction(string deviceId, UpdateRadarInfoArgs args) : base(deviceId) 
    {
        this.args = args;
    }

    protected override void RunRadarAction(Radar radar)
    {
        System.Console.WriteLine($"Updating radar info - {radar.Id}");

        radar.Name = args.Name;
        radar.Description = args.Description;
    }
}