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
            Console.WriteLine($"[{radarDevice.Id}] device is disabled - ignore connect action.");
            return;
        }

        if (radarDevice.State == RadarDevice.DeviceState.Disconnected)
        {
            radarDevice.SetStatus("Connecting to the radar device...");

            try
            {
                DeviceMapper.MappedDevice mappedDevice;

                // Note: radar must be mapped before connection attempt, unless it has static IP.
                if ((radarDevice.deviceMapping != null) && radarDevice.deviceMapping.staticIP)
                {
                    mappedDevice = radarDevice.deviceMapping;
                }
                else
                {
                    mappedDevice = DeviceMapper.Instance.GetMappedDevice(radarDevice.Id); 
                }

                radarDevice.ipRadarClient = new IPRadarClient(mappedDevice.ipAddress);
                radarDevice.ipRadarClient.Connect();
            }
            catch
            {
                radarDevice.SetStatus("Error: connection attempt to the device failed.");
                return;
            }

            radarDevice.State = RadarDevice.DeviceState.Connected;
            radarDevice.SetStatus("The device is connected.");
        }

        if (radarDevice.State == RadarDevice.DeviceState.Connected)
        {
            if (radarDevice.ConfigScript.Count == 0)
            {
                radarDevice.SetStatus("Error: no connection script is assigned to this device.");
                return;
            }

            radarDevice.SetStatus("Starting radar tracker...");
            radarDevice.radarTracker = new RadarTracker(radarDevice);
            radarDevice.radarTracker.Start();

            radarDevice.State = RadarDevice.DeviceState.Active;
        }


    }

}