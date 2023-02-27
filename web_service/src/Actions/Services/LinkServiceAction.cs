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
using WebService.Services;

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

public class LinkServiceAction : IAction
{
    private DeviceEntity device;
    private LinkServiceArgs args;

    public LinkServiceAction(DeviceEntity device, LinkServiceArgs args)
    {
        this.device = device;
        this.args = args;
    }

    public void Run()
    {
        var alreadyLinked = device.LinkedServices.Exists(linkedService => linkedService.ServiceId == args.ServiceId);

        if (alreadyLinked)
            throw new Exception($"The service is already linked to this device.");        

        if (!ServiceManager.Instance.ServiceExist(args.ServiceId))
            throw new Exception($"Cannot find service with the provided id.");

        if (!ServiceManager.Instance.IsDeviceSupportedByService(args.ServiceId, device))
            throw new Exception("The requested service does not support the given device");

        var linkedService = new DeviceEntity.LinkedService() 
        {
            ServiceId = args.ServiceId,
            ServiceOptions = args.ServiceOptions
        };

        if (device.State == DeviceEntity.DeviceState.Active)
        {
            ServiceManager.Instance.InitServiceContext(device, linkedService);
        }

        device.LinkedServices.Add(linkedService);
    }
} 