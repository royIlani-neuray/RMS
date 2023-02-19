/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Database;
using WebService.Entites;

namespace WebService.Context;

public sealed class DeviceContext {

    private static Dictionary<string, RadarDevice> devices = new Dictionary<string, RadarDevice>();

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile DeviceContext? instance; 

    public static DeviceContext Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new DeviceContext();
                }
            }

            return instance;
        }
    }

    private DeviceContext() {}

    #endregion

    public void LoadDevicesFromStorage()
    {
        devices = new Dictionary<string, RadarDevice>(RadarDeviceStorage.LoadAllDevices());

        foreach (var device in devices.Values)
        {
            device.State = RadarDevice.DeviceState.Disconnected;
            device.Status = device.Enabled ? "The device is disconnected." : "The device is disabled.";
        }
    }

    public bool IsRadarDeviceExist(string deviceId)
    {
        if (devices.Keys.Contains(deviceId))
            return true;
        
        return false;
    }

    public RadarDevice GetDevice(string deviceId)
    {
        if (!IsRadarDeviceExist(deviceId))
            throw new NotFoundException($"Could not find device in context with id - {deviceId}");

        return devices[deviceId];
    }

    public void AddDevice(RadarDevice device)
    {
        if (IsRadarDeviceExist(device.Id))
            throw new Exception("Cannot add device. Another device with the same ID already exist.");

        RadarDeviceStorage.SaveDevice(device);
        devices.Add(device.Id, device);
    }

    public void UpdateDevice(RadarDevice device)
    {
        if (!IsRadarDeviceExist(device.Id))
            throw new NotFoundException($"Could not find device in context with id - {device.Id}");

        RadarDeviceStorage.SaveDevice(device);
    }

    public void DeleteDevice(RadarDevice device)
    {
        GetDevice(device.Id); // make sure device enlisted

        RadarDeviceStorage.DeleteDevice(device);
        devices.Remove(device.Id);
    }

    public List<RadarDevice.RadarDeviceBrief> GetDevicesBrief()
    {
        return devices.Values.ToList().ConvertAll<RadarDevice.RadarDeviceBrief>(device => new RadarDevice.RadarDeviceBrief(device));
    }
}