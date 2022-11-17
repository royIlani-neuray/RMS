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
            radarDevice.SetStatus("Stopping tracker...");
            radarDevice.radarTracker!.Stop();
            radarDevice.radarTracker = null;
            radarDevice.SetStatus("Tracker stopped.");
            radarDevice.State = RadarDevice.DeviceState.Connected;
        }   

        if (radarDevice.State == RadarDevice.DeviceState.Connected)
        {
            radarDevice.SetStatus("Disconnecting from the radar device...");
            if (radarDevice.ipRadarClient!.IsConnected())
            {
                radarDevice.ipRadarClient!.Disconnect();
                radarDevice.ipRadarClient = null;
            }

            radarDevice.State = RadarDevice.DeviceState.Disconnected;
            radarDevice.SetStatus("The device is disconnected.");
        }    
    }

}