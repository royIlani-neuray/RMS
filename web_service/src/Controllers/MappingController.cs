/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;
using WebService.Radar;
using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers;

[ApiController]
[Route("device-mapping")]
public class DeviceMappingController : ControllerBase
{
    private readonly ILogger<DeviceMappingController> _logger;

    public DeviceMappingController(ILogger<DeviceMappingController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public List<DeviceMapper.MappedDevice> GetMappedDevices()
    {
        return DeviceMapper.Instance.GetMappedDevices();
    }

    [HttpPost]
    public void TriggerDeviceMapping()
    {
        DeviceMapper.Instance.MapDevices();
    }

}
