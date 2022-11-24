using System.IO;
using System.Text.Json;
using System.Text.Encodings;

namespace DeviceEmulator.Recordings;

public class RecordingDataReader 
{
    private Stream stream;
    private BinaryReader binaryReader;

    private float frameRate;

    public float FrameRate { get => frameRate; }
    
    public RecordingDataReader(string dataFilePath)
    {
        stream = new FileStream(dataFilePath, FileMode.Open);
        binaryReader = new BinaryReader(stream);

        frameRate = binaryReader.ReadSingle();

        System.Console.WriteLine($"Recording frame rate: {frameRate} fps");
    }

    public byte[]? GetNextFrame()
    {
        try
        {
            UInt32 frameDataSize = binaryReader.ReadUInt32();
            byte [] frameBytes = binaryReader.ReadBytes((int) frameDataSize); 
            return frameBytes;
        }
        catch (EndOfStreamException)
        {
            return null;
        }
    }

    public void Rewind()
    {
        stream.Seek(4, SeekOrigin.Begin);
    } 
}