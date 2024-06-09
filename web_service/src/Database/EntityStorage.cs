/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using System.Text.Json;

namespace WebService.Database;

public class EntityStorage<Entity> where Entity : IEntity {

    public static readonly string StorageFileExtention = ".json";

    public static void SaveEntity(Entity entity)
    {
        string jsonString = JsonSerializer.Serialize(entity);
        File.WriteAllText(System.IO.Path.Combine(entity.StoragePath, entity.Id + StorageFileExtention), jsonString);
    } 

    public static void DeleteEntity(Entity entity)
    {
        string filePath = System.IO.Path.Combine(entity.StoragePath, entity.Id + StorageFileExtention);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else
        {
            Console.WriteLine($"Warning: could not delete entity file for {entity.EntityType}: {entity.Id}");
        }
    }

    public static Dictionary<string, Entity> LoadAllEntitys(string storagePath)
    {
        Dictionary<string, Entity> entities = new Dictionary<string, Entity>();

        var files = System.IO.Directory.GetFiles(storagePath, "*" + StorageFileExtention);
        foreach (string filePath in files)
        {
            string jsonString = File.ReadAllText(filePath);
            
            Entity? entity = JsonSerializer.Deserialize<Entity>(jsonString);

            if (entity == null)
                throw new Exception("deserialze entity failed!");

            entities.Add(entity.Id, entity);
        }

        Console.WriteLine($"Loaded {entities.Keys.Count} entities from storage.");
        return entities;
    }

}