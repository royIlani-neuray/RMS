using WebService.Entites;
using WebService.Tracking;
using System.Text.Json;

namespace WebService.Services.Recording;

public class RecordingService : IRadarService 
{
    public const string SERVICE_ID = "RADAR_RECORDER";

    public static readonly string StoragePath = "./data/recordings";

    public string ServiceId 
    { 
        get { return SERVICE_ID; }
    }

    public RadarServiceSettings? Settings { get; set; }

    public RecordingService()
    {
        if (!System.IO.Directory.Exists(StoragePath))
        {
            System.Console.WriteLine("Creating device recordings folder.");
            System.IO.Directory.CreateDirectory(StoragePath);
        }
    }

    public IServiceContext CreateServiceContext(RadarDevice device, Dictionary<string,string> serviceOptions)
    {
        System.Console.WriteLine($"[{device.Id}] Creating recording context.");

        string filename = $"{device.Id}_{DateTime.UtcNow.ToString("yyyy_MM_ddTHH_mm_ss")}";
        string recordingPath = System.IO.Path.Combine(StoragePath, $"{filename}.bin");

        string deviceString = JsonSerializer.Serialize(device);
        string configPath = System.IO.Path.Combine(StoragePath, $"{filename}.json");
        File.WriteAllText(configPath, deviceString);

        RecordingContext recordingContext = new RecordingContext(device.Id, recordingPath);
        recordingContext.StartWorker();
        recordingContext.State = IServiceContext.ServiceState.Active;
        return recordingContext;
    }

    public void DisposeServiceContext(IServiceContext serviceContext)
    {
        RecordingContext recordingContext = (RecordingContext) serviceContext;
        System.Console.WriteLine($"[{recordingContext.deviceId}] Disposing recording context.");
        recordingContext.StopWorker();
        recordingContext.State = IServiceContext.ServiceState.Initialized;
    }

    public void HandleFrame(FrameData frame, IServiceContext serviceContext)
    {
        RecordingContext recordingContext = (RecordingContext) serviceContext;
        recordingContext.RecordFrame(frame);
    }
}