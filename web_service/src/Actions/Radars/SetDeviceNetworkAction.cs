/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.RadarLogic;
using System.Text.Json.Serialization;
using System.Net;

namespace WebService.Actions.Radars;


public class SetDeviceNetworkArgs
{
    [JsonPropertyName("ip")]
    public string ipAddress { get; set; } = String.Empty;

    [JsonPropertyName("subnet")]
    public string subnetMask { get; set; } = String.Empty;

    [JsonPropertyName("gateway")]
    public string gwAddress { get; set; } = String.Empty;

    [JsonPropertyName("static_ip")]
    public bool? staticIP { get; set; }


    public void Validate()
    {
        if (staticIP == null)
            throw new BadRequestException("static IP option not provided");

        if (staticIP == true)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) || !IPAddress.TryParse(ipAddress, out _))
                throw new BadRequestException("invalid IP provided");
            if (string.IsNullOrWhiteSpace(subnetMask) || !IPAddress.TryParse(subnetMask, out _))
                throw new BadRequestException("invalid subnet provided");
            if (string.IsNullOrWhiteSpace(gwAddress) || !IPAddress.TryParse(gwAddress, out _))
                throw new BadRequestException("invalid gateway address provided");
        }
    }
}

public class SetDeviceNetworkAction : IAction {

    private SetDeviceNetworkArgs args;
    private string radarId;

    public SetDeviceNetworkAction(string radarId, SetDeviceNetworkArgs args)
    {
        this.args = args;
        this.radarId = radarId;
    }

    public void Run()
    {
        IPRadarClient.SetDeviceNetwork(radarId, args.ipAddress, args.subnetMask, args.gwAddress, args.staticIP!.Value);
    }

}