using WebService.Entites;
using WebService.Radar;

public class DisconnectRadarAction : IAction 
{
    private RadarDevice radarDevice;

    public DisconnectRadarAction(RadarDevice radarDevice)
    {
        this.radarDevice = radarDevice;
    }

    public void Run()
    {
        //System.Console.WriteLine($"Debug: DisconnectRadarAction - state: {radarDevice.State}, enabled: {radarDevice.Enabled}");

        if (radarDevice.State == RadarDevice.DeviceState.Active)
        {
            System.Console.WriteLine("Stopping tracker...");
            radarDevice.radarTracker!.Stop();
            radarDevice.radarTracker = null;
            radarDevice.State = RadarDevice.DeviceState.Connected;
        }   

        if (radarDevice.State == RadarDevice.DeviceState.Connected)
        {
            System.Console.WriteLine("Disconnecting from radar...");
            if (radarDevice.ipRadarClient!.IsConnected())
            {
                radarDevice.ipRadarClient!.Disconnect();
                radarDevice.ipRadarClient = null;
            }

            radarDevice.State = RadarDevice.DeviceState.Disconnected;
        }

        System.Console.WriteLine($"Disconnected from radar device - {radarDevice.Id}");     
    }

}