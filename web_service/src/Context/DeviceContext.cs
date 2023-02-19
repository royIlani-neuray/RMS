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

public sealed class DeviceContext : EntityContext<RadarDevice> {

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

    private DeviceContext() : base(IEntity.EntityTypes.Radar) {}

    #endregion

    public void LoadDevicesFromStorage()
    {
        LoadEntitiesFromStorage(StorageDatabase.RadarStoragePath);

        foreach (var device in entities.Values)
        {
            device.State = RadarDevice.DeviceState.Disconnected;
            device.Status = device.Enabled ? "The device is disconnected." : "The device is disabled.";
        }
    }

    public bool IsRadarDeviceExist(string deviceId)
    {
        return IsEntityExist(deviceId);
    }

    public RadarDevice GetDevice(string deviceId)
    {
        return GetEntity(deviceId);
    }

    public void AddDevice(RadarDevice device)
    {
        AddEntity(device);
    }

    public void UpdateDevice(RadarDevice device)
    {
        UpdateEntity(device);
    }

    public void DeleteDevice(RadarDevice device)
    {
        DeleteEntity(device);
    }

    public List<RadarDevice.RadarDeviceBrief> GetDevicesBrief()
    {
        return entities.Values.ToList().ConvertAll<RadarDevice.RadarDeviceBrief>(device => new RadarDevice.RadarDeviceBrief(device));
    }
}