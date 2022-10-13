using System;
using System.Threading;
using System.Threading.Tasks;

using WebService.Context;
using WebService.Radar;
using WebService.Actions.Radar;

public class Startup {

    public static Task? mainTask;

    public static void ApplicationStart()
    {
        Console.WriteLine("Loading devices from storage...");
        DeviceContext.Instance.LoadDevicesFromStorage();

        Console.WriteLine("Starting Device Mapper...");
        DeviceMapper.Instance.SetDeviceRegisteredCallback(DeviceRegisteredAction.OnDeviceRegisteredCallback);
        DeviceMapper.Instance.Start();

        Console.WriteLine("Starting WebSocket reporter...");
        TracksWebsocketReporter.Instance.StartWorker();   
    }

}