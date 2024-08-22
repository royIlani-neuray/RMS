
using Serilog;
using Utils;

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
    }

    #endregion

    private const string HTTP_CLIENT_NAME = "INFERENCE_SERVICE";
    private const string ENV_VARIABLE_SERVICE_HOST = "INFERENCE_SERVICE_HOST";
    private const string ENV_VARIABLE_SERVICE_PORT = "INFERENCE_SERVICE_PORT";

    private System.Uri GetServiceBaseAddress()
    {
        var port = Environment.GetEnvironmentVariable(ENV_VARIABLE_SERVICE_PORT);
        var host = Environment.GetEnvironmentVariable(ENV_VARIABLE_SERVICE_HOST);

        if (host == null)
        {
            host = "localhost";
            Log.Warning($"Inference Service host not provided. using host - {host}");
        }
        
        if (port == null)
        {
            port = "16502";
            Log.Warning($"Inference Service port not provided. using port - {port}");
        }

        UriBuilder builder = new UriBuilder();
        builder.Host = host;
        builder.Port = int.Parse(port);

        return builder.Uri;
    }

    public void GetHttpClientDetails(out string name, out Uri baseAddress)
    {
        name = HTTP_CLIENT_NAME;
        baseAddress = GetServiceBaseAddress();
    }

    public async Task<string> Predict(string modelName, string requestInput)
    {
        var httpClient = HttpClientFactory.Instance.CreateClient(HTTP_CLIENT_NAME);
        
        var predictRequestArgs = new {
            prediction_input = requestInput
        };

        var response = await httpClient.PostAsJsonAsync($"api/models/{modelName}/predict", predictRequestArgs);
        
        string responseBody = await response.Content.ReadAsStringAsync();
        
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Log.Error($"Inference Service Predict error - status code: {response.StatusCode}");
        }

        return responseBody;
    }

}