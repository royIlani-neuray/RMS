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
using WebService.RadarLogic;

namespace WebService.Context;

public sealed class CameraContext : EntityContext<Camera> {

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile CameraContext? instance; 

    public static CameraContext Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new CameraContext();
                }
            }

            return instance;
        }
    }

    private CameraContext() : base(IEntity.EntityTypes.Camera) {}

    #endregion

    public void LoadCamerasFromStorage()
    {
        LoadEntitiesFromStorage(StorageDatabase.CameraStoragePath);

        foreach (var camera in entities.Values)
        {
            camera.State = Camera.DeviceState.Disconnected;
            camera.Status = camera.Enabled ? "The device is disconnected." : "The device is disabled.";
        }
    }

    public bool IsCameraExist(string cameraId)
    {
        return IsEntityExist(cameraId);
    }

    public Camera GetCamera(string cameraId)
    {
        return GetEntity(cameraId);
    }

    public void AddCamera(Camera camera)
    {
        AddEntity(camera);
    }

    public void UpdateCamera(Camera camera)
    {
        UpdateEntity(camera);
    }

    public void DeleteCamera(Camera camera)
    {
        DeleteEntity(camera);
    }

    public List<Camera.CameraBrief> GetCamerasBrief()
    {
        return entities.Values.ToList().ConvertAll<Camera.CameraBrief>(camera => new Camera.CameraBrief(camera));
    }
}