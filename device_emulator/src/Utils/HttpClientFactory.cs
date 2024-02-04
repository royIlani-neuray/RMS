/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
namespace Utils;

public class HttpClientFactory {

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile HttpClientFactory? instance; 

    public static HttpClientFactory Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new HttpClientFactory();
                }
            }

            return instance;
        }
    }

    private HttpClientFactory() 
    {
        serviceCollection = new ServiceCollection();
        ConfigureServices();
        serviceProvider = serviceCollection.BuildServiceProvider();
        httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    }

    #endregion

    private ServiceCollection serviceCollection;
    private ServiceProvider serviceProvider;
    private IHttpClientFactory httpClientFactory;

    private void ConfigureServices()
    {
        // this is the default HttpClient
        serviceCollection.AddHttpClient();

        // add here any named client needed.
        AddNamedClient(HTTP_CLIENT_RMS, "http://localhost:16500");
    }

    public const string HTTP_CLIENT_RMS = "RMS";

    public HttpClient CreateClient()
    {
        return httpClientFactory.CreateClient();
    }

    public HttpClient CreateClient(string name)
    {
        return httpClientFactory.CreateClient(name);
    }

    private void AddNamedClient(string name, string baseAddress)
    {
        serviceCollection.AddHttpClient(name, options => {
            options.BaseAddress = new Uri(baseAddress);
        });
    }
}