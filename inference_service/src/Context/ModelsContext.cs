/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using InferenceService.Storage;
using InferenceService.Entities;
using Microsoft.ML.OnnxRuntime;

namespace InferenceService.Context;

public sealed class ModelsContext {

    private static Dictionary<string, Model> models = new Dictionary<string, Model>();

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile ModelsContext? instance; 

    public static ModelsContext Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new ModelsContext();
                }
            }

            return instance;
        }
    }

    private ModelsContext() {}

    #endregion

    public void LoadModelsFromStorage()
    {
        models = new Dictionary<string, Model>(ModelsStorage.LoadAllModels());

        // create a onnx runtime sessions

        foreach (var model in models.Values)
        {
            string modelFilePath = ModelsStorage.GetModelFilePath(model.Name);

            //System.Console.WriteLine("Loading model: " + modelFilePath);
            model.Session = new InferenceSession(modelFilePath);
        }
    }

    public bool IsModelExist(string modelName)
    {
        if (models.Keys.Contains(modelName))
            return true;
        
        return false;
    }

    public Model GetModel(string modelName)
    {
        if (!IsModelExist(modelName))
            throw new NotFoundException($"Could not find model '{modelName}' in context.");

        return models[modelName];
    }

    public void AddModel(Model model)
    {
        if (IsModelExist(model.Name))
            throw new Exception("Cannot add model. Another model with the same name already exist.");

        ModelsStorage.SaveModelMetadata(model);
        models.Add(model.Name, model);
    }

    public void UpdateModel(Model model)
    {
        if (!IsModelExist(model.Name))
            throw new NotFoundException($"Could not find model '{model.Name}' in context.");

        ModelsStorage.SaveModelMetadata(model);
    }

    public void DeleteModel(Model model)
    {
        GetModel(model.Name); // make sure model enlisted

        ModelsStorage.DeleteModel(model);
        models.Remove(model.Name);
    }

    public List<Model.ModelBrief> GetModelsBrief()
    {
        return models.Values.ToList().ConvertAll<Model.ModelBrief>(model => new Model.ModelBrief(model));
    }
}