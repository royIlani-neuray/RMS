/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text;
using System.Text.Json;
using WebService.Utils;
using System.Net.WebSockets;

namespace WebService.WebSockets;

public class WebSocketServer : WorkerThread<WebSocketMessage>
{
    private const int MAX_QUEUE_CAPACITY = 20;

    private List<(WebSocket, TaskCompletionSource<object>)> WebSocketClientList;

    public WebSocketServer() : base("WebSocketServer", MAX_QUEUE_CAPACITY)
    {
        WebSocketClientList = new List<(WebSocket, TaskCompletionSource<object>)>();
    }

    ~WebSocketServer()
    {
        CloseServer();
    }

    public void CloseServer()
    {
        StopWorker();

        foreach (var client in WebSocketClientList)
        {
            System.Console.WriteLine("Debug: Forcing websocket client close...");
            var webSocket = client.Item1;
            var clientTCS = client.Item2;
            clientTCS.SetResult(webSocket);
        }

        WebSocketClientList.Clear();
    }

    public void AddWebSocketClient(WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs)
    {
        WebSocketClientList.Add((webSocket,socketFinishedTcs));

        Task.Run(async () =>
        {
            // The only way for a websocket state to update is if we read from it.
            // we run this task on each socket in order to detect when the socket is closed.
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[1]), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        //System.Console.WriteLine("Got Socket close request!");
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                        break;
                    }
                }
            }
            catch 
            {
                // catch exception in case on unexpected connection close.
            }
        });
    }

    protected override async Task DoWork(WebSocketMessage message)
    {
        if (WebSocketClientList.Count == 0)
            return;
        
        try
        {
            string jsonString = JsonSerializer.Serialize(message);
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);

            // send the message to all clients in parallel
            
            var tasks = WebSocketClientList.Select(async client =>
            {
                var webSocket = client.Item1;

                if (webSocket.State != WebSocketState.Open)
                    return;
                
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
            });
            await Task.WhenAll(tasks);
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Websocket server error: could not send object - {ex.Message}");
        }
        finally
        {
            RemoveClosedSockets();
        }
    }

    private void RemoveClosedSockets()
    {
        for (int i = WebSocketClientList.Count - 1; i >= 0; i--)
        {
            var webSocket = WebSocketClientList[i].Item1;

            if (webSocket.State != WebSocketState.Open)
            {
                // signal the controller task that it can be released
                var clientTCS = WebSocketClientList[i].Item2;
                clientTCS.SetResult(webSocket);
                WebSocketClientList.RemoveAt(i);
            }
        }
    }
}