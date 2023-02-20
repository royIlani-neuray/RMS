/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using System.Text.Json.Serialization;

namespace WebService.Actions.Radars;

public class SetTracksReportsArgs
{
    [JsonPropertyName("send_tracks_report")]
    public bool? SendTracksReport { get; set; }

    public void Validate()
    { 
        if (SendTracksReport == null)
            throw new HttpRequestException("Missing send_tracks_report argument");        
    }
}

public class SetTracksReportsAction : RadarAction 
{
    private SetTracksReportsArgs args;

    public SetTracksReportsAction(string deviceId, SetTracksReportsArgs args) : base(deviceId) 
    {
        this.args = args;
    }

    protected override void RunRadarAction(Radar radar)
    {
        radar.SendTracksReport = args.SendTracksReport!.Value;
    }

} 