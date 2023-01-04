/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet("devices/{deviceId}")]
    public async Task GetDeviceWebsocket(string deviceId)
    {
        //System.Console.WriteLine($"In get device websocket - device id: {deviceId}");

        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            RadarDevice device = DeviceContext.Instance.GetDevice(deviceId);

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var socketFinishedTcs = new TaskCompletionSource<object>();

            device.DeviceWebSocket.AddWebSocketClient(webSocket, socketFinishedTcs); 
            
            // we must wait for the TracksWebsocketReporter to finish processing before returning
            // from this function, otherwise the socket connection will close
            await socketFinishedTcs.Task;

            //System.Console.WriteLine($"Websocket connection closed - device id: {deviceId}");
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}