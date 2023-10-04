/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Utils;
using WebService.RadarLogic.Tracking;

namespace WebService.WebSockets;

public class RadarWebSocketServer : WebSocketServer
{
    private const int MAX_FRAME_RATE_FPS = 10;
    private const int GATE_ID_PREDICTIONS_RATE = 2;
    private const int FALL_DETECTION_SEND_RATE = 5;
    private const int FAN_GESTURES_SEND_RATE = 5;

    private ActionRateLimiter frameRateLimiter;
    private ActionRateLimiter gateIdRateLimiter;
    private ActionRateLimiter fallDetectionRateLimiter;
    private ActionRateLimiter fanGesturesRateLimiter;

    public RadarWebSocketServer()
    {
        frameRateLimiter = new ActionRateLimiter(MAX_FRAME_RATE_FPS);
        gateIdRateLimiter = new ActionRateLimiter(GATE_ID_PREDICTIONS_RATE);
        fallDetectionRateLimiter = new ActionRateLimiter(FALL_DETECTION_SEND_RATE);
        fanGesturesRateLimiter = new ActionRateLimiter(FAN_GESTURES_SEND_RATE);

        StartWorker();
    }

    ~RadarWebSocketServer()
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

    public void SendSmartFanGesturesPredictions(Object predictions)
    {
        var message = new WebSocketMessage() 
        {
            MessageType = "SMART_FAN_GESTURES_PREDICTIONS",
            MessageData = predictions
        };

        fanGesturesRateLimiter.Run(() => Enqueue(message));
    }


    public void SendFallDetectionData(Object fallData)
    {
        var message = new WebSocketMessage() 
        {
            MessageType = "FALL_DETECTION",
            MessageData = fallData
        };

        fallDetectionRateLimiter.Run(() => Enqueue(message));
    }   
}