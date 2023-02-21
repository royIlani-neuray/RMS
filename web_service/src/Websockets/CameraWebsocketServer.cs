/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Utils;

namespace WebService.WebSockets;

public class CameraWebSocketServer : WebSocketServer
{
    private const int MAX_FRAME_RATE_FPS = 30;
    private ActionRateLimiter frameRateLimiter;

    public CameraWebSocketServer()
    {
        frameRateLimiter = new ActionRateLimiter(MAX_FRAME_RATE_FPS);
        StartWorker();
    }

    ~CameraWebSocketServer()
    {
        StopWorker();
    }

    public void SendFrameData(object frame)
    {
        var message = new WebSocketMessage() 
        {
            MessageType = "FRAME_DATA",
            MessageData = frame
        };

        // limit the rate since a high frame rate may cause display issues at the web app.
        frameRateLimiter.Run(() => Enqueue(message));
    }

    
}