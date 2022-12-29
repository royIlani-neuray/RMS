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

public class PoseEstimationModel : IModelImplementation
{
    private Model model;

    public PoseEstimationModel(Model model)
    {
        this.model = model;
    }

    public object Predict(string request)
    {
        throw new NotImplementedException();
    }
}