/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.IO;
using InferenceService.Entities;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace InferenceService.Storage;

public class ModelsStorage 
{
    public static readonly string StoragePath = "./data/models";
    public static readonly string ModelFileExtention = ".onnx";
    public static readonly string ModelMetaFileExtention = ".json";


    public static void InitStorage()
    {
        if (!Directory.Exists(StoragePath))
        {
            System.Console.WriteLine("Creating models storage folder.");
            Directory.CreateDirectory(StoragePath);
        }
    }

    public static string GetModelMetaFilePath(string modelName)
    {
        return Path.Combine(StoragePath, $"{modelName}{ModelMetaFileExtention}");
    }

    public static string GetModelFilePath(string modelName)
    {
        return Path.Combine(StoragePath, $"{modelName}{ModelFileExtention}");
    }

    private static bool IsValidFileName(string fileName)
    {
        // The regular expression used to check if the file name is valid
        Regex regex = new Regex("^[^/\\\\:*?\"<>|]*$");
        return regex.IsMatch(fileName);
    }

    public static void SaveModelMetadata(Model model)
    {
        if (!IsValidFileName(model.Name))
        {
            throw new BadRequestException("Error: model name is invalid.");
        }

        string jsonString = JsonSerializer.Serialize(model);
        File.WriteAllText(GetModelMetaFilePath(model.Name), jsonString);
    }

    public static void DeleteModel(Model model)
    {
        string metaFilePath = Path.Combine(StoragePath, model.Name + ModelMetaFileExtention);

        if (File.Exists(metaFilePath))
        {
            File.Delete(metaFilePath);
        }
        else
        {
            Console.WriteLine($"Warning: could not delete meta file for model: {model.Name}");
        }

        string modelFilePath = Path.Combine(StoragePath, model.Name + ModelFileExtention);

        if (File.Exists(modelFilePath))
        {
            File.Delete(modelFilePath);
        }
        else
        {
            Console.WriteLine($"Warning: could not delete model file for model: {model.Name}");
        }
    }

    public static Dictionary<string, Model> LoadAllModels()
    {
        Dictionary<string, Model> models = new Dictionary<string, Model>();

        var files = Directory.GetFiles(StoragePath, "*" + ModelMetaFileExtention);
        foreach (string filePath in files)
        {
            string jsonString = File.ReadAllText(filePath);
            
            Model? model = JsonSerializer.Deserialize<Model>(jsonString);

            if (model == null)
                throw new Exception("deserialze failed!");

            if (!File.Exists(GetModelFilePath(model.Name)))
            {
                Console.WriteLine($"Error: missing model file for model: {model.Name}");
                continue;
            }

            models.Add(model.Name, model);
        }

        Console.WriteLine($"Loaded {models.Keys.Count} models from storage.");
        return models;
    }

}