
using Serilog;
using Utils;

namespace WebService.Services.Inference;

public class ModelsServiceClient 
{
    #region Singleton
    
    private static object singletonLock = new();
    private static volatile ModelsServiceClient? instance; 

    public static ModelsServiceClient Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    instance ??= new ModelsServiceClient();
                }
            }
            
            return instance;
        }
    }

    private ModelsServiceClient() { }

    #endregion

    private const string HTTP_CLIENT_NAME = "MODELS_SERVICE";
    private const string ENV_VARIABLE_SERVICE_HOST = "MODELS_SERVICE_HOST";
    private const string ENV_VARIABLE_SERVICE_PORT = "MODELS_SERVICE_PORT";

    private System.Uri GetServiceBaseAddress()
    {
        var port = Environment.GetEnvironmentVariable(ENV_VARIABLE_SERVICE_PORT);
        var host = Environment.GetEnvironmentVariable(ENV_VARIABLE_SERVICE_HOST);

        if (host == null)
        {
            host = "localhost";
            Log.Warning($"Models Service host not provided. using host - {host}");
        }
        
        if (port == null)
        {
            port = "16504";
            Log.Warning($"Models Service port not provided. using port - {port}");
        }

        var builder = new UriBuilder
        {
            Host = host,
            Port = int.Parse(port)
        };

        return builder.Uri;
    }

    public void GetHttpClientDetails(out string name, out Uri baseAddress)
    {
        name = HTTP_CLIENT_NAME;
        baseAddress = GetServiceBaseAddress();
    }

    public async Task<string> Predict(string modelName, object requestInput)
    {
        var httpClient = HttpClientFactory.Instance.CreateClient(HTTP_CLIENT_NAME);
        
        var response = await httpClient.PostAsJsonAsync($"api/models/{modelName}/predict", requestInput);
        
        string responseBody = await response.Content.ReadAsStringAsync();
        
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Log.Error($"Models Service Predict error - status code: {response.StatusCode}");
        }

        return responseBody;
    }

    public async Task<string> GetModelInfo(string modelName)
    {
        var httpClient = HttpClientFactory.Instance.CreateClient(HTTP_CLIENT_NAME);
        
        var response = await httpClient.GetAsync($"api/models/{modelName}");
        
        string responseBody = await response.Content.ReadAsStringAsync();
        
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Log.Error($"Models Service GetModelInfo error - status code: {response.StatusCode}");
        }

        return responseBody;
    }


}