import torch
import importlib
import numpy as np

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
        module = importlib.import_module("." + self.config['model_strategy'].lower(), 'models.gait_recognition')

        return getattr(module, class_name)
