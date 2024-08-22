/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.Actions.Radars;

public class DisconnectRadarAction : IAction 
{
    private Radar radar;

    public DisconnectRadarAction(Radar radar)
    {
        this.radar = radar;
    }

    public void Run()
    {
        // radar.Log.Debug($"DisconnectRadarAction - state: {radar.State}, enabled: {radar.Enabled}");

        if (radar.State == Radar.DeviceState.Active)
        {
            radar.SetStatus("Stopping tracker...");
            radar.radarTracker!.Stop();
            radar.radarTracker = null;
            radar.SetStatus("Tracker stopped.");
            radar.State = Radar.DeviceState.Connected;
        }   

        if (radar.State == Radar.DeviceState.Connected)
        {
            radar.SetStatus("Disconnecting from the radar device...");
            if (radar.ipRadarAPI!.IsConnected())
            {
                radar.ipRadarAPI!.Disconnect();
                radar.ipRadarAPI = null;
            }

            radar.State = Radar.DeviceState.Disconnected;
            radar.SetStatus("The device is disconnected.");
        }    
    }

}