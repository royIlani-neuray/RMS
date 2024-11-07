import torch
import importlib
import numpy as np
from torch_geometric.data import Data

from constants import STRATEGIES, TASKS, REPRESENTATIONS


class Recognizer:
    def __init__(self, config, pretrained_model_path):
        self.config = config
        self.pretrained_model_path = pretrained_model_path
        self.device = "cuda" if torch.cuda.is_available() else "cpu"
        self.task = self.set_task(self.config["task"])
        self.representation = self.set_representation(self.config["representation_type"])
        self.strategy = self.set_strategy()(config=self.config,
                                            device=self.device,
                                            pretrained_model_path=self.pretrained_model_path,
                                            mode='test')

        self.subjects = self.config["base_labels"]

    def recognize(self, window):
        if not self.is_valid_window(window):
            return None, None

        self.strategy.model.eval()

        with torch.no_grad():
            data = self.representation.create_sample_representation(window)
            data = Data(**data)
            data = data.to(self.device)

            logits, embeddings = self.strategy(data)

            _, predicted = torch.max(logits.data, 1)

            classes_probabilities = torch.round(torch.softmax(logits[0], 0), decimals=3).cpu().numpy()
            predicted_class_idx = np.argmax(classes_probabilities)

            return self.subjects[predicted_class_idx], classes_probabilities[predicted_class_idx]

    def is_valid_window(self, window):
        return self.task.validate_for_task(window)[0]

    def set_task(self, task):
        class_name = TASKS[task]
        module = importlib.import_module("tasks." + class_name.lower())
        task_class = getattr(module, class_name)
        return task_class()

    def set_representation(self, representation_type):
        class_name = REPRESENTATIONS[representation_type]
        module = importlib.import_module("representations." + class_name.lower())
        representation_class = getattr(module, class_name)
        return representation_class(self.config)

    def set_strategy(self):
        class_name = STRATEGIES[self.config['model_strategy']]
        module = importlib.import_module("strategies." + self.config['model_strategy'].lower(), 'models.recognition')

        return getattr(module, class_name)


