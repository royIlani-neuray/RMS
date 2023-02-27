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
using Microsoft.AspNetCore.Mvc;
using WebService.Actions.Cameras;
using WebService.Actions.Services;

namespace WebService.Controllers;

[ApiController]
[Route("cameras")]
public class CameraController : ControllerBase
{
    private readonly ILogger<CameraController> _logger;

    public CameraController(ILogger<CameraController> logger)
    {
        _logger = logger;
    }

    private void ValidateCameraId(string cameraId)
    {
        if (string.IsNullOrWhiteSpace(cameraId) || !Guid.TryParse(cameraId, out _))
            throw new BadRequestException("invalid camera id provided.");
    }

    [HttpGet]
    public List<Camera.CameraBrief> GetCameras()
    {
        return CameraContext.Instance.GetCamerasBrief();
    }

    [HttpGet("{cameraId}")]
    public Camera GetCamera(string cameraId)
    {
        ValidateCameraId(cameraId);        
        if (!CameraContext.Instance.IsCameraExist(cameraId))
            throw new NotFoundException("There is no camera with the provided id");

        return CameraContext.Instance.GetCamera(cameraId);
    }

    [HttpPost]
    public void AddCamera([FromBody] AddCameraArgs args)
    {
        AddCameraAction action = new AddCameraAction(args);
        action.Run();
        return;
    }

    [HttpDelete("{cameraId}")]
    public void DeleteCamera(string cameraId)
    {        
        ValidateCameraId(cameraId); 
        var action = new DeleteCameraAction(cameraId);
        action.Run();
    }

    [HttpPost("{cameraId}/enable")]
    public void EnableCamera(string cameraId)
    {
        ValidateCameraId(cameraId);
        var action = new EnableCameraAction(cameraId);
        action.Run();
    }

    [HttpPost("{cameraId}/disable")]
    public void DisableCamera(string cameraId)
    {
        ValidateCameraId(cameraId);
        var action = new DisableCameraAction(cameraId);
        action.Run();
    }

    [HttpPost("{cameraId}/services")]
    public void LinkToService(string cameraId, [FromBody] LinkServiceArgs args)
    {
        ValidateCameraId(cameraId); 
        args.Validate();
        var action = new LinkCameraServiceAction(cameraId, args);
        action.Run();
    }

    [HttpDelete("{cameraId}/services/{serviceId}")]
    public void UnlinkService(string cameraId, string serviceId)
    {        
        ValidateCameraId(cameraId); 
        var action = new UnlinkCameraServiceAction(cameraId, serviceId);
        action.Run();
    }

    [HttpPost("test-connection")]
    public async Task<TestCameraConnectionResults> TestCameraConnection([FromBody] TestCameraConnectionArgs args)
    {
        var action = new TestCameraConnectionAction(args);
        return await action.Run();
    }

}