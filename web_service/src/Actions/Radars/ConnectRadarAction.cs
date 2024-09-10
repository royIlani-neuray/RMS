/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.RadarLogic.IPRadar;
using WebService.RadarLogic.Streaming;

namespace WebService.Actions.Radars;

public class ConnectRadarAction : IAction 
{
    private Radar radar;

    public ConnectRadarAction(Radar radar)
    {
        this.radar = radar;
    }

    private bool ConnectLocalRadar()
    {
        if (radar.State == Radar.DeviceState.Connected)
            return true;

        if (radar.State == Radar.DeviceState.Disconnected)
        {
            try
            {
                RadarDeviceMapper.MappedDevice mappedDevice;

                // Note: radar must be mapped before connection attempt, unless it has static IP.
                if ((radar.DeviceMapping != null) && radar.DeviceMapping.staticIP)
                {
                    mappedDevice = radar.DeviceMapping;
                }
                else
                {
                    mappedDevice = RadarDeviceMapper.Instance.GetMappedDevice(radar.Id); 
                }

                if (mappedDevice.remoteDevice)
                {
                    radar.Log.Error("radar device is registered as remote network - ignore connect action.");
                    return false;
                }

                radar.SetStatus("Connecting to the radar device...");
                radar.ipRadarAPI = new IPRadarAPI();
                radar.ipRadarAPI.ConnectLocalRadar(mappedDevice.ipAddress);
            }
            catch
            {
                radar.SetStatus("Error: connection attempt to the radar failed.", logAsError: true);
                return false;
            }

            radar.State = Radar.DeviceState.Connected;
            radar.SetStatus("The radar device is connected.");
            return true;
        }

        return false;
    }

    public void InitRemoteRadarPorts()
    {
        if (radar.State == Radar.DeviceState.Disconnected && (radar.ipRadarAPI == null))
        {
            var radarAPI = new IPRadarAPI();
            radarAPI.InitRemoteRadarConnection(radar.Id);
            radar.ipRadarAPI = radarAPI;
        }
    }

    public void ActivateRadar()
    {
        if (radar.State == Radar.DeviceState.Connected)
        {
            if (radar.ConfigScript.Count == 0)
            {
                radar.SetStatus("Error: no connection script is assigned.");
                return;
            }

            radar.SetStatus("Starting radar streamer...");
            radar.radarStreamer = new RadarStreamer(radar);
            radar.radarStreamer.Start();

            radar.State = Radar.DeviceState.Active;
        }
    }

    public void Run()
    {
        // radar.Log.Debug($"ConnectRadarAction - state: {radar.State}, enabled: {radar.Enabled}");

        if (!radar.Enabled)
        {
            radar.Log.Information("radar device is disabled - ignore connect action.");
            return;
        }

        if (radar.RadarOnRemoteNetwork)
        {
            // instead of connecting, just initialize the IPRadarAPI for remote connection.
            // connection will be initiated by the radar.
            InitRemoteRadarPorts();

            // once the radar is connected this action is triggered again, this time we'll activate the radar.
            ActivateRadar();
        }
        else
        {
            bool connected = ConnectLocalRadar();

            if (connected)
            {
                ActivateRadar();
            }
        }


    }

}