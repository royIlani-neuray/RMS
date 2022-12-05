/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
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
using WebService.Tracking;

public class TracksWebsocketReporter : WorkerThread<FrameData>
{
    private const int MAX_QUEUE_CAPACITY = 5;
    private List<(WebSocket, TaskCompletionSource<object>)> WebSocketClientList;

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile TracksWebsocketReporter? instance; 

    public static TracksWebsocketReporter Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new TracksWebsocketReporter();
                }
            }

            return instance;
        }
    }

    private TracksWebsocketReporter() : base(MAX_QUEUE_CAPACITY)
    {
        WebSocketClientList = new List<(WebSocket, TaskCompletionSource<object>)>();
    }

    #endregion

    public void SendReport(FrameData frameData)
    {
        Enqueue(frameData);
    }

    public void AddWebSocketClient(WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs)
    {
        WebSocketClientList.Add((webSocket,socketFinishedTcs));
    }

    private void RemoveClosedSockets()
    {
        for (int i = WebSocketClientList.Count - 1; i >= 0; i--)
        {
            //System.Console.WriteLine($"state: {WebSocketClientList[i].Item1.State}");
            if (WebSocketClientList[i].Item1.State == WebSocketState.Closed)
            {
                System.Console.WriteLine("IN remove socket!!");

                // signal the controller task that it can be released
                WebSocketClientList[i].Item2.SetResult(WebSocketClientList[i].Item1);
                WebSocketClientList.RemoveAt(i);
            }
        }
    }

    protected override void DoWork(FrameData workItem)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(workItem);
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);

            foreach (var webSocket in WebSocketClientList)
            {
                if (webSocket.Item1.State != WebSocketState.Open)
                    continue;
                
                try
                {
                    webSocket.Item1.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Websocket send error: {ex.Message}");
                }
            }

            RemoveClosedSockets();
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Tracks websocket Error: could not send tracks report - {ex.Message}");
        }
    }
}