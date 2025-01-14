/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
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
using WebService.Services;
using Serilog;

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
                ServiceManager.Instance.InitDeviceServices(camera);               
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
        if (frame.Type != RtspClientSharpCore.RawFrames.FrameType.Video)
        {
            // skip Audio frames.
            return;
        }
        
        if (frame is RtspRawVideo.RawH264IFrame iFrame)
        {
            camera.CameraWebSocket.SendFrameData(new { segment_type = "SPS" , segment_data = Convert.ToBase64String(iFrame.SpsPpsSegment) });
            camera.CameraWebSocket.SendFrameData(new { segment_type = "IDATA" , segment_data = Convert.ToBase64String(iFrame.FrameSegment) });
        }
        if (frame is RtspRawVideo.RawH264PFrame pFrame)
        {
            camera.CameraWebSocket.SendFrameData(new { segment_type = "PDATA" , segment_data = Convert.ToBase64String(pFrame.FrameSegment) });
        }

        ServiceManager.Instance.RunServices(camera, frame);
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
                camera.Log.Debug($"Connecting to camera URL: {camera.RTSPUrl}");
                await rtspClient.ConnectAsync(cancellationTokenSource!.Token);

                camera.SetStatus("The device is active.");
                
                await rtspClient.ReceiveAsync(cancellationTokenSource!.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                camera.Log.Error("Camera stream error", ex);
                throw;
            }
        }
    }
}