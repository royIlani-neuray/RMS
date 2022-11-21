using WebService.Utils;
using WebService.Tracking;
using System.Text.Json;

namespace WebService.Services.Recording;

public class RecordingContext : WorkerThread<FrameData>, IServiceContext
{
    public IServiceContext.ServiceState State { get; set; }

    public string deviceId;

    private Stream stream;
    private BinaryWriter binaryWriter;


    private const int MAX_QUEUE_CAPACITY = 5;
    
    public RecordingContext(string deviceId, string recordingPath) : base(MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;
        this.deviceId = deviceId;

        stream = new FileStream(recordingPath, FileMode.Create);
        binaryWriter = new BinaryWriter(stream);
    }

    public void RecordFrame(FrameData frameData)
    {
        Enqueue(frameData);
    }

    protected override void DoWork(FrameData workItem)
    {
        if ((workItem.tracksList.Count) == 0 && (workItem.PointsList.Count == 0))
            return;

        string jsonString = JsonSerializer.Serialize(workItem);
        byte [] frameBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
        UInt32 frameBytesSize = (uint) frameBytes.Length;
        binaryWriter.Write(frameBytesSize);
        binaryWriter.Write(frameBytes);
        binaryWriter.Flush();
    }

    ~RecordingContext()
    {
        binaryWriter.Close();
    }
}