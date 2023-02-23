/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.RadarLogic.IPRadar;
using WebService.RadarLogic.Tracking;

namespace WebService.Actions.Radars;

public class ConnectRadarAction : IAction 
{
    private Radar radar;

    public ConnectRadarAction(Radar radar)
    {
        this.radar = radar;
    }

    public void Run()
    {
        //System.Console.WriteLine($"Debug: ConnectRadarAction - state: {radar.State}, enabled: {radar.Enabled}");

        if (!radar.Enabled)
        {
            Console.WriteLine($"[{radar.Id}] radar device is disabled - ignore connect action.");
            return;
        }

        if (radar.State == Radar.DeviceState.Disconnected)
        {
            radar.SetStatus("Connecting to the radar device...");

            try
            {
                RadarDeviceMapper.MappedDevice mappedDevice;

                // Note: radar must be mapped before connection attempt, unless it has static IP.
                if ((radar.deviceMapping != null) && radar.deviceMapping.staticIP)
                {
                    mappedDevice = radar.deviceMapping;
                }
                else
                {
                    mappedDevice = RadarDeviceMapper.Instance.GetMappedDevice(radar.Id); 
                }

                radar.ipRadarClient = new IPRadarClient(mappedDevice.ipAddress);
                radar.ipRadarClient.Connect();
            }
            catch
            {
                radar.SetStatus("Error: connection attempt to the radar failed.");
                return;
            }

            radar.State = Radar.DeviceState.Connected;
            radar.SetStatus("The radar device is connected.");
        }

        if (radar.State == Radar.DeviceState.Connected)
        {
            if (radar.ConfigScript.Count == 0)
            {
                radar.SetStatus("Error: no connection script is assigned.");
                return;
            }

            radar.SetStatus("Starting radar tracker...");
            radar.radarTracker = new RadarTracker(radar);
            radar.radarTracker.Start();

            radar.State = Radar.DeviceState.Active;
        }


    }

}