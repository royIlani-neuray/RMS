using WebService.Entites;
using WebService.Tracking.Applications;

namespace WebService.Tracking;

public class RadarTracker 
{
    private RadarDevice radarDevice;
    private Task? trackerTask;
    private ITrackingApplication? trackingApp;
    private bool runTracker;
    private TrackReporter trackReporter;

    public RadarTracker(RadarDevice radarDevice)
    {
        this.radarDevice = radarDevice;
        runTracker = false;
        trackReporter = new TrackReporter();
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
                trackReporter.StartWorker();
                ConfigureRadar();
                trackingApp = new TrafficMonitoring();
                TreakingLoop();               
            }
            catch
            {
                // unexpected connection timeout, trigger a disconnect flow
                TriggerDisconnectAction();
            }
            finally
            {
                trackReporter.StopWorker();
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
            System.Console.WriteLine("Getting next frame...");
            trackingApp!.GetNextFrame(radarDevice.ipRadarClient!.ReadTIData);

            trackReporter.SendReport("");
        }

        System.Console.WriteLine("Debug: Tracking loop exited.");
    }
} 