/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.RadarLogic;

namespace WebService.Actions.Radars;

public class DisconnectRadarAction : IAction 
{
    private RadarDevice radarDevice;

    public DisconnectRadarAction(RadarDevice radarDevice)
    {
        this.radarDevice = radarDevice;
    }

    public void Run()
    {
        //System.Console.WriteLine($"Debug: DisconnectRadarAction - state: {radarDevice.State}, enabled: {radarDevice.Enabled}");

        if (radarDevice.State == RadarDevice.DeviceState.Active)
        {
            radarDevice.SetStatus("Stopping tracker...");
            radarDevice.radarTracker!.Stop();
            radarDevice.radarTracker = null;
            radarDevice.SetStatus("Tracker stopped.");
            radarDevice.State = RadarDevice.DeviceState.Connected;
        }   

        if (radarDevice.State == RadarDevice.DeviceState.Connected)
        {
            radarDevice.SetStatus("Disconnecting from the radar device...");
            if (radarDevice.ipRadarClient!.IsConnected())
            {
                radarDevice.ipRadarClient!.Disconnect();
                radarDevice.ipRadarClient = null;
            }

            radarDevice.State = RadarDevice.DeviceState.Disconnected;
            radarDevice.SetStatus("The device is disconnected.");
        }    
    }

}