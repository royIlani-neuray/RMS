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

public class DeviceDiscoveredAction : RadarAction {

    public DeviceDiscoveredAction(string deviceId) : base(deviceId) {}

    protected override void RunRadarAction(RadarDevice radarDevice)
    {
        // first, update current known info in the radar device entity
        var mappedDevice = RadarDeviceMapper.Instance.GetMappedDevice(radarDevice.Id); 
        radarDevice.deviceMapping = mappedDevice;

        var action = new ConnectRadarAction(radarDevice);
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
            System.Console.WriteLine($"[{deviceId}] The following device is not registerd in the system. ignoring discovery event.");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[{deviceId}] Unexpected error on DeviceDiscoveredAction. error: {ex.Message}");
        }
    }
}