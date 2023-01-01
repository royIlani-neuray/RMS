/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;

namespace WebService.Actions.Radar;

public class DeleteRadarAction : RadarDeviceAction 
{
    public DeleteRadarAction(string deviceId) : base(deviceId) {}

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        System.Console.WriteLine($"Deleting radar device - {deviceId}");

        var disconnectAction = new DisconnectRadarAction(radarDevice);
        disconnectAction.Run();

        DeviceContext.Instance.DeleteDevice(radarDevice);

        radarDevice.DeviceWebSocket.CloseServer();

        radarDevice.SetStatus("Device deleted.");
    }
}