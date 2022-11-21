using WebService.Context;
using WebService.Actions.Radar;

namespace WebService.Scheduler;

public class ConnectionScheduler : TaskScheduler{

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile ConnectionScheduler? instance; 

    public static ConnectionScheduler Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new ConnectionScheduler();
                }
            }

            return instance;
        }
    }

    private ConnectionScheduler() : base(SCHEDULING_PERIOD_MINUTES) {}

    #endregion

    private const int SCHEDULING_PERIOD_MINUTES = 2;

    public override void RunTask()
    {
        var devicesList = DeviceContext.Instance.GetDevicesBrief();

        foreach (var deviceInfo in devicesList)
        {
            if (deviceInfo.State == Entites.RadarDevice.DeviceState.Disconnected)
            {
                try
                {
                    var action = new ReconnectAction(deviceInfo.Id);
                    action.Run();
                }
                catch 
                {
                }
            }
        }
    }

}