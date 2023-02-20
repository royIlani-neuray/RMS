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
using WebService.Actions.Radars;
using WebService.Tracking;
using Microsoft.AspNetCore.Mvc;
using WebService.Actions.Services;

namespace WebService.Controllers;

[ApiController]
[Route("devices")]
public class DeviceController : ControllerBase
{
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(ILogger<DeviceController> logger)
    {
        _logger = logger;
    }

    private void ValidateDeviceId(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || !Guid.TryParse(deviceId, out _))
            throw new BadRequestException("invalid device id provided.");
    }

    [HttpGet]
    public List<RadarDevice.RadarDeviceBrief> GetDevices()
    {
        return RadarContext.Instance.GetDevicesBrief();
    }

    [HttpGet("{deviceId}")]
    public RadarDevice GetRadarDevice(string deviceId)
    {
        ValidateDeviceId(deviceId);        
        if (!RadarContext.Instance.IsRadarDeviceExist(deviceId))
            throw new NotFoundException("There is no device with the provided id");

        return RadarContext.Instance.GetDevice(deviceId);
    }

    [HttpPost]
    public void AddRadarDevice([FromBody] AddRadarDeviceArgs args)
    {
        AddRadarAction action = new AddRadarAction(args);
        action.Run();
        return;
    }

    [HttpDelete("{deviceId}")]
    public void DeleteRadarDevice(string deviceId)
    {        
        ValidateDeviceId(deviceId); 
        var action = new DeleteRadarAction(deviceId);
        action.Run();
    }

    [HttpPost("{deviceId}/enable")]
    public void EnableRadarDevice(string deviceId)
    {
        ValidateDeviceId(deviceId);
        var action = new EnableRadarAction(deviceId);
        action.Run();
    }

    [HttpPost("{deviceId}/disable")]
    public void DisableRadarDevice(string deviceId)
    {
        ValidateDeviceId(deviceId);
        var action = new DisableRadarAction(deviceId);
        action.Run();
    }

    [HttpPut("{deviceId}/radar-info")]
    public void UpdateRadarInfo(string deviceId, [FromBody] UpdateRadarInfoArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        var action = new UpdateRadarInfoAction(deviceId, args);
        action.Run();        
    }

    [HttpPut("{deviceId}/tracks-reports")]
    public void SetTracksReports(string deviceId, [FromBody] SetTracksReportsArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        var action = new SetTracksReportsAction(deviceId, args);
        action.Run();        
    }

    [HttpPut("{deviceId}/network")]
    public void SetDeviceNetwork(string deviceId, [FromBody] SetDeviceNetworkArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        var action = new SetDeviceNetworkAction(deviceId, args);
        action.Run();
    }
    
    [HttpPut("{deviceId}/device-id")]
    public void SetDeviceId(string deviceId, [FromBody] SetDeviceIdArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        var action = new SetDeviceIdAction(deviceId, args);
        action.Run();
    }

    [HttpPost("{deviceId}/config")]
    public void SetRadarConfig(string deviceId, [FromBody] SetRadarConfigArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        var action = new SetRadarConfigAction(deviceId, args);
        action.Run();
    }

    [HttpPost("{deviceId}/reset-device-broadcast")]
    public void SendRestBroadcast(string deviceId)
    {
        ValidateDeviceId(deviceId); 
        RadarLogic.IPRadarClient.SendResetBroadcast(deviceId);
    }

    [HttpGet("{deviceId}/tracks")]
    public HttpTracksReport GetDeviceTracks(string deviceId)
    {
        ValidateDeviceId(deviceId); 
        var radarDevice = RadarContext.Instance.GetDevice(deviceId);

        if ((radarDevice.radarTracker != null) && (radarDevice.radarTracker.LastFrameData != null))
        {
            return new HttpTracksReport(radarDevice.radarTracker.LastFrameData);
        }
        else
        {
            return new HttpTracksReport() {
                DeviceId = radarDevice.Id,
                DeviceName = radarDevice.Name
            };
        }
    }

    [HttpPost("{deviceId}/fw-update")]
    public async Task<IActionResult> FirmewareUpdate(string deviceId)
    {
        using (var ms = new MemoryStream())
        {
            await Request.Body.CopyToAsync(ms);
            byte [] image = new byte[ms.Length];
            ms.Seek(0,SeekOrigin.Begin);
            ms.Read(image, 0, image.Length);

            var action = new FWUpdateAction(deviceId, image);
            action.Run();
        }

        return Ok();
    }   

    [HttpPost("{deviceId}/tracks-loopback")]
    public void TracksLoopback(string deviceId, [FromBody] object data)
    {
        // this API is used for debug

        System.Console.WriteLine("***** Tracks Loopback ******");
        System.Console.WriteLine(data);
        System.Console.WriteLine("****************************");
    }

    [HttpPost("{deviceId}/services")]
    public void LinkToService(string deviceId, [FromBody] LinkServiceArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        LinkServiceAction action = new LinkServiceAction(deviceId, args);
        action.Run();
    }

    [HttpDelete("{deviceId}/services/{serviceId}")]
    public void UnlinkService(string deviceId, string serviceId)
    {        
        ValidateDeviceId(deviceId); 
        var action = new UnlinkServiceAction(deviceId, serviceId);
        action.Run();
    }

}
