/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Services;
using System.Text.Json.Serialization;

namespace WebService.Actions.Services;

public class LinkServiceArgs
{
    [JsonPropertyName("service_id")]
    public string ServiceId { get; set; } = String.Empty;

    [JsonPropertyName("service_options")]
    public Dictionary<string,string> ServiceOptions { get; set; } = new Dictionary<string, string>();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ServiceId))
            throw new BadRequestException("service id wasn't provided.");
    }
}

public class LinkServiceAction : RadarAction 
{
    private LinkServiceArgs args;

    public LinkServiceAction(string radarId, LinkServiceArgs args) : base(radarId) 
    {
        this.args = args;
    }

    protected override void RunRadarAction(Radar radar)
    {
        var alreadyLinked = radar.LinkedServices.Exists(linkedService => linkedService.ServiceId == args.ServiceId);

        if (alreadyLinked)
            throw new Exception($"The service is already linked to this radar device.");        

        if (!ServiceManager.Instance.ServiceExist(args.ServiceId))
            throw new Exception($"Cannot find service with the provided id.");

        var linkedService = new Radar.LinkedService() 
        {
            ServiceId = args.ServiceId,
            ServiceOptions = args.ServiceOptions
        };

        if (radar.State == Radar.DeviceState.Active)
        {
            ServiceManager.Instance.InitServiceContext(radar, linkedService);
        }

        radar.LinkedServices.Add(linkedService);
    }
}