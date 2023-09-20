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

public class SmartFanModel : IModelImplementation
{
    private Model model;

    private const int POINTS_PER_FRAME = 32;
    private const int VALUES_PER_POINT = 5; // x, y, z, velocity, intensity
    private int framesPerWindow;

    public SmartFanModel(Model model)
    {
        this.model = model;
        framesPerWindow = int.Parse(this.model.Settings["FRAMES_PER_WINDOW"]);
    }

    public object Predict(string request)
    {
        SmartFanGestureRequest? smartFanGestureRequest = JsonSerializer.Deserialize<SmartFanGestureRequest>(request);

        if (smartFanGestureRequest == null)
        {
            throw new BadRequestException("Invalid SmartFanGesture request provided");
        }

        return PredictSmartFanGesture(smartFanGestureRequest);
    }

    private object PredictSmartFanGesture(SmartFanGestureRequest request)
    {
        if (request.Frames.Count != framesPerWindow)
            throw new BadRequestException($"Invalid SmartFanGesture request provided. request should contain {framesPerWindow} frames.");

        // 32 Frames X 32 points X 5 values 
        Tensor<float> inputTensor = new DenseTensor<float>(new[] {framesPerWindow, POINTS_PER_FRAME, VALUES_PER_POINT});
        
        //System.Console.WriteLine($"Input tensor len: {inputTensor.Length}");

        for (int frameIndex=0; frameIndex<framesPerWindow; frameIndex++)
        {
            var frame = request.Frames[frameIndex];

            if ((frame.xAxis.Count != POINTS_PER_FRAME) || (frame.yAxis.Count != POINTS_PER_FRAME) || (frame.zAxis.Count != POINTS_PER_FRAME) ||
                (frame.Intensity.Count != POINTS_PER_FRAME) || (frame.Velocity.Count != POINTS_PER_FRAME))
            {
                throw new BadRequestException($"Invalid SmartFanGesture request provided. each frame should contain {POINTS_PER_FRAME} points.");
            }

            float[] xAxis = frame.xAxis.ToArray();
            float[] yAxis = frame.yAxis.ToArray();
            float[] zAxis = frame.zAxis.ToArray();
            float[] velocity = frame.Velocity.ToArray();
            float[] intensity = frame.Intensity.ToArray();

            for (int pointIndex=0; pointIndex<POINTS_PER_FRAME; pointIndex++)
            {
                inputTensor.SetValue((frameIndex * POINTS_PER_FRAME * VALUES_PER_POINT) + (pointIndex * VALUES_PER_POINT), xAxis[pointIndex]);
                inputTensor.SetValue((frameIndex * POINTS_PER_FRAME * VALUES_PER_POINT) + (pointIndex * VALUES_PER_POINT) + 1, yAxis[pointIndex]);
                inputTensor.SetValue((frameIndex * POINTS_PER_FRAME * VALUES_PER_POINT) + (pointIndex * VALUES_PER_POINT) + 2, zAxis[pointIndex]);
                inputTensor.SetValue((frameIndex * POINTS_PER_FRAME * VALUES_PER_POINT) + (pointIndex * VALUES_PER_POINT) + 3, velocity[pointIndex]);
                inputTensor.SetValue((frameIndex * POINTS_PER_FRAME * VALUES_PER_POINT) + (pointIndex * VALUES_PER_POINT) + 4, intensity[pointIndex]);
            }
        }
        
        var inputs = new List<NamedOnnxValue> 
        { 
            NamedOnnxValue.CreateFromTensor<float>("frame_data", inputTensor)
        };

        var output = model.Session!.Run(inputs).ToList();
        DenseTensor<float> outTensor = (DenseTensor<float>) output[0].Value;

        var softmax = Softmax(outTensor.ToArray());
        
        float confidence = -1;
        string label = String.Empty;

        for (int lableIndex=0; lableIndex < softmax.Length; lableIndex++)
        {
            Console.WriteLine($"{this.model.Labels[lableIndex]} - {softmax[lableIndex]:0.00}");

            if (confidence < softmax[lableIndex])
            {
                confidence = softmax[lableIndex];
                label = this.model.Labels[lableIndex];
            }
        }

        Console.WriteLine($"Selected label ==> ** {label} **");

        SmartFanGestureResponse response = new SmartFanGestureResponse() 
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