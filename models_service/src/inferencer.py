import json
import glob

from recognizer import Recognizer


class Inferencer:
    def __init__(self, model_folder_path):
        self.model_info = self.get_model_info(model_folder_path)
        self.inference_method = self.get_model_inference_method()

    def translate(self, prediction_input):
        window = []
        for frame in prediction_input["frames"]:
            window.append()
        return window
    
    def run(self, window):
        return self.inference_method(window)

    def get_model_inference_method(self):
        model_type = self.model_info['meta_data']['type']

        if model_type == 'gait_recognition':
            return Recognizer(self.model_info['meta_data'],
                              self.model_info['pretrained_model_path']).recognize

    @staticmethod
    def get_model_info(model_folder_path):

        with open(model_folder_path + r"/model_meta_data.json", "r") as f:
            model_info = {
                "meta_data": json.load(f),
                "pretrained_model_path": glob.glob(model_folder_path + r"/*.pth")[0],
            }

        return model_info
