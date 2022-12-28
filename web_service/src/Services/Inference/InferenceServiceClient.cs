
using System.Net;

namespace WebService.Services.Inference;

public class InferenceServiceClient 
{
    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile InferenceServiceClient? instance; 

    public static InferenceServiceClient Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new InferenceServiceClient();
                }
            }

            return instance;
        }
    }

    private InferenceServiceClient() 
    {
        httpClient = new HttpClient();
        baseUrl = "localhost:16500";
    }

    #endregion

    private HttpClient httpClient;
    private string baseUrl;


}