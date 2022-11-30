using WebService.Entites;
using WebService.Context;
using WebService.Radar;

namespace WebService.Actions.Radar;

public class FWUpdateAction : IAction {

    private const int DEVICE_BOOT_WAIT_TIME_MS = 20 * 1000;
    private string deviceId;
    private byte [] image;

    public FWUpdateAction(string deviceId, byte [] image)
    {
        this.deviceId = deviceId;
        this.image = image;
    }

    public void Run()
    {
        bool enableDevice = false;

        if (DeviceContext.Instance.IsRadarDeviceExist(deviceId))
        {
            var device = DeviceContext.Instance.GetDevice(deviceId);

            if (device.Enabled)
            {
                var disableRadarAction = new DisableRadarAction(deviceId);
                disableRadarAction.Run();
                enableDevice = true;
                Thread.Sleep(DEVICE_BOOT_WAIT_TIME_MS); // wait in order to make sure the device reset is done.
            }
        }

        try
        {
            if (!DeviceMapper.Instance.IsDeviceHasMapping(deviceId))
                throw new BadRequestException("The provided device id does not appear in the mapped devices list.");

            var mappedDevice = DeviceMapper.Instance.GetMappedDevice(deviceId); 
            
            IPRadarClient client = new IPRadarClient(mappedDevice.ipAddress);
            client.Connect();

            client.UpdateFirmware(image);

            client.Disconnect();
            if (enableDevice)
            {
                Thread.Sleep(DEVICE_BOOT_WAIT_TIME_MS);
            }
        }
        finally
        {
            if (enableDevice)
            {
                var enableRadarAction = new EnableRadarAction(deviceId);
                enableRadarAction.Run();
            }
        }
    }

} 