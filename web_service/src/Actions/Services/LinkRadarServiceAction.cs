/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.Actions.Services;

public class LinkRadarServiceAction : RadarAction 
{
    private LinkServiceArgs args;

    public LinkRadarServiceAction(string radarId, LinkServiceArgs args) : base(radarId) 
    {
        this.args = args;
    }

    protected override void RunRadarAction(Radar radar)
    {
        var linkServiceAction = new LinkServiceAction(radar, args);
        linkServiceAction.Run();
    }
}