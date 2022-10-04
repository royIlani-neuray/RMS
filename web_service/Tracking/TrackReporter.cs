using WebService.Utils;

namespace WebService.Tracking;

public class TrackReporter : WorkerThread<string>
{
    private const int MAX_QUEUE_CAPACITY = 5;

    public TrackReporter() : base(MAX_QUEUE_CAPACITY)
    {
    }

    public void SendReport(string report)
    {
        Enqueue(report);
    }

    protected override void DoWork(string workItem)
    {
        try
        {
            
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }
}