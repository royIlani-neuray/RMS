/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Services;
using WebService.Actions.Radars;
using WebService.RadarLogic.Streaming.Applications;

namespace WebService.RadarLogic.Streaming;

public class RadarStreamer 
{
    private Radar radar;
    private Task? streamerTask;
    private IFirmwareApplication? streamingApp;
    private bool runStreamer;
    private TracksHttpReporter tracksHttpReporter;  // TODO: move this feature to a dedicated service.
    
    public FrameData? LastFrameData;

    public RadarStreamer(Radar radar)
    {
        this.radar = radar;
        runStreamer = false;
        tracksHttpReporter = new TracksHttpReporter();
    }

    public void TriggerDisconnectAction()
    {
        Task disconnectTask = new Task(() =>
        {
            radar.EntityLock.EnterWriteLock();
            try
            {
                var disconnectAction = new DisconnectRadarAction(radar);
                disconnectAction.Run();
            }
            finally
            {
                radar.EntityLock.ExitWriteLock();
            }
        });

        disconnectTask.Start();
    }

    private void InitStreamingApp()
    {
        if (radar.DeviceMapping == null)
            throw new Exception("Error: cannot get device application.");

        string appName = radar.DeviceMapping.appName;

        if (appName.StartsWith("PEOPLE_TRACKING") || (appName == "VITAL_SIGNS"))
        {
            if ((radar.radarSettings == null) || (radar.radarSettings.SensorPosition == null))
            {
                throw new Exception($"Error: cannot create streaming app - missing radar position.");
            }

            streamingApp = new PeopleTracking(radar.radarSettings.SensorPosition);
        }
        else if (appName == "LONG_RANGE_TRACKING")
        {
            if ((radar.radarSettings == null) || (radar.radarSettings.SensorPosition == null))
            {
                throw new Exception($"Error: cannot create streaming app - missing radar position.");
            }

            streamingApp = new LongRangeTracking(radar.radarSettings.SensorPosition);
        }
        else if (appName == "OUT_OF_BOX" || appName == "PC_RANGE_DOPPLER")
        {
            if ((radar.radarSettings == null) || (radar.radarSettings.SensorPosition == null))
            {
                throw new Exception($"Error: cannot create streaming app - missing radar position.");
            }

            streamingApp = new OutOfBox(radar.radarSettings.SensorPosition);
        }
        else if (appName == "TRAFFIC_MONITORING")
        {
            streamingApp = new TrafficMonitoring();
        }
        else if (appName == "LEVEL_SENSING")
        {
            streamingApp = new LevelSensing();
        }
        else if (appName == "EMULATOR_APPLICATION")
        {
            streamingApp = new EmulatorStream(radar.Name, radar.Id);
        }
        else throw new Exception($"Error: no streaming app exist for application: {appName}");
    }

    public void Start()
    {
        if (runStreamer)
        {
            throw new Exception("Error: streamer already started!");
        }

        runStreamer = true;

        streamerTask = new Task(() => 
        {
            try
            {
                tracksHttpReporter.StartWorker();
                ServiceManager.Instance.InitDeviceServices(radar);
                ConfigureRadar();
                InitStreamingApp();                
                TreakingLoop();               
            }
            catch (Exception ex)
            {
                radar.Log.Error("RadarTracker task error: ", ex);
                // unexpected connection timeout, trigger a disconnect flow
                TriggerDisconnectAction();
            }
            finally
            {
                tracksHttpReporter.StopWorker();
            } 
        });

        
        streamerTask.Start();
    }

    public void Stop()
    {
        if (!runStreamer)
            return;

        runStreamer = false;

        if (streamerTask == null)
            return;

        streamerTask.Wait();
    }

    private void ConfigureRadar()
    {
        radar.SetStatus($"Configuring the device...");

        foreach (string tiCommand in radar.ConfigScript)
        {
            if (string.IsNullOrWhiteSpace(tiCommand) || tiCommand.StartsWith("%"))
                continue;
            
            radar.Log.Information($"Sending command - {tiCommand}");
            var response = radar.ipRadarAPI!.SendTICommand(tiCommand);
            radar.Log.Information(response);

            if (response != "Done")
            {
                radar.Log.Error($"The command '{tiCommand}' failed - got: {response}");
                throw new Exception("Error: failed to configure the device!");
            }
        }
    }

    private void TreakingLoop()
    {
        radar.SetStatus("The device is active.");

        while (runStreamer)
        {
            FrameData nextFrame;
            //Log.Debug("Getting next frame...");
            
            try
            {
                nextFrame = streamingApp!.GetNextFrame(radar.ipRadarAPI!.ReadTIData);
            }
            catch (System.Exception ex)
            {
                radar.Log.Error("failed getting frame", ex);
                throw;
            }
            
            nextFrame.DeviceId = radar.Id;
            nextFrame.DeviceName = radar.Name;
            LastFrameData = nextFrame;

            // send tracks report over HTTP and Websockets

            if (radar.SendTracksReport)
            {
                tracksHttpReporter.SendReport(LastFrameData);
            }

            radar.RadarWebSocket.SendFrameData(LastFrameData);

            // pass the frame to linked services
            ServiceManager.Instance.RunServices(radar, LastFrameData);
        }

        // Log.Debug("Streaming loop exited.");
    }
} 