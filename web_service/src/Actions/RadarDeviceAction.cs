using WebService.Context;
using WebService.Entites;

public abstract class RadarDeviceAction : IAction
{
    protected readonly string deviceId;
    public RadarDeviceAction(string deviceId)
    {
        this.deviceId = deviceId;
    }

    protected abstract void RunDeviceAction(RadarDevice radarDevice);

    public void Run()
    {
        var radarDevice = DeviceContext.Instance.GetDevice(deviceId);
        radarDevice.deviceLock.EnterUpgradeableReadLock();

        if (!DeviceContext.Instance.IsRadarDeviceExist(deviceId))
        {
            radarDevice.deviceLock.ExitUpgradeableReadLock();
            throw new NotFoundException($"Cannot find device with id '{deviceId}' in context. action failed.");
        }

        try
        {
            radarDevice.deviceLock.EnterWriteLock();
            RunDeviceAction(radarDevice);
        }
        finally
        {
            if (DeviceContext.Instance.IsRadarDeviceExist(deviceId))
                DeviceContext.Instance.UpdateDevice(radarDevice);
                
            radarDevice.deviceLock.ExitWriteLock();
            radarDevice.deviceLock.ExitUpgradeableReadLock();
        }
    }
}