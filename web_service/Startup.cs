using System;
using System.Threading;
using System.Threading.Tasks;

using WebService.Context;
using WebService.Service;

public class Startup {

    public static Task? mainTask;

    public static void ApplicationStart()
    {
        Console.WriteLine("Loading devices from storage...");
        DeviceContext.Instance.LoadDevicesFromStorage();

        Console.WriteLine("Starting Device Mapper...");
        DeviceMapper.Instance.Start();

        Console.WriteLine("Starting web service thread...");

        mainTask = new Task(() => 
        { 
            while (true)
                Thread.Sleep(10000);                
        });
        
        mainTask.Start();
    }

}