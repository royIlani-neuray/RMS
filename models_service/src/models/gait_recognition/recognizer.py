import torch
import importlib
import numpy as np
from .mm_gait_net_4d import mmGaitNet4D

from .constants import STRATEGIES


class Recognizer:
    def __init__(self, config, pretrained_model_path):
        self.config = config
        self.pretrained_model_path = pretrained_model_path
        self.device = "cuda" if torch.cuda.is_available() else "cpu"
        self.strategy = self.set_strategy()(config=self.config,
                                            device=self.device,
                                            pretrained_model_path=self.pretrained_model_path,
                                            mode='test')
        self.subjects = self.config["base_labels"]

    def recognize(self, window):
        self.strategy.model.eval()

        with torch.no_grad():
            window = torch.tensor(window)
            data = window.to(self.device)

            logits, embeddings = self.strategy(data)

            _, predicted = torch.max(logits.data, 1)

            classes_probabilities = torch.round(torch.softmax(logits[0], 0), decimals=3).cpu().numpy()
            predicted_class_idx = np.argmax(classes_probabilities)

            return self.subjects[predicted_class_idx], classes_probabilities[predicted_class_idx]

    def set_strategy(self):
        class_name = STRATEGIES[self.config['model_strategy']]
        #module = importlib.import_module('.' + self.config['model_strategy'].lower()        )

        #return getattr(module, class_name)
        return mmGaitNet4D


if __name__ == '__main__':
    '''Unit-test'''
    import json

    CONFIG_PATH = r"./unit_test_data/gait_recognition/model_meta_data.json"
    MODEL_PATH = r"./unit_test_data/gait_recognition/model_weights.pth"
    WINDOW_PATH = r"./unit_test_data/gait_recognition/window.npy"

    with open(CONFIG_PATH, 'r') as f:
        config_ = json.load(f)

    recognizer = Recognizer(config_, MODEL_PATH)

    window_ = np.load(WINDOW_PATH, allow_pickle=True).item()["x"]
    print(recognizer.recognize(window_))
