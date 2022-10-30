using System.Text;
using System.Text.Json;
using WebService.Utils;

namespace WebService.Tracking;

public class TracksHttpReporter : WorkerThread<FrameData>
{
    private const int MAX_QUEUE_CAPACITY = 5;

    private DateTime LastReportTime;
    private HttpClient httpClient;

    public TracksHttpReporter() : base(MAX_QUEUE_CAPACITY)
    {
        LastReportTime = DateTime.Now;
        httpClient = new HttpClient();
    }

    public void SendReport(FrameData frameData)
    {
        Enqueue(frameData);
    }

    protected override void DoWork(FrameData workItem)
    {
        try
        {
            string reportsURL = ServiceSettings.Instance.ReportsURL;
            int ReportsIntervalSec = ServiceSettings.Instance.ReportsIntervalSec;

            //System.Console.WriteLine($"ReportsURL: {reportsURL}");
            //System.Console.WriteLine($"ReportsIntervalSec: {ReportsIntervalSec}");

            DateTime currentTime = DateTime.Now;
            var timeDiffSeconds = (currentTime - LastReportTime).TotalSeconds;

            if (reportsURL == String.Empty)
            {
                //System.Console.WriteLine("Warning: reports URL is not set. cannot send report.");
                return;
            }

            if (timeDiffSeconds < ReportsIntervalSec)
            {
                // avoid sending since we are still within ReportsIntervalSec
                return;
            }

            if (workItem.tracksList.Count == 0)
            {
                // report only if there is a detection
                return;
            }
            System.Console.WriteLine($"Sening report: - {reportsURL} - {DateTime.Now}");
            LastReportTime = DateTime.Now;

            HttpTracksReport httpReport = new HttpTracksReport(workItem);
            string jsonString = JsonSerializer.Serialize(httpReport);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, reportsURL)
            {
                Content = new StringContent(jsonString, Encoding.UTF8, "application/json")
            };

            httpClient.Send(httpRequest);

        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Error: could not send tracks report - {ex.Message}");
        }
    }
}