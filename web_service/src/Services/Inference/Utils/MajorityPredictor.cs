/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/

using Serilog;

namespace WebService.Services.Inference.Utils;

public class MajorityPerdictor
{
    private string invalidPredictionValue;      // the default value to return if no prediction exist.
    private int minRequiredHitCount;            // minimal number of times we must get a value before it can be considered as prediction.
    private int inferenceWindowSize;            // the size of the inference window wich updates over time. (helps to solve track switch issues)
    private Queue<string> inferenceWindow;      // holds the last values provided, up to 'inferenceWindowSize'

    private string currentPrediction;

    public MajorityPerdictor(int minRequiredHitCount, int inferenceWindowSize, string invalidPredictionValue)
    {
        this.minRequiredHitCount = minRequiredHitCount;
        this.inferenceWindowSize = inferenceWindowSize;

        this.invalidPredictionValue = invalidPredictionValue;
        this.currentPrediction = invalidPredictionValue;
        inferenceWindow = new Queue<string>();
    }

    public string GetPrediction()
    {
        return currentPrediction;
    }

    public void UpdatePrediction(string newPrediction)
    {
        inferenceWindow.Enqueue(newPrediction);

        if (inferenceWindow.Count > inferenceWindowSize)
            inferenceWindow.Dequeue();
        
        currentPrediction = invalidPredictionValue;
        int currentValueCount = minRequiredHitCount - 1;

        var distinctValues = inferenceWindow.Distinct();

        foreach (string value in distinctValues) 
        {
            int valueCount = inferenceWindow.Where(entry => entry == value).Count();

            if (valueCount > currentValueCount)
            {
                currentPrediction = value;
                currentValueCount = valueCount;
            }
        }

        //Log.Debug($"Prediction: '{currentPrediction}' [{currentValueCount}]");

    }
}