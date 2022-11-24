using System.Text.Json;
using WebService.Entites;

namespace WebService.Tracking.Applications;

public class EmulatorStream : ITrackingApplication
{
    private string deviceId;
    private string deviceName;

    public EmulatorStream(string deviceName, string deviceId)
    {
        this.deviceId = deviceId;
        this.deviceName = deviceName;
    }

    public FrameData GetNextFrame(ITrackingApplication.ReadTIData readTIDataFunction)
    {
        FrameData frame;
        byte [] frameSizeBytes = new byte[4];
            
        var len = readTIDataFunction(frameSizeBytes, frameSizeBytes.Length);

        if (len != sizeof(uint))
        {
            System.Console.WriteLine("Error: Failed to get frame size from emulator stream.");
            throw new Exception("Error: Failed to get frame size from emulator stream.");
        }

        uint frameSize = BitConverter.ToUInt32(frameSizeBytes);

        if (frameSize == 0)
        {
            // the emulator is not streaming a recording yet. generate an empty frame
            frame = new FrameData();
        }
        else
        {
            byte [] frameBytes = new byte[frameSize];

            len = readTIDataFunction(frameBytes, frameBytes.Length);
            
            if (len != frameSize)
            {
                System.Console.WriteLine("Error: Failed to get frame data from emulator stream.");
                throw new Exception("Error: Failed to get frame data from emulator stream.");
            }

            string jsonString = System.Text.Encoding.UTF8.GetString(frameBytes);
            frame = JsonSerializer.Deserialize<FrameData>(jsonString)!;
        }

        // override the original device details
        frame.DeviceId = deviceId;
        frame.DeviceName = deviceName;
        frame.Timestamp = DateTime.UtcNow;

        return frame;
    }
}