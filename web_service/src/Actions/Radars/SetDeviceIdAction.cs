/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.RadarLogic.IPRadar;
using WebService.Context;
using System.Text.Json.Serialization;
using Serilog;

namespace WebService.Actions.Radars;

public class SetRadarIdArgs
{
    [JsonPropertyName("new_radar_id")]
    public string NewRadarId { get; set; } = String.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(NewRadarId))
            throw new BadRequestException("new_radar_id wasn't provided.");

        if (!Guid.TryParse(NewRadarId, out _))
            throw new BadRequestException("new_radar_id must be a valid Guid format.");
    }
}

public class SetRadarIdAction : IAction {

    private SetRadarIdArgs args;
    private string radarId;

    public SetRadarIdAction(string radarId, SetRadarIdArgs args)
    {
        this.args = args;
        this.radarId = radarId;
    }

    public void Run()
    {
        if (RadarDeviceMapper.Instance.IsDeviceHasMapping(args.NewRadarId))
            throw new Exception("The new device id provided is already exist for another device in the network.");

        if (RadarContext.Instance.IsRadarExist(args.NewRadarId))
            throw new Exception("The new device id provided is already registerd in RMS by another device.");

        var mappedDevice = RadarDeviceMapper.Instance.GetMappedDevice(radarId); 

        if (mappedDevice.remoteDevice)
        {
            throw new Exception("Cannot update device id to a device in a remote network.");
        }

        Log.Information($"Updating radar device id. Current id: [{radarId}], New id: [{args.NewRadarId}] ...");    
        IPRadarAPI client = new IPRadarAPI();
        client.ConnectLocalRadar(mappedDevice.ipAddress);

        client.SetDeviceId(radarId, args.NewRadarId);
    
        client.Disconnect();
    }

}