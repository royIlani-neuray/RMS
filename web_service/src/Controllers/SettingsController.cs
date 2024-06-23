/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers;

[ApiController]
[Route("settings")]
public class SettingsController : ControllerBase
{
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ILogger<SettingsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("")]
    public ServiceSettings.SettingsData GetSettings()
    {
        return ServiceSettings.Instance.GetSettings();
    }

    [HttpPut("")]
    public void UpdateSettings([FromBody] ServiceSettings.SettingsData settings)
    {
        ServiceSettings.Instance.UpdateSettings(settings);
    }


    public class ReportsUrl 
    {
        [JsonPropertyName("url")]
        public string? URL { get; set; }
    }

    [HttpGet("tracking-report-url")]
    public ReportsUrl GetTrackingReportURL()
    {
        return new ReportsUrl{
            URL = ServiceSettings.Instance.ReportsURL
        };
    }

    [HttpPost("tracking-report-url")]
    public void SetTrackingReportURL([FromBody] ReportsUrl reportsUrl)
    {
         if ((reportsUrl == null) || (reportsUrl.URL == null))
            throw new BadRequestException("missing url.");
        
        if (reportsUrl.URL != String.Empty)
        {
            if (!Uri.IsWellFormedUriString(reportsUrl.URL, UriKind.Absolute))
                throw new BadRequestException("invalid url provided.");
        }

        ServiceSettings.Instance.ReportsURL = reportsUrl.URL!;       
    }


    public class ReportsInterval 
    {
        [JsonPropertyName("interval_seconds")]
        public int? IntervalSeconds { get; set; }
    }

    [HttpGet("reports-interval")]
    public object GetReportsInterval()
    {
        return new ReportsInterval{
            IntervalSeconds = ServiceSettings.Instance.ReportsIntervalSec
        };
    }

    [HttpPost("reports-interval")]
    public void SetReportsInterval([FromBody] ReportsInterval reportsInterval)
    {
        if (reportsInterval.IntervalSeconds == null)
            throw new BadRequestException("missing interval_seconds.");

        ServiceSettings.Instance.ReportsIntervalSec = reportsInterval.IntervalSeconds.Value;
    }

    [HttpGet("version")]
    public object GetVersion()
    {
        return new 
        {
            version = ServiceSettings.Instance.RMSVersion
        };
    }

    [HttpGet("cloud-upload-support")]
    public object GetCloudUploadSupport()
    {
        return new
        {
            support = ServiceSettings.Instance.CloudUploadSupport
        };
    }
}
