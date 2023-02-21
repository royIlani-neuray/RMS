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

public sealed class RadarContext : EntityContext<Radar> {

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile RadarContext? instance; 

    public static RadarContext Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new RadarContext();
                }
            }

            return instance;
        }
    }

    private RadarContext() : base(IEntity.EntityTypes.Radar) {}

    #endregion

    public void LoadRadarsFromStorage()
    {
        LoadEntitiesFromStorage(StorageDatabase.RadarStoragePath);

        foreach (var radar in entities.Values)
        {
            radar.State = Radar.DeviceState.Disconnected;
            radar.Status = radar.Enabled ? "The device is disconnected." : "The device is disabled.";
        }
    }

    public bool IsRadarExist(string radarId)
    {
        return IsEntityExist(radarId);
    }

    public Radar GetRadar(string radarId)
    {
        return GetEntity(radarId);
    }

    public void AddRadar(Radar device)
    {
        AddEntity(device);
    }

    public void UpdateRadar(Radar device)
    {
        UpdateEntity(device);
    }

    public void DeleteRadar(Radar device)
    {
        DeleteEntity(device);
    }

    public List<Radar.RadarBrief> GetRadarsBrief()
    {
        return entities.Values.ToList().ConvertAll<Radar.RadarBrief>(device => new Radar.RadarBrief(device));
    }
}