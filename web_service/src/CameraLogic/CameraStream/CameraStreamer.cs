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

    /*
    // a working sample for writing the stream as an H264 file.

    bool fileCreated = false;
    BinaryWriter? writer;

    private void WriteToFile(ArraySegment<byte> buffer)
    {
        if (!fileCreated)
        {
            writer = new BinaryWriter(File.Open("./data/test.h264", FileMode.Create));
            fileCreated = true;
        }

        writer!.Write(buffer);
        writer.Flush();
    }
    */

    private void RtspClient_FrameReceived(object? sender, RtspClientSharpCore.RawFrames.RawFrame frame)
    {
        if (frame.Type != RtspClientSharpCore.RawFrames.FrameType.Video)
        {
            // skip Audio frames.
            return;
        }
        
        if (frame is RtspRawVideo.RawH264IFrame f1)
        {
            //WriteToFile(f1.SpsPpsSegment);
            //WriteToFile(f1.FrameSegment);

            camera.CameraWebSocket.SendFrameData(new { segment_type = "SPS" , segment_data = Convert.ToBase64String(f1.SpsPpsSegment) });
            camera.CameraWebSocket.SendFrameData(new { segment_type = "IDATA" , segment_data = Convert.ToBase64String(f1.FrameSegment) });
        }
        if (frame is RtspRawVideo.RawH264PFrame f2)
        {
            //WriteToFile(f2.FrameSegment);
            camera.CameraWebSocket.SendFrameData(new { segment_type = "PDATA" , segment_data = Convert.ToBase64String(f2.FrameSegment) });
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
            catch (Exception ex)
            {
                System.Console.WriteLine($"{camera.LogTag} Camera stream error: " + ex.Message);
                throw ex;
            }
        }
    }
}