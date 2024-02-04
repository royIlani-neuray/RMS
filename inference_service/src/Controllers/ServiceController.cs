/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Microsoft.AspNetCore.Mvc;

namespace InferenceService.Controllers;


[ApiController]
[Route("api/service")]
public class ServiceController : ControllerBase
{
    public static string ServiceVersion = string.Empty;
    private readonly ILogger<ServiceController> _logger;

    public ServiceController(ILogger<ServiceController> logger)
    {
        _logger = logger;
    }

    [HttpGet("version")]
    public object GetVersion()
    {
        return new 
        {
            version = ServiceVersion
        };
    }

}
