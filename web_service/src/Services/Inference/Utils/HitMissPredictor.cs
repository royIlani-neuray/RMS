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

public abstract class HitMissPredictor<T> 
{
    private int requiredHitCount;
    private int requiredMissCount;
    private T invalidPredictionValue;

    private int hits;
    private int misses;
    private T currentPrediction;


    public HitMissPredictor(int requiredHitCount, int requiredMissCount, T invalidPredictionValue)
    {
        this.requiredHitCount = requiredHitCount;
        this.requiredMissCount = requiredMissCount;
        this.invalidPredictionValue = invalidPredictionValue;
        this.currentPrediction = invalidPredictionValue;
        hits = 0;
        misses = 0;
    }

    public T GetPrediction()
    {
        if (hits >= requiredHitCount)
        {
            return currentPrediction;
        }
        else
        {
            return invalidPredictionValue;
        }
    }

    public void UpdatePrediction(T newPrediction)
    {
        if (hits == 0)
        {
            hits = 1;
            currentPrediction = newPrediction;
            Log.Verbose($"FIRST HIT: {newPrediction}\n");
            return;
        }

        if (IsEqual(currentPrediction, newPrediction))
        {
            hits++;
            Log.Verbose($"HITS: {hits}\n");
        }
        else
        {
            if (hits < requiredHitCount)
            {
                Log.Verbose($"HIT RESET\n");
                hits = 1;
                currentPrediction = newPrediction;
                return;
            }

            misses++;
            Log.Verbose($"MISES: {misses}\n");
            if (misses >= requiredMissCount)
            {
                misses = 0;
                hits = 1;
                currentPrediction = newPrediction;
            }
        }
    }

    protected abstract bool IsEqual(T value1, T value2);
}

public class LabelHitMissPredictor : HitMissPredictor<string>
{
    public LabelHitMissPredictor(int requiredHitCount, int requiredMissCount, string invalidPredictionValue) 
            : base(requiredHitCount, requiredMissCount, invalidPredictionValue)
    {
    }

    protected override bool IsEqual(string value1, string value2)
    {
        return value1 == value2;
    }
} 

