/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.RadarLogic;
using WebService.Context;
using System.Text.Json.Serialization;

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

        System.Console.WriteLine($"Updating device id. Current id: [{radarId}], New id: [{args.NewRadarId}] ...");    
        IPRadarClient client = new IPRadarClient(mappedDevice.ipAddress);
        client.Connect();

        client.SetDeviceId(radarId, args.NewRadarId);
    
        client.Disconnect();
    }

}