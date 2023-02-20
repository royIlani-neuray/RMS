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
using WebService.Events;

namespace WebService.Actions.Radars;

public class DeleteRadarAction : RadarAction 
{
    public DeleteRadarAction(string deviceId) : base(deviceId) {}

    protected override void RunRadarAction(RadarDevice radarDevice)
    {
        System.Console.WriteLine($"Deleting radar device - {radarDevice.Id}");

        var disconnectAction = new DisconnectRadarAction(radarDevice);
        disconnectAction.Run();

        RadarContext.Instance.DeleteDevice(radarDevice);

        radarDevice.DeviceWebSocket.CloseServer();

        radarDevice.SetStatus("Device deleted.");

        RMSEvents.Instance.RadarDeviceDeletedEvent(radarDevice.Id);
    }
}