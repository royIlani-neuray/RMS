using WebService.Context;
using WebService.Actions;
using WebService.Radar;

namespace WebService.Scheduler;

public class DeviceMappingScheduler : TaskScheduler{

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile DeviceMappingScheduler? instance; 

    public static DeviceMappingScheduler Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new DeviceMappingScheduler();
                }
            }

            return instance;
        }
    }

    private DeviceMappingScheduler() : base(SCHEDULING_PERIOD_MINUTES) {}

    #endregion

    private const int SCHEDULING_PERIOD_MINUTES = 2;

    public override void RunTask()
    {
        // Trigger device mapping
        System.Console.WriteLine("Mapping Scheduler: Triggering device mapping.");

        DeviceMapper.Instance.MapDevices();
    }



}