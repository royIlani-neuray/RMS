using WebService.Entites;
using WebService.Radar;
using WebService.Tracking;

public class ConnectRadarAction : IAction 
{
    private RadarDevice radarDevice;

    public ConnectRadarAction(RadarDevice radarDevice)
    {
        this.radarDevice = radarDevice;
    }

    public void Run()
    {
        //System.Console.WriteLine($"Debug: ConnectRadarAction - state: {radarDevice.State}, enabled: {radarDevice.Enabled}");

        if (!radarDevice.Enabled)
        {
            Console.WriteLine($"device is disabled - {radarDevice.Id}. ignore connect action.");
            return;
        }

        if (radarDevice.State == RadarDevice.DeviceState.Disconnected)
        {
            try
            {
                // Note: radar must be mapped before connection attempt
                var mappedDevice = DeviceMapper.Instance.GetMappedDevice(radarDevice.Id); 
                radarDevice.ipRadarClient = new IPRadarClient(mappedDevice.ipAddress);
                radarDevice.ipRadarClient.Connect();
            }
            catch
            {
                Console.WriteLine($"Error: connection attempt failed for device - {radarDevice.Id}");
            }

            radarDevice.State = RadarDevice.DeviceState.Connected;
            Console.WriteLine($"Successfully connected to device - {radarDevice.Id}");
        }

        if (radarDevice.State == RadarDevice.DeviceState.Connected)
        {
            if (radarDevice.ConfigScript.Count == 0)
            {
                Console.WriteLine($"Error: no connection script defined for device - {radarDevice.Id}");
                return;
            }

            radarDevice.radarTracker = new RadarTracker(radarDevice);
            radarDevice.radarTracker.Start();

            radarDevice.State = RadarDevice.DeviceState.Active;
        }


    }

}