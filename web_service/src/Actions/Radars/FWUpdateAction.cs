/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;
using WebService.RadarLogic.IPRadar;

namespace WebService.Actions.Radars;

public class FWUpdateAction : IAction {

    private const int DEVICE_BOOT_WAIT_TIME_MS = 20 * 1000;
    private string radarId;
    private byte [] image;

    public FWUpdateAction(string radarId, byte [] image)
    {
        this.radarId = radarId;
        this.image = image;
    }

    public void Run()
    {
        bool enableDevice = false;

        if (RadarContext.Instance.IsRadarExist(radarId))
        {
            var device = RadarContext.Instance.GetRadar(radarId);

            if (device.Enabled)
            {
                var disableRadarAction = new DisableRadarAction(radarId);
                disableRadarAction.Run();
                enableDevice = true;
                Thread.Sleep(DEVICE_BOOT_WAIT_TIME_MS); // wait in order to make sure the device reset is done.
            }
        }

        try
        {
            if (!RadarDeviceMapper.Instance.IsDeviceHasMapping(radarId))
                throw new BadRequestException("The provided device id does not appear in the mapped devices list.");

            var mappedDevice = RadarDeviceMapper.Instance.GetMappedDevice(radarId); 
            
            IPRadarClient client = new IPRadarClient(mappedDevice.ipAddress);
            client.Connect();

            client.UpdateFirmware(image);

            client.Disconnect();
            if (enableDevice)
            {
                Thread.Sleep(DEVICE_BOOT_WAIT_TIME_MS);
            }
        }
        finally
        {
            if (enableDevice)
            {
                var enableRadarAction = new EnableRadarAction(radarId);
                enableRadarAction.Run();
            }
        }
    }

} 