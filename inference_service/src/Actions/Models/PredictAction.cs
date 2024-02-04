/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using InferenceService.Context;
using InferenceService.Models;

namespace InferenceService.Actions.Models;

public class PredictActionArgs 
{
    [JsonPropertyName("prediction_input")]
    public string PredictionInput { get; set; } = String.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(PredictionInput))
            throw new HttpRequestException("Missing prediction input.");
    }
}


public class PredictAction : IAction
{
    private string modelName;
    private PredictActionArgs args;

    public object? PredictResult { get; private set; }

    public PredictAction(string modelName, PredictActionArgs args)
    {
        this.modelName = modelName;
        this.args = args;
    }

    public void Run()
    {
        System.Console.WriteLine($"\n***** Prediction : {modelName} [{DateTime.Now}] *****");
        DateTime start = DateTime.Now;
        var modelEntity = ModelsContext.Instance.GetModel(modelName);
        var model = ModelFactory.GetModelImplementation(modelEntity);

        PredictResult = model.Predict(args.PredictionInput);

        var predictTime = (DateTime.Now - start).TotalMilliseconds;
        System.Console.WriteLine($"Prediction Time: {predictTime} ms");
    }
}