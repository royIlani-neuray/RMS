/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Context;
using WebService.Entites;

namespace WebService.Actions;

public abstract class RadarTemplateAction : EntityAction<RadarTemplate>
{
    public RadarTemplateAction(string templateId) : base(TemplateContext.Instance, templateId) {}

    protected abstract void RunTemplateAction(RadarTemplate template);

    protected override void RunAction(RadarTemplate template)
    {
        RunTemplateAction(template);
    }

    protected override void RunPostActionTask(RadarTemplate template)
    {
    }
    
}