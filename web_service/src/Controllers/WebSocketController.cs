/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers;

[ApiController]
[Route("")]
public class WebSocketController : ControllerBase
{
    [HttpGet("")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var socketFinishedTcs = new TaskCompletionSource<object>();

            TracksWebsocketReporter.Instance.AddWebSocketClient(webSocket, socketFinishedTcs); 
            
            // we must wait for the TracksWebsocketReporter to finish processing before returning
            // from this function, otherwise the socket connection will close
            await socketFinishedTcs.Task;

            /*
            //System.Console.WriteLine("Got A Socket connection!!!!!");
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            TracksWebsocketReporter.Instance.AddWebSocketClient(socket);  

            //string jsonString = "{ \"test\" : 22222 }";
            //var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            //await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None); 
            Thread.Sleep(10000);
            */
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}