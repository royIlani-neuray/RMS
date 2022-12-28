/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Microsoft.AspNetCore.Mvc;
using InferenceService.Context;
using InferenceService.Entities;

// for testing:
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

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
    public object RunPrediction(string modelName)
    {      
        if (!ModelsContext.Instance.IsModelExist(modelName))
            throw new NotFoundException("There is no model with the given name");

        // TODO:....
        PrdeictTest(modelName);
        return new {};
    }


    private float[] Softmax(float[] values)
    {
        var maxVal = values.Max();
        var exp = values.Select(v => Math.Exp(v - maxVal));
        var sumExp = exp.Sum();

        return exp.Select(v => (float)(v / sumExp)).ToArray();
    }

    void PrdeictTest(string modelName)
    {
        var model = ModelsContext.Instance.GetModel(modelName);

        // 1 Batch X 1 sample X 128 points X 30 Frames
        Tensor<float> xTensor = new DenseTensor<float>(new[] {1, 1, 128, 30});
        Tensor<float> yTensor = new DenseTensor<float>(new[] {1, 1, 128, 30});
        Tensor<float> zTensor = new DenseTensor<float>(new[] {1, 1, 128, 30});
        Tensor<float> vTensor = new DenseTensor<float>(new[] {1, 1, 128, 30});
        Tensor<float> iTensor = new DenseTensor<float>(new[] {1, 1, 128, 30});

        var inputs = new List<NamedOnnxValue> 
        { 
            NamedOnnxValue.CreateFromTensor<float>("x_axis", xTensor),
            NamedOnnxValue.CreateFromTensor<float>("y_axis", yTensor),
            NamedOnnxValue.CreateFromTensor<float>("z_axis", zTensor), 
            NamedOnnxValue.CreateFromTensor<float>("velocity", vTensor), 
            NamedOnnxValue.CreateFromTensor<float>("intensity", iTensor) 
        };

        var output = model.Session!.Run(inputs).ToList();
        //var output = model.Session!.Run(inputs).ToList().AsEnumerable<NamedOnnxValue>();
        //System.Console.WriteLine(output[0].Name);
        //System.Console.WriteLine(output[0].Value);
        DenseTensor<float> outTensor = (DenseTensor<float>) output[0].Value;

        var softmax = Softmax(outTensor.ToArray());
        foreach (var x in softmax)
            System.Console.WriteLine(x);

    }

}
