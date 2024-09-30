"""
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
"""
import os
import json
import logging
import shutil
from exceptions import *
from dataclasses import dataclass
from models.inferencer import Inferencer  

DEFAULT_MODELS_PATH = "./default_models"
MODELS_PATH = "./data/models"

class ModelsManager(object):
    __instance = None

    @dataclass
    class ModelData:
        metadata: dict
        inferencer: Inferencer 

    def __new__(cls):
        if cls.__instance is None:
            cls.__instance = super(ModelsManager,cls).__new__(cls)
            cls.__instance.__initialized = False
        return cls.__instance

    def __init__(self):
        if self.__initialized: 
            return
        
        self.__initialized = True
        self.models = {}

    def _init_models_directory(self):
        if not os.path.exists(MODELS_PATH):
            logging.info("creating models directory...")
            shutil.copytree(DEFAULT_MODELS_PATH, MODELS_PATH, dirs_exist_ok=True)
    
    def _get_model_data(self, model_name : str) -> ModelData:
        model = self.models.get(model_name)
        if model is None:
            logging.error(f"Model named '{model_name}' not found.")
            raise NotFoundException(f"Model named '{model_name}' not found.")
        
        return model

    def _load_model(self, model_name : str, model_path : str, meta_file_path :str):
        logging.info(f"loading model: {model_name}")

        try:
            with open(meta_file_path) as metaFile:
                meta_data = json.load(metaFile)
        except:
            logging.error("Failed to load model metadata from - {meta_file_path}")
            return

        inferencer = Inferencer(model_path)
        model_data = ModelsManager.ModelData(meta_data, inferencer)
        self.models[model_name] = model_data

    def load_models(self):
        self._init_models_directory()

        for filename in os.listdir(MODELS_PATH):
            full_path = os.path.join(MODELS_PATH, filename)
            if os.path.isdir(full_path):    # iterate over model subfolders
                meta_file_path = os.path.join(full_path, "model_meta_data.json")
                if os.path.exists(meta_file_path):
                    model_name = filename
                    model_path = full_path
                    self._load_model(model_name, model_path, meta_file_path)
                else:
                    logging.warning(f"cannot find metadata file on model subfolder: {full_path}")

                    

    def get_model_info(self, model_name : str) -> dict:
        model_data = self._get_model_data(model_name)
        return model_data.metadata
    
    def run_prediction(self, model_name : str, prediction_input : dict) -> dict:
        
        #print(len(prediction_input['frames'][0]["range"]))

        model_data = self._get_model_data(model_name)

        # translation...
        #window = translate(prediction_input)
        
        #output = model_data.inferencer.run(window)

        output = "elad" , 100
        logging.info(f"Prediction result - Model: {model_name}, Label: {output[0]}, Confidence: {output[1]}")
        return { "label" : output[0], "confidence" : output[1] } 
