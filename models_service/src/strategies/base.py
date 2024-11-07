import torch
import os
from abc import ABC, abstractmethod
from torch import nn


class Base(ABC):
    def __init__(self, model, device, pretrained_model_path, mode):
        self.model = model
        self.device = device
        self.pretrained_model_path = pretrained_model_path
        self.mode = mode
        self.is_transfer_learning = self.config.get('is_transfer_learning')
        self.is_fine_tuning = self.config.get('is_fine_tuning')
        self.criterion = nn.CrossEntropyLoss()

        self.init_model()

    def __call__(self, *args, **kwargs):
        outputs = self.evaluate(args, kwargs)

        if len(outputs) == 1:
            return [outputs, None]
        else:
            return outputs

    @abstractmethod
    def evaluate(self, args, kwargs):
        pass

    def init_model(self):
        if self.mode == "train":
            if self.is_transfer_learning or self.is_fine_tuning:
                if os.path.exists(self.pretrained_model_path):
                    model = torch.load(self.pretrained_model_path, map_location=torch.device(self.device))
                    self.model.load_state_dict(model)
                else:
                    raise Exception(
                        "Couldn't find pretrained model file. Cannot perform transfer learning / fine-tuning."
                    )

        elif self.mode == "test":
            if os.path.exists(self.pretrained_model_path):
                model = torch.load(self.pretrained_model_path, map_location=torch.device(self.device))
                self.model.load_state_dict(model)
            else:
                raise Exception("Couldn't find pretrained model file. Cannot perform test.")

        self.model.to(self.device)
