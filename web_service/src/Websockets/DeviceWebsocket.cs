/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Utils;
using WebService.Tracking;

namespace WebService.WebSockets;

public class DeviceWebSocketServer : WebSocketServer
{
    private const int MAX_FRAME_RATE_FPS = 10;
    private const int GATE_ID_PREDICTIONS_RATE = 2;
    private ActionRateLimiter frameRateLimiter;
    private ActionRateLimiter gateIdRateLimiter;

    public DeviceWebSocketServer()
    {
        frameRateLimiter = new ActionRateLimiter(MAX_FRAME_RATE_FPS);
        gateIdRateLimiter = new ActionRateLimiter(GATE_ID_PREDICTIONS_RATE);

        StartWorker();
    }

    ~DeviceWebSocketServer()
    {
        StopWorker();
    }

    public void SendFrameData(FrameData frame)
    {
        var message = new WebSocketMessage() 
        {
            MessageType = "FRAME_DATA",
            MessageData = frame
        };

        // limit the rate since a high frame rate may cause display issues at the web app.
        frameRateLimiter.Run(() => Enqueue(message));
    }

    public void SendGateIdPredictions(Object predictions)
    {
        var message = new WebSocketMessage() 
        {
            MessageType = "GATE_ID_PREDICTIONS",
            MessageData = predictions
        };

        gateIdRateLimiter.Run(() => Enqueue(message));
    }
    
}