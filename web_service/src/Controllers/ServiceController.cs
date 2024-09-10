/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Microsoft.AspNetCore.Mvc;
using WebService.Actions.SystemActions;

namespace WebService.Controllers;

[ApiController]
[Route("")]
public class ServiceController : ControllerBase
{
    private readonly ILogger<ServiceController> _logger;

    public ServiceController(ILogger<ServiceController> logger)
    {
        _logger = logger;
    }

    [HttpGet("storage-info")]
    public GetStorageInfoResults GetStorageInfo()
    {
        var action = new GetStorageInfoAction();
        action.Run();
        return action.Results!;
    }

    [HttpGet("system-log")]
    public IActionResult GetSystemLog()
    {
        string logPath = "./data/logs/rms.log"; // TODO: read from config.
        var logContent = Utils.LogFileReader.ReadLastLines(logPath, 5000);
        return Content(logContent, "text/plain");
    }
}
