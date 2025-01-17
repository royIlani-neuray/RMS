/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Microsoft.AspNetCore.Mvc;
using Serilog;
using WebService.Context;
using WebService.Entites;
using WebService.Events;

namespace WebService.Controllers;

[ApiController]
[Route("/ws")]
public class WebSocketController : ControllerBase
{
    [HttpGet("events")]
    public async Task GetEventsWebsocket()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var socketFinishedTcs = new TaskCompletionSource<object>();

            RMSEvents.Instance.AddWebSocketClient(webSocket, socketFinishedTcs); 
            
            // we must wait for the TracksWebsocketReporter to finish processing before returning
            // from this function, otherwise the socket connection will close
            await socketFinishedTcs.Task;
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }    

    [HttpGet("radars/{radarId}")]
    public async Task GetRadarWebsocket(string radarId)
    {
        // Log.Debug($"In get radar device websocket - radar id: {radarId}");

        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            Radar radar = RadarContext.Instance.GetRadar(radarId);

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var socketFinishedTcs = new TaskCompletionSource<object>();

            radar.RadarWebSocket.AddWebSocketClient(webSocket, socketFinishedTcs); 
            
            // we must wait for the TracksWebsocketReporter to finish processing before returning
            // from this function, otherwise the socket connection will close
            await socketFinishedTcs.Task;

            //Log.Debug($"Websocket connection closed - radar device id: {radarId}");
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    [HttpGet("cameras/{cameraId}")]
    public async Task GetCameraWebsocket(string cameraId)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            Camera camera = CameraContext.Instance.GetCamera(cameraId);

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var socketFinishedTcs = new TaskCompletionSource<object>();

            camera.CameraWebSocket.AddWebSocketClient(webSocket, socketFinishedTcs); 
            
            // we must wait for the TracksWebsocketReporter to finish processing before returning
            // from this function, otherwise the socket connection will close
            await socketFinishedTcs.Task;
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}