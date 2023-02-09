/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using InferenceService.Entities;

namespace InferenceService.Models;

public class ModelFactory {

    public static IModelImplementation GetModelImplementation(Model model)
    {
        switch (model.ModelType)
        {
            case Model.ModelTypes.GateId: return new GateIdModel(model);
            case Model.ModelTypes.PoseEstimation: return new PoseEstimationModel(model);
            case Model.ModelTypes.HumanDetection: return new HumanDetectionModel(model);

            default:
                throw new Exception("missing implementation for the given model type!");
        }
    }
}