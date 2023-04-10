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
    private int framesPerWindow;

    public GateIdModel(Model model)
    {
        this.model = model;
        framesPerWindow = int.Parse(this.model.Settings["FRAMES_PER_WINDOW"]);
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
        if (request.Frames.Count != framesPerWindow)
            throw new BadRequestException($"Invalid GateId request provided. request should contain {framesPerWindow} frames.");

        // 1 Batch X 1 sample X 128 points X 30 Frames
        Tensor<float> xTensor = new DenseTensor<float>(new[] {1, 1, framesPerWindow, POINTS_PER_FRAME});
        Tensor<float> yTensor = new DenseTensor<float>(new[] {1, 1, framesPerWindow, POINTS_PER_FRAME});
        Tensor<float> zTensor = new DenseTensor<float>(new[] {1, 1, framesPerWindow, POINTS_PER_FRAME});
        Tensor<float> vTensor = new DenseTensor<float>(new[] {1, 1, framesPerWindow, POINTS_PER_FRAME});
        Tensor<float> iTensor = new DenseTensor<float>(new[] {1, 1, framesPerWindow, POINTS_PER_FRAME});

        //System.Console.WriteLine($"Tensor len: {xTensor.Length}");

        for (int frameIndex=0; frameIndex<framesPerWindow; frameIndex++)
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
        
        // need to align the xTensor and zTensors according to the mean.
        
        var averageX = xTensor.Average();
        for (int index=0; index < xTensor.Length; index++)
        {
            xTensor.SetValue(index, xTensor.GetValue(index) - averageX);
        }

        /*
        var averageZ = zTensor.Average();
        for (int index=0; index < zTensor.Length; index++)
        {
            zTensor.SetValue(index, zTensor.GetValue(index) - averageZ);
        }
        */


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
        
        float confidence = -1;
        string label = String.Empty;

        for (int lableIndex=0; lableIndex < softmax.Length; lableIndex++)
        {
            System.Console.WriteLine($"{this.model.Labels[lableIndex]} - {softmax[lableIndex]}");

            if (confidence < softmax[lableIndex])
            {
                confidence = softmax[lableIndex];
                label = this.model.Labels[lableIndex];
            }
        }

        System.Console.WriteLine($"Selected label: {label}");

        GateIdResponse response = new GateIdResponse() 
        {
            Label = label,
            Confidence = confidence
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
    
}