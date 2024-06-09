/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.WebSockets;

namespace WebService.Events;

public class RMSEvents : WebSocketServer
{
    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile RMSEvents? instance; 

    public static RMSEvents Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new RMSEvents();
                }
            }

            return instance;
        }
    }

    private RMSEvents()
    {
        StartWorker();
    }

    #endregion

    private void SendEvent(string eventName, object eventData)
    {
        var message = new WebSocketMessage() 
        {
            MessageType = eventName,
            MessageData = eventData
        };

        Enqueue(message);
    }
    
    public void RadarUpdatedEvent(string radarId)
    {
        SendEvent("RADAR_DEVICE_UPDATED", radarId);
    }

    public void RadarAddedEvent(string radarId)
    {
        SendEvent("RADAR_DEVICE_ADDED", radarId);
    }

    public void RadarDeletedEvent(string radarId)
    {
        SendEvent("RADAR_DEVICE_DELETED", radarId);
    }

    public void DeviceMappingUpdatedEvent()
    {
        SendEvent("DEVICE_MAPPING_UPDATED", new Object());
    }

    public void TemplateAddedEvent(string templateId)
    {
        SendEvent("TEMPLATE_ADDED", templateId);
    }

    public void TemplateDeletedEvent(string templateId)
    {
        SendEvent("TEMPLATE_DELETED", templateId);
    }

    public void CameraUpdatedEvent(string cameraId)
    {
        SendEvent("CAMERA_DEVICE_UPDATED", cameraId);
    }

    public void CameraAddedEvent(string cameraId)
    {
        SendEvent("CAMERA_DEVICE_ADDED", cameraId);
    }

    public void CameraDeletedEvent(string cameraId)
    {
        SendEvent("CAMERA_DEVICE_DELETED", cameraId);
    }

    public void RecordingScheduleAddedEvent(string scheduleId)
    {
        SendEvent("RECORDING_SCHEDULE_ADDED", scheduleId);
    }

    public void RecordingScheduleDeletedEvent(string scheduleId)
    {
        SendEvent("RECORDING_SCHEDULE_DELETED", scheduleId);
    }


}