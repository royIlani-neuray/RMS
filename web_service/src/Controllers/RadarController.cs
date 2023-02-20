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
[Route("radars")]
public class RadarController : ControllerBase
{
    private readonly ILogger<RadarController> _logger;

    public RadarController(ILogger<RadarController> logger)
    {
        _logger = logger;
    }

    private void ValidateRadarId(string radarId)
    {
        if (string.IsNullOrWhiteSpace(radarId) || !Guid.TryParse(radarId, out _))
            throw new BadRequestException("invalid device id provided.");
    }

    [HttpGet]
    public List<Radar.RadarBrief> GetRadars()
    {
        return RadarContext.Instance.GetRadarsBrief();
    }

    [HttpGet("{radarId}")]
    public Radar GetRadar(string radarId)
    {
        ValidateRadarId(radarId);        
        if (!RadarContext.Instance.IsRadarExist(radarId))
            throw new NotFoundException("There is no radar with the provided id");

        return RadarContext.Instance.GetRadar(radarId);
    }

    [HttpPost]
    public void AddRadar([FromBody] AddRadarArgs args)
    {
        AddRadarAction action = new AddRadarAction(args);
        action.Run();
        return;
    }

    [HttpDelete("{radarId}")]
    public void DeleteRadar(string radarId)
    {        
        ValidateRadarId(radarId); 
        var action = new DeleteRadarAction(radarId);
        action.Run();
    }

    [HttpPost("{radarId}/enable")]
    public void EnableRadar(string radarId)
    {
        ValidateRadarId(radarId);
        var action = new EnableRadarAction(radarId);
        action.Run();
    }

    [HttpPost("{radarId}/disable")]
    public void DisableRadar(string radarId)
    {
        ValidateRadarId(radarId);
        var action = new DisableRadarAction(radarId);
        action.Run();
    }

    [HttpPut("{radarId}/radar-info")]
    public void UpdateRadarInfo(string radarId, [FromBody] UpdateRadarInfoArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new UpdateRadarInfoAction(radarId, args);
        action.Run();        
    }

    [HttpPut("{radarId}/tracks-reports")]
    public void SetTracksReports(string radarId, [FromBody] SetTracksReportsArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new SetTracksReportsAction(radarId, args);
        action.Run();        
    }

    [HttpPut("{radarId}/network")]
    public void SetDeviceNetwork(string radarId, [FromBody] SetDeviceNetworkArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new SetDeviceNetworkAction(radarId, args);
        action.Run();
    }
    
    [HttpPut("{radarId}/device-id")]
    public void SetRadarId(string radarId, [FromBody] SetRadarIdArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new SetRadarIdAction(radarId, args);
        action.Run();
    }

    [HttpPost("{radarId}/config")]
    public void SetRadarConfig(string radarId, [FromBody] SetRadarConfigArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new SetRadarConfigAction(radarId, args);
        action.Run();
    }

    [HttpPost("{radarId}/reset-device-broadcast")]
    public void SendRestBroadcast(string radarId)
    {
        ValidateRadarId(radarId); 
        RadarLogic.IPRadarClient.SendResetBroadcast(radarId);
    }

    [HttpGet("{radarId}/tracks")]
    public HttpTracksReport GetDeviceTracks(string radarId)
    {
        ValidateRadarId(radarId); 
        var radar = RadarContext.Instance.GetRadar(radarId);

        if ((radar.radarTracker != null) && (radar.radarTracker.LastFrameData != null))
        {
            return new HttpTracksReport(radar.radarTracker.LastFrameData);
        }
        else
        {
            return new HttpTracksReport() {
                DeviceId = radar.Id,
                DeviceName = radar.Name
            };
        }
    }

    [HttpPost("{radarId}/fw-update")]
    public async Task<IActionResult> FirmewareUpdate(string radarId)
    {
        using (var ms = new MemoryStream())
        {
            await Request.Body.CopyToAsync(ms);
            byte [] image = new byte[ms.Length];
            ms.Seek(0,SeekOrigin.Begin);
            ms.Read(image, 0, image.Length);

            var action = new FWUpdateAction(radarId, image);
            action.Run();
        }

        return Ok();
    }   

    [HttpPost("{radarId}/tracks-loopback")]
    public void TracksLoopback(string radarId, [FromBody] object data)
    {
        // this API is used for debug

        System.Console.WriteLine("***** Tracks Loopback ******");
        System.Console.WriteLine(data);
        System.Console.WriteLine("****************************");
    }

    [HttpPost("{radarId}/services")]
    public void LinkToService(string radarId, [FromBody] LinkServiceArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        LinkServiceAction action = new LinkServiceAction(radarId, args);
        action.Run();
    }

    [HttpDelete("{radarId}/services/{serviceId}")]
    public void UnlinkService(string radarId, string serviceId)
    {        
        ValidateRadarId(radarId); 
        var action = new UnlinkServiceAction(radarId, serviceId);
        action.Run();
    }

}
