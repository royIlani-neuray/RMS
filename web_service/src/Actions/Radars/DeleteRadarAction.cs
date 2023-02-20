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
    public DeleteRadarAction(string radarId) : base(radarId) {}

    protected override void RunRadarAction(Radar radar)
    {
        System.Console.WriteLine($"Deleting radar device - {radar.Id}");

        var disconnectAction = new DisconnectRadarAction(radar);
        disconnectAction.Run();

        RadarContext.Instance.DeleteRadar(radar);

        radar.DeviceWebSocket.CloseServer();

        radar.SetStatus("Device deleted.");

        RMSEvents.Instance.RadarDeletedEvent(radar.Id);
    }
}