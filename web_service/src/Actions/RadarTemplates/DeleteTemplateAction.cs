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
using WebService.Events;
using Serilog;

namespace WebService.Actions.RadarTemplates;

public class DeleteTemplateAction : RadarTemplateAction 
{
    public DeleteTemplateAction(string templateId) : base(templateId) {}

    protected override void RunTemplateAction(RadarTemplate template)
    {
        Log.Information($"Deleting radar template - {template.Id}");
        TemplateContext.Instance.DeleteTemplate(template);

        RMSEvents.Instance.TemplateDeletedEvent(template.Id);
    }
}