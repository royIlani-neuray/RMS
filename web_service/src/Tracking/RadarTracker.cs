using WebService.Entites;
using WebService.Services;
using WebService.Tracking.Applications;

namespace WebService.Tracking;

public class RadarTracker 
{
    private RadarDevice radarDevice;
    private Task? trackerTask;
    private ITrackingApplication? trackingApp;
    private bool runTracker;
    private TracksHttpReporter tracksHttpReporter;
    
    public FrameData? LastFrameData;

    public RadarTracker(RadarDevice radarDevice)
    {
        this.radarDevice = radarDevice;
        runTracker = false;
        tracksHttpReporter = new TracksHttpReporter();
    }

    public void TriggerDisconnectAction()
    {
        Task disconnectTask = new Task(() =>
        {
            radarDevice.deviceLock.EnterWriteLock();
            try
            {
                var disconnectAction = new DisconnectRadarAction(radarDevice);
                disconnectAction.Run();
            }
            finally
            {
                radarDevice.deviceLock.ExitWriteLock();
            }
        });

        disconnectTask.Start();
    }

    private void InitTrackingApp()
    {
        if (radarDevice.deviceMapping == null)
            throw new Exception("Error: cannot get device application.");

        string appName = radarDevice.deviceMapping.appName;

        if (appName == "PEOPLE_TRACKING")
        {
            if ((radarDevice.radarSettings == null) || (radarDevice.radarSettings.SensorPosition == null))
            {
                throw new Exception($"Error: cannot create tracker app - missing radar position.");
            }

            trackingApp = new PeopleTracking(radarDevice.radarSettings.SensorPosition);
        }
        else if (appName == "TRAFFIC_MONITORING")
        {
            trackingApp = new TrafficMonitoring();
        }
        else if (appName == "EMULATOR_APPLICATION")
        {
            trackingApp = new EmulatorStream(radarDevice.Name, radarDevice.Id);
        }
        else throw new Exception($"Error: no tracker exist for application: {appName}");
    }

    public void Start()
    {
        if (runTracker)
        {
            throw new Exception("Error: tracker already started!");
        }

        runTracker = true;

        trackerTask = new Task(() => 
        {
            try
            {
                tracksHttpReporter.StartWorker();
                InitServices();
                ConfigureRadar();
                InitTrackingApp();                
                TreakingLoop();               
            }
            catch
            {
                // unexpected connection timeout, trigger a disconnect flow
                TriggerDisconnectAction();
            }
            finally
            {
                tracksHttpReporter.StopWorker();
            } 
        });

        
        trackerTask.Start();
    }

    public void Stop()
    {
        if (!runTracker)
            return;

        runTracker = false;

        if (trackerTask == null)
            return;

        trackerTask.Wait();
    }

    private void InitServices()
    {
        foreach (var linkedService in radarDevice.LinkedServices)
        {
            try
            {
                ServiceManager.Instance.InitServiceContext(radarDevice, linkedService);
            }
            catch
            {
                System.Console.WriteLine($"[{radarDevice.Id}] Error: could not initialize service context for service: {linkedService.ServiceId}");
                throw;
            }
        }
    }

    private void ConfigureRadar()
    {
        radarDevice.SetStatus("Configuring the device...");

        foreach (string tiCommand in radarDevice.ConfigScript)
        {
            if (tiCommand.StartsWith("%"))
                continue;
            
            Console.WriteLine($"Sending command - {tiCommand}");
            var response = radarDevice.ipRadarClient!.SendTICommand(tiCommand);
            Console.WriteLine(response);

            if (response != "Done")
            {
                Console.WriteLine($"The command '{tiCommand}' failed - got: {response}");
                throw new Exception("Error: failed to configure the device!");
            }
        }
    }

    private void TreakingLoop()
    {
        radarDevice.SetStatus("The device is active.");

        while (runTracker)
        {
            FrameData nextFrame;
            //System.Console.WriteLine("Getting next frame...");
            
            try
            {
                nextFrame = trackingApp!.GetNextFrame(radarDevice.ipRadarClient!.ReadTIData);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error: failed getting frame: {ex.Message}");
                throw;
            }
            
            nextFrame.DeviceId = radarDevice.Id;
            nextFrame.DeviceName = radarDevice.Name;
            LastFrameData = nextFrame;

            // send tracks report over HTTP and Websockets

            if (radarDevice.SendTracksReport)
            {
                tracksHttpReporter.SendReport(LastFrameData);
            }

            TracksWebsocketReporter.Instance.SendReport(LastFrameData);

            // pass the frame to linked services
            ServiceManager.Instance.HandleFrame(LastFrameData, radarDevice.LinkedServices);
        }

        // System.Console.WriteLine("Debug: Tracking loop exited.");
    }
} 