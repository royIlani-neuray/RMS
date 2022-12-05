/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Microsoft.AspNetCore.Mvc;

namespace DeviceEmulator.Controllers;


[ApiController]
[Route("emulator")]
public class EmulatorController : ControllerBase
{
    private readonly ILogger<EmulatorController> _logger;

    public EmulatorController(ILogger<EmulatorController> logger)
    {
        _logger = logger;
    }

    [HttpGet("version")]
    public object GetVersion()
    {
        return new 
        {
            version = EmulatorSettings.Instance.EmulatorVersion
        };
    }

}
