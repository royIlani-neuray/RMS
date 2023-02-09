/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
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
    
    public void RadarDeviceUpdatedEvent(string deviceId)
    {
        SendEvent("RADAR_DEVICE_UPDATED", deviceId);
    }

    public void RadarDeviceAddedEvent(string deviceId)
    {
        SendEvent("RADAR_DEVICE_ADDED", deviceId);
    }

    public void RadarDeviceDeletedEvent(string deviceId)
    {
        SendEvent("RADAR_DEVICE_DELETED", deviceId);
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

}