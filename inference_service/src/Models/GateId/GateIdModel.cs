/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using InferenceService.Entities;

namespace InferenceService.Models;

public class GateIdModel : IModelImplementation
{
    private Model model;

    private const int POINTS_PER_FRAME = 128;
    private const int FRAMES_PER_WINDOW = 30;

    public GateIdModel(Model model)
    {
        this.model = model;
    }

    public object Predict(string request)
    {
        GateIdRequest? gateIdRequest = JsonSerializer.Deserialize<GateIdRequest>(request);

        if (gateIdRequest == null)
        {
            throw new BadRequestException("Invalid GateId request provided");
        }

        return PredictGateId(gateIdRequest);
    }

    private object PredictGateId(GateIdRequest request)
    {
        if (request.Frames.Count != FRAMES_PER_WINDOW)
            throw new BadRequestException($"Invalid GateId request provided. request should contain {FRAMES_PER_WINDOW} frames.");

        // 1 Batch X 1 sample X 128 points X 30 Frames
        Tensor<float> xTensor = new DenseTensor<float>(new[] {1, 1, POINTS_PER_FRAME, FRAMES_PER_WINDOW});
        Tensor<float> yTensor = new DenseTensor<float>(new[] {1, 1, POINTS_PER_FRAME, FRAMES_PER_WINDOW});
        Tensor<float> zTensor = new DenseTensor<float>(new[] {1, 1, POINTS_PER_FRAME, FRAMES_PER_WINDOW});
        Tensor<float> vTensor = new DenseTensor<float>(new[] {1, 1, POINTS_PER_FRAME, FRAMES_PER_WINDOW});
        Tensor<float> iTensor = new DenseTensor<float>(new[] {1, 1, POINTS_PER_FRAME, FRAMES_PER_WINDOW});

        //System.Console.WriteLine($"Tensor len: {xTensor.Length}");

        for (int frameIndex=0; frameIndex<FRAMES_PER_WINDOW; frameIndex++)
        {
            var frame = request.Frames[frameIndex];

            if ((frame.xAxis.Count != POINTS_PER_FRAME) || (frame.yAxis.Count != POINTS_PER_FRAME) || (frame.zAxis.Count != POINTS_PER_FRAME) ||
                (frame.Intensity.Count != POINTS_PER_FRAME) || (frame.Velocity.Count != POINTS_PER_FRAME))
            {
                throw new BadRequestException($"Invalid GateId request provided. each frame should contain {POINTS_PER_FRAME} points.");
            }

            float[] xAxis = frame.xAxis.ToArray();
            float[] yAxis = frame.yAxis.ToArray();
            float[] zAxis = frame.zAxis.ToArray();
            float[] velocity = frame.Velocity.ToArray();
            float[] intensity = frame.Intensity.ToArray();

            for (int pointIndex=0; pointIndex<POINTS_PER_FRAME; pointIndex++)
            {
                xTensor.SetValue((frameIndex * POINTS_PER_FRAME) + pointIndex, xAxis[pointIndex]);
                yTensor.SetValue((frameIndex * POINTS_PER_FRAME) + pointIndex, yAxis[pointIndex]);
                zTensor.SetValue((frameIndex * POINTS_PER_FRAME) + pointIndex, zAxis[pointIndex]);
                vTensor.SetValue((frameIndex * POINTS_PER_FRAME) + pointIndex, velocity[pointIndex]);
                iTensor.SetValue((frameIndex * POINTS_PER_FRAME) + pointIndex, intensity[pointIndex]);
            }
        }
        
        var inputs = new List<NamedOnnxValue> 
        { 
            NamedOnnxValue.CreateFromTensor<float>("x_axis", xTensor),
            NamedOnnxValue.CreateFromTensor<float>("y_axis", yTensor),
            NamedOnnxValue.CreateFromTensor<float>("z_axis", zTensor), 
            NamedOnnxValue.CreateFromTensor<float>("velocity", vTensor), 
            NamedOnnxValue.CreateFromTensor<float>("intensity", iTensor) 
        };

        var output = model.Session!.Run(inputs).ToList();
        DenseTensor<float> outTensor = (DenseTensor<float>) output[0].Value;

        var softmax = Softmax(outTensor.ToArray());
        
        float predictionPrecent = -1;
        string label = String.Empty;

        for (int lableIndex=0; lableIndex < softmax.Length; lableIndex++)
        {
            System.Console.WriteLine($"{this.model.Labels[lableIndex]} - {softmax[lableIndex]}");

            if (predictionPrecent < softmax[lableIndex])
            {
                predictionPrecent = softmax[lableIndex];
                label = this.model.Labels[lableIndex];
            }
        }

        System.Console.WriteLine($"Selected label: {label}");

        GateIdResponse response = new GateIdResponse() 
        {
            Label = label,
            Prediction = predictionPrecent
        };

        return response;
    }

    
    private float[] Softmax(float[] values)
    {
        var maxVal = values.Max();
        var exp = values.Select(v => Math.Exp(v - maxVal));
        var sumExp = exp.Sum();

        return exp.Select(v => (float)(v / sumExp)).ToArray();
    }
    
    /*
    void PrdeictTest()
    {

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
    */

}