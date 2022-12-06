using System.IO;
using InferenceService.Entities;
using System.Text.Json;

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

    public static void SaveModelMetadata(Model model)
    {
        string jsonString = JsonSerializer.Serialize(model);
        File.WriteAllText(Path.Combine(StoragePath, model.Name + ModelMetaFileExtention), jsonString);
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