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

namespace WebService.Actions.Template;

public class DeleteTemplateAction : RadarTemplateAction 
{
    public DeleteTemplateAction(string templateId) : base(templateId) {}

    protected override void RunTemplateAction(RadarTemplate radarTemplate)
    {
        System.Console.WriteLine($"Deleting radar template - {templateId}");
        TemplateContext.Instance.DeleteTemplate(radarTemplate);
    }
}