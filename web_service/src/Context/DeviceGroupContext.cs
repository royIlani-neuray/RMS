/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Database;
using WebService.Entites;

namespace WebService.Context;

public sealed class DeviceGroupContext : EntityContext<DeviceGroup> {

    #region Singleton
    
    private static object singletonLock = new();
    private static volatile DeviceGroupContext? instance; 

    public static DeviceGroupContext Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    instance ??= new DeviceGroupContext();
                }
            }

            return instance;
        }
    }

    private DeviceGroupContext() : base(IEntity.EntityTypes.DeviceGroup) {}

    #endregion

    public void LoadDeviceGroupsFromStorage()
    {
        LoadEntitiesFromStorage(StorageDatabase.DeviceGroupStoragePath);
    }

    public bool IsGroupExist(string groupId)
    {
        return IsEntityExist(groupId);
    }

    public DeviceGroup GetDeviceGroup(string groupId)
    {
        return GetEntity(groupId);
    }

    public void AddDeviceGroup(DeviceGroup group)
    {
        AddEntity(group);
    }

    public void UpdateDeviceGroup(DeviceGroup group)
    {
        UpdateEntity(group);
    }

    public void DeleteDeviceGroup(DeviceGroup group)
    {
        DeleteEntity(group);
    }

    public List<DeviceGroup.DeviceGroupBrief> GetDeviceGroupsBrief()
    {
        return entities.Values.ToList().ConvertAll<DeviceGroup.DeviceGroupBrief>(group => new DeviceGroup.DeviceGroupBrief(group));
    }
}