/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Serilog;
using WebService.Actions.Cameras;
using WebService.Actions.Radars;
using WebService.Entites;

namespace WebService.Actions.DeviceGroups;

public class DisableDevicesAction : DeviceGroupAction 
{
    public DisableDevicesAction(string groupId) : base(groupId) {}

    protected override void RunDeviceGroupAction(DeviceGroup group)
    {
        bool hasError = false;

        Log.Information($"Disabling all devices in group - '{group.GroupName}'...");

        group.Radars.ForEach(radar => 
        {
            try
            {
                var disableRadarAction = new DisableRadarAction(radar.Id);
                disableRadarAction.Run();
            }
            catch
            {
                hasError = true;
            }
        });

        group.Cameras.ForEach(camera => 
        {
            try
            {
                var disableCameraAction = new DisableCameraAction(camera.Id);
                disableCameraAction.Run();
            }
            catch
            {
                hasError = true;
            }
        });

        if (hasError)
        {
            throw new WebServiceException($"Disabling all devices in group - '{group.GroupName}' failed.");
        }
    }
}