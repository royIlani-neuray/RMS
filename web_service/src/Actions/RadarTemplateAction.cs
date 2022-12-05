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

public abstract class RadarTemplateAction : IAction
{
    protected readonly string templateId;
    public RadarTemplateAction(string templateId)
    {
        this.templateId = templateId;
    }

    protected abstract void RunTemplateAction(RadarTemplate radarTemplate);

    public void Run()
    {
        var radarTemplate = TemplateContext.Instance.GetTemplate(templateId);
        radarTemplate.templateLock.EnterUpgradeableReadLock();

        if (!TemplateContext.Instance.IsRadarTemplateExist(templateId))
        {
            radarTemplate.templateLock.ExitUpgradeableReadLock();
            throw new NotFoundException($"Cannot find template with id '{templateId}' in context. action failed.");
        }

        try
        {
            radarTemplate.templateLock.EnterWriteLock();
            RunTemplateAction(radarTemplate);
        }
        finally
        {
            if (TemplateContext.Instance.IsRadarTemplateExist(templateId))
                TemplateContext.Instance.UpdateTemplate(radarTemplate);
                
            radarTemplate.templateLock.ExitWriteLock();
            radarTemplate.templateLock.ExitUpgradeableReadLock();
        }
    }
}