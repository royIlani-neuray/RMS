/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using RtspClientSharpCore;
using RtspRawVideo = RtspClientSharpCore.RawFrames.Video;
using WebService.Actions.Cameras;
using WebService.Entites;

namespace WebService.CameraLogic.CameraStream;

public class CameraStreamer 
{
    private Camera camera;
    private Task? streamerTask;
    private bool runStreamer;
    private CancellationTokenSource? cancellationTokenSource;
    
    public CameraStreamer(Camera camera)
    {
        this.camera = camera;
        runStreamer = false;
    }

    private readonly byte[] SpsPpsDelimiter = new byte[] { 0x00, 0x00, 0x00, 0x01 };

    public void TriggerDisconnectAction()
    {
        Task disconnectTask = new Task(() =>
        {
            camera.EntityLock.EnterWriteLock();
            try
            {
                var disconnectAction = new DisconnectCameraAction(camera);
                disconnectAction.Run();
            }
            finally
            {
                camera.EntityLock.ExitWriteLock();
            }
        });

        disconnectTask.Start();
    }

    public void Start()
    {
        if (runStreamer)
        {
            throw new Exception("Error: streamer already started!");
        }

        runStreamer = true;
        cancellationTokenSource = new CancellationTokenSource();

        streamerTask = new Task(async () => 
        {
            try
            {               
                await CameraStreamLoop();               
            }
            catch
            {
                // unexpected error, trigger a disconnect flow
                TriggerDisconnectAction();
            }
        });

        
        streamerTask.Start();
    }

    public void Stop()
    {
        if (!runStreamer)
            return;

        runStreamer = false;
        cancellationTokenSource!.Cancel();

        if (streamerTask == null)
            return;

        streamerTask.Wait();
    }

    private void RtspClient_FrameReceived(object? sender, RtspClientSharpCore.RawFrames.RawFrame frame)
    {   
        /*
        if (frame is RtspRawVideo.RawH264IFrame f1)
        {
            camera.CameraWebSocket.SendFrameData(f1.SpsPpsSegment);
            camera.CameraWebSocket.SendFrameData(f1.FrameSegment);
            System.Console.WriteLine($"I-Frame f1.SpsPpsSegment len : {f1.SpsPpsSegment.Count}, f1.FrameSegment len : {f1.FrameSegment.Count}");
        }
        if (frame is RtspRawVideo.RawH264PFrame f2)
        {
            camera.CameraWebSocket.SendFrameData(f2.FrameSegment);
            System.Console.WriteLine($"P-Frame f2.FrameSegment len : {f2.FrameSegment.Count}");
        }
        */
        //camera.CameraWebSocket.SendFrameData(frame.FrameSegment);

        if (frame is RtspRawVideo.RawH264IFrame f1)
        {
            System.Console.WriteLine($"I-Frame f1.SpsPpsSegment len : {f1.SpsPpsSegment.Count}, f1.FrameSegment len : {f1.FrameSegment.Count}");

            using (var ms = new MemoryStream())
            {
                ms.Write(f1.SpsPpsSegment.Array!, f1.SpsPpsSegment.Offset, f1.SpsPpsSegment.Count);
                ms.Write(SpsPpsDelimiter, 0, SpsPpsDelimiter.Length);
                ms.Write(f1.FrameSegment.Array!, f1.FrameSegment.Offset, f1.FrameSegment.Count);
                ms.Seek(0, SeekOrigin.Begin);

                var base64 = Convert.ToBase64String(ms.ToArray());
                camera.CameraWebSocket.SendFrameData(base64);
            }
        }
        if (frame is RtspRawVideo.RawH264PFrame f2)
        {
            System.Console.WriteLine($"P-Frame f2.FrameSegment len : {f2.FrameSegment.Count}");

            using (var ms = new MemoryStream(frame.FrameSegment.Array!, frame.FrameSegment.Offset, frame.FrameSegment.Count))
            {
                var base64 = Convert.ToBase64String(ms.ToArray());
                camera.CameraWebSocket.SendFrameData(base64);
            }
        }
        



    }

    private async Task CameraStreamLoop()
    {
        var cameraUri = new Uri(camera.RTSPUrl);
        var connectionParameters = new ConnectionParameters(cameraUri);

        using (var rtspClient = new RtspClient(connectionParameters))
        {
            rtspClient.FrameReceived += RtspClient_FrameReceived;

            try
            {
                //Console.WriteLine("Connecting...");
                await rtspClient.ConnectAsync(cancellationTokenSource!.Token);
                //Console.WriteLine("Connected.");
                await rtspClient.ReceiveAsync(cancellationTokenSource!.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}