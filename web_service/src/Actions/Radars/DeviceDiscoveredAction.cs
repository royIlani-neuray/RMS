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

namespace WebService.Actions.Radars;

public class DeviceDiscoveredAction : RadarAction {

    public DeviceDiscoveredAction(string radarId) : base(radarId) {}

    protected override void RunRadarAction(Radar radar)
    {
        // first, update current known info in the radar entity
        var mappedDevice = RadarDeviceMapper.Instance.GetMappedDevice(radar.Id); 
        radar.deviceMapping = mappedDevice;

        var action = new ConnectRadarAction(radar);
        action.Run();
    }

    public static void OnDeviceDiscoveredCallback(string deviceId)
    {
        try
        {
            var action = new DeviceDiscoveredAction(deviceId);
            action.Run();
        }
        catch (NotFoundException)
        {
            System.Console.WriteLine($"[{deviceId}] The following radar is not registerd in the system. ignoring discovery event.");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[{deviceId}] Unexpected error on DeviceDiscoveredAction. error: {ex.Message}");
        }
    }
}