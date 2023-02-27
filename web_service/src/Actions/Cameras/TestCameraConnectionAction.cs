/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using RtspClientSharpCore;
using System.Text.Json.Serialization;

namespace WebService.Actions.Cameras;

public class TestCameraConnectionArgs 
{
    [JsonPropertyName("rtsp_url")]
    public String RTSPUrl { get; set; }

    public TestCameraConnectionArgs()
    {
        RTSPUrl = String.Empty;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(RTSPUrl))
            throw new HttpRequestException("Camera RTSP url not defined");
    }
}

public class TestCameraConnectionResults
{
    [JsonPropertyName("connected")]
    public bool Connected { get; set; } = false;   

    [JsonPropertyName("status_string")]
    public string StatusString { get; set; } = String.Empty;  
}

public class TestCameraConnectionAction
{
    private TestCameraConnectionArgs args;

    public TestCameraConnectionAction(TestCameraConnectionArgs args)
    {
        this.args = args;
    }

    public async Task<TestCameraConnectionResults> Run()
    {
        args.Validate();

        var cameraUri = new Uri(args.RTSPUrl);
        var connectionParameters = new ConnectionParameters(cameraUri);
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        TestCameraConnectionResults results = new TestCameraConnectionResults();

        using (var rtspClient = new RtspClient(connectionParameters))
        {
            try
            {
                Console.WriteLine($"Testing Connection to camera. URL: {args.RTSPUrl}");
                await rtspClient.ConnectAsync(cancellationTokenSource.Token);
                results.Connected = true;
                results.StatusString = "Camera connection successfull.";
            }
            catch (System.Security.Authentication.InvalidCredentialException)
            {
                results.StatusString = "Invalid credentials provided.";
            }
            catch
            {
                results.StatusString = "Connection failed.";
            }
        }

        System.Console.WriteLine(results.StatusString);
        return results;
    }
}