using WebService.Entites;
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
            trackingApp = new PeopleTracking();
        }
        else if (appName == "TRAFFIC_MONITORING")
        {
            trackingApp = new TrafficMonitoring();
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

    private void ConfigureRadar()
    {
        Console.WriteLine($"Configuring radar device - {radarDevice.Id}...");

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
            }
        }
    }

    private void TreakingLoop()
    {
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
        }

        System.Console.WriteLine("Debug: Tracking loop exited.");
    }
} 