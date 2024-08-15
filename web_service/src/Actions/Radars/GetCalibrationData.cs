/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.Actions.Radars;

public class GetCalibrationData : RadarAction 
{
    public string Results { get; set; } = String.Empty;
    public GetCalibrationData(string radarId) : base(radarId) {}

    protected override void RunRadarAction(Radar radar)
    {
        if (radar.State != DeviceEntity.DeviceState.Active)
        {
            throw new BadRequestException("In order to get calibration data the radar has to be active.");
        }

        Results = radar.ipRadarAPI!.GetCalibrationData();
    }
}