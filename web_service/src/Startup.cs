using System;
using System.Threading;
using System.Threading.Tasks;

using WebService.Database;
using WebService.Context;
using WebService.Radar;
using WebService.Actions.Radar;
using WebService.Scheduler;
using WebService.Services;

public class Startup 
{
    public static void ApplicationStart(ConfigurationManager config)
    {
        string version = config["RMS_version"]!;

        Console.WriteLine($"Radar Management Service Started - Version: {version}");

        Console.WriteLine($"Running as user: {Environment.UserName}");

        ServiceSettings.Instance.RMSVersion = version;

        Console.WriteLine("Initializing DB...");
        Database.DatabaseInit();

        Console.WriteLine("Loading templates from storage...");
        TemplateContext.Instance.LoadTemplatesFromStorage();

        Console.WriteLine("Loading services...");
        var servicesSettings = config.GetSection("services").Get<List<RadarServiceSettings>>();

        ServiceManager.Instance.InitServices(servicesSettings);

        Console.WriteLine("Loading devices from storage...");
        DeviceContext.Instance.LoadDevicesFromStorage();

        Console.WriteLine("Starting Device Mapper...");
        DeviceMapper.Instance.SetDeviceDiscoveredCallback(DeviceDiscoveredAction.OnDeviceDiscoveredCallback);
        DeviceMapper.Instance.Start();

        Console.WriteLine("Starting WebSocket reporter...");
        TracksWebsocketReporter.Instance.StartWorker();   

        Console.WriteLine("Starting Device Mapping Scheduler...");
        DeviceMappingScheduler.Instance.Start();

        Console.WriteLine("Starting Connection Scheduler...");
        ConnectionScheduler.Instance.Start();
    }

}