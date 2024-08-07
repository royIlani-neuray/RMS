/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using System.Text.Json.Serialization;
using WebService.RadarLogic.IPRadar;

namespace WebService.Actions.Radars;


public class SetRMSHostnameArgs
{
    [JsonPropertyName("hostname")]
    public string Hostname { get; set; } = String.Empty;

    public void Validate()
    {
        if (Hostname == null)
            throw new BadRequestException("Hostname not provided");
    }
}

public class SetRMSHostnameAction : RadarAction {

    private SetRMSHostnameArgs args;

    public SetRMSHostnameAction(string deviceId, SetRMSHostnameArgs args) : base(deviceId) 
    {
        this.args = args;
    }

    protected override void RunRadarAction(Radar radar)
    {
        bool doDisconnect = false;
        IPRadarAPI? client = radar.ipRadarAPI;

        Console.WriteLine($"Setting RMS hostname for device - {radar.Id}");
        
        // on local radars we can connect even if the radar is disabled.
        if (client == null && !radar.RadarOnRemoteNetwork)
        {
            if (!RadarDeviceMapper.Instance.IsDeviceHasMapping(radar.Id))
                throw new BadRequestException("The provided device does not appear in the mapped devices list.");

            var mappedDevice = RadarDeviceMapper.Instance.GetMappedDevice(radar.Id); 
            
            client = new IPRadarAPI();
            client.ConnectLocalRadar(mappedDevice.ipAddress);
            doDisconnect = true;
        }
        
        if (client == null || !client.IsConnected())
        {
            throw new BadRequestException("No connection to the radar. operation aborted.");
        }

        try
        {
            System.Console.WriteLine($"Setting RMS hostname to: {args.Hostname}");
            client.SetRMSHostname(args.Hostname);
        }
        catch
        {
            System.Console.WriteLine("Error: failed to set RMS hostname in radar.");
            throw;
        }
        finally
        {
            if (doDisconnect)
            {
                client.Disconnect();
            }
        }
        
    }

}