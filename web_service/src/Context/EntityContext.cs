/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Database;
using WebService.Entites;

namespace WebService.Context;

public class EntityContext<Entity> where Entity : IEntity {

    private IEntity.EntityTypes entityType;

    public EntityContext(IEntity.EntityTypes entityType)
    {
        this.entityType = entityType;
    }

    protected static Dictionary<string, Entity> entities = new Dictionary<string, Entity>();

    protected void LoadEntitiesFromStorage(string StoragePath)
    {
        entities = new Dictionary<string, Entity>(EntityStorage<Entity>.LoadAllEntitys(StoragePath));
    }

    public bool IsEntityExist(string entityId)
    {
        if (entities.Keys.Contains(entityId))
            return true;
        
        return false;
    }

    public Entity GetEntity(string entityId)
    {
        if (!IsEntityExist(entityId))
            throw new NotFoundException($"Could not find {entityType} in context with id - {entityId}");

        return entities[entityId];
    }

    public void AddEntity(Entity entity)
    {
        if (IsEntityExist(entity.Id))
            throw new Exception($"Cannot add {entityType} entity. Another {entityType} with the same ID already exist.");

        EntityStorage<Entity>.SaveEntity(entity);
        entities.Add(entity.Id, entity);
    }

    public void UpdateEntity(Entity entity)
    {
        if (!IsEntityExist(entity.Id))
            throw new NotFoundException($"Could not find {entityType} entity in context with id - {entity.Id}");

        EntityStorage<Entity>.SaveEntity(entity);
    }

    public void DeleteEntity(Entity entity)
    {
        GetEntity(entity.Id); // make sure entity enlisted

        EntityStorage<Entity>.DeleteEntity(entity);
        entities.Remove(entity.Id);
    }

}