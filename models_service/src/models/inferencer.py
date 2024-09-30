import json
import glob

import numpy as np

from .gait_recognition.recognizer import Recognizer


class Inferencer:
    def __init__(self, model_folder_path):
        self.model_info = self.get_model_info(model_folder_path)
        self.inference_method = self.get_model_inference_method()

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


if __name__ == '__main__':
    '''Unit-test'''

    WINDOW_PATH = r"./unit_test_data/gait_recognition/window.npy"
    window_ = np.load(WINDOW_PATH, allow_pickle=True).item()["x"]

    inferencer = Inferencer(r"C:\Users\GuyAdar\Code\ai_research\services\unit_test_data\gait_recognition")
    print(inferencer.run(window_))
