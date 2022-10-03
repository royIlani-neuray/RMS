

namespace WebService.Tracking.Applications;

public interface ITrackingApplication 
{
    public delegate int ReadTIData(byte[] dataArray, int size);
    public void GetNextFrame(ReadTIData readTIDataFunction);
}