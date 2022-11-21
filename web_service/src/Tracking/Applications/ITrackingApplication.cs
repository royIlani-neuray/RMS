using WebService.Tracking;

namespace WebService.Tracking.Applications;

public interface ITrackingApplication 
{
    public delegate int ReadTIData(byte[] dataArray, int size);
    public FrameData GetNextFrame(ReadTIData readTIDataFunction);
}