"""
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
"""
from fastapi import Request
from fastapi_utils.cbv import cbv
from fastapi_utils.inferring_router import InferringRouter
from models_manager import ModelsManager

models_router = InferringRouter()

@cbv(models_router)
class ModelsController(object):

    @models_router.get("/api/models/{model_name}")
    def get_model(self, model_name: str):
        return ModelsManager().get_model_info(model_name)

    @models_router.post("/api/models/{model_name}/predict")
    async def predict(self, model_name: str, request: Request):
        input_data = await request.json()
        return ModelsManager().run_prediction(model_name, input_data)
