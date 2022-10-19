using WebService.Entites;
using WebService.Radar;

namespace WebService.Actions.Radar;

public class DeviceDiscoveredAction : RadarDeviceAction {

    public DeviceDiscoveredAction(string deviceId) : base(deviceId) {}

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        // first, update current known info in the radar device entity
        var mappedDevice = DeviceMapper.Instance.GetMappedDevice(radarDevice.Id); 
        radarDevice.deviceMapping = mappedDevice;

        var action = new ConnectRadarAction(radarDevice);
        action.Run();
    }

    public static void OnDeviceDiscoveredCallback(string deviceId)
    {
        try
        {
            var action = new DeviceDiscoveredAction(deviceId);
            action.Run();
        }
        catch (NotFoundException)
        {
            System.Console.WriteLine($"The following device is not registerd in the system - {deviceId}. ignoring discovery event.");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Unexpected error on DeviceDiscoveredAction - {deviceId}. error: {ex.Message}");
        }
    }
}