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
import uvicorn
import argparse
from fastapi import FastAPI
from models_controller import models_router
from models_manager import ModelsManager


MODELS_SERVICE_DEFAULT_PORT = 16504
MODELS_SERVICE_DEFAULT_HOST = "localhost"

LOGGER_FILES_PATH = "./data/logs"



class ModelsService(object):
    def __init__(self, config):
        self.config = config
        self.service_host = os.getenv("MODELS_SERVICE_HOST", MODELS_SERVICE_DEFAULT_HOST)
        self.service_port = int(os.getenv("MODELS_SERVICE_PORT", MODELS_SERVICE_DEFAULT_PORT))
        self.init_logger(self.config['logging'])
        self.modelsManager = ModelsManager()
        self.modelsManager.load_models()

    def init_logger(self, logSettings):
        if not os.path.exists(LOGGER_FILES_PATH):
            os.makedirs(LOGGER_FILES_PATH)

        logLevel = getattr(logging, logSettings['level'])

        file_handler = logging.handlers.RotatingFileHandler(
            os.path.join(LOGGER_FILES_PATH,logSettings['filename']), 
            maxBytes=logSettings['max_file_size_mb']*1024*1024,
            backupCount=logSettings['max_files']
        )
        
        logging.basicConfig(
            level=logLevel,
            format="%(asctime)s [%(levelname)s] %(message)s",
            handlers=[
                file_handler,
                logging.StreamHandler()
            ]
        ) 

    def start(self):
        self.app = FastAPI()
        self.app.include_router(models_router)
        uvicorn.run(self.app, host=self.service_host, port=self.service_port)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-config", help="set the configuration file", required=True)
    args = parser.parse_args()

    with open(args.config) as configFile:
        config = json.load(configFile)
        
    models_service = ModelsService(config)
    models_service.start()

    
    
