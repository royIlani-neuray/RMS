/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Microsoft.AspNetCore.Mvc;
using InferenceService.Context;
using InferenceService.Entities;
using InferenceService.Actions.Models;

namespace InferenceService.Controllers;

[ApiController]
[Route("api/models")]
public class ModelController : ControllerBase
{
    private readonly ILogger<ModelController> _logger;

    public ModelController(ILogger<ModelController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public List<Model.ModelBrief> GetModels()
    {
        return ModelsContext.Instance.GetModelsBrief();
    }

    [HttpGet("{modelName}")]
    public Model GetModel(string modelName)
    {      
        if (!ModelsContext.Instance.IsModelExist(modelName))
            throw new NotFoundException("There is no model with the given name");

        return ModelsContext.Instance.GetModel(modelName);
    }

    [HttpPost("{modelName}/predict")]
    public object RunPrediction(string modelName, [FromBody] PredictActionArgs args)
    {      
        if (!ModelsContext.Instance.IsModelExist(modelName))
            throw new NotFoundException("There is no model with the given name");

        var predictAction = new PredictAction(modelName, args);
        predictAction.Run();

        return predictAction.PredictResult!;
    }

}
