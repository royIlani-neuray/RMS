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

namespace WebService.Actions;

public abstract class EntityAction<Entity> : IAction where Entity : IEntity 
{
    protected readonly string entityId;
    private EntityContext<Entity> entityContext;

    public EntityAction(EntityContext<Entity> entityContext, string entityId)
    {
        this.entityContext = entityContext;
        this.entityId = entityId;
    }

    protected abstract void RunAction(Entity entity);
    protected abstract void RunPostActionTask(Entity entity);   // for sending events etc.

    public void Run()
    {
        var entity = entityContext.GetEntity(entityId);
        entity.EntityLock.EnterUpgradeableReadLock();

        if (!entityContext.IsEntityExist(entityId))
        {
            entity.EntityLock.ExitUpgradeableReadLock();
            throw new NotFoundException($"Cannot find {entity.EntityType} entity with id '{entityId}' in context. action failed.");
        }

        try
        {
            entity.EntityLock.EnterWriteLock();
            RunAction(entity);
        }
        finally
        {
            if (entityContext.IsEntityExist(entityId))
                entityContext.UpdateEntity(entity);
                
            entity.EntityLock.ExitWriteLock();
            entity.EntityLock.ExitUpgradeableReadLock();

            RunPostActionTask(entity);
        }
    }
}