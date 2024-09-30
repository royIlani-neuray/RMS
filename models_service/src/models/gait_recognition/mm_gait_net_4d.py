import torch
import torch.nn as nn
import torch_geometric

from .base import Base
from ..common.constants import X_IDX, Y_IDX, Z_IDX, V_IDX


class ResidualBlock(nn.Module):
    def __init__(self, inplanes, outplanes, kernel=3, stride=1):
        super().__init__()

        self.conv1 = nn.Conv2d(inplanes, outplanes, kernel, stride, padding=1)
        self.bn1 = nn.BatchNorm2d(outplanes)
        self.relu = nn.ReLU(inplace=True)
        self.conv2 = nn.Conv2d(outplanes, outplanes, kernel, stride, padding=1)
        self.bn2 = nn.BatchNorm2d(outplanes)
        self.downsample = nn.Sequential(
            nn.Conv2d(inplanes, outplanes, 1, 1),
            nn.BatchNorm2d(outplanes),
        )

    def forward(self, x):
        identity = x
        out = self.conv1(x)
        out = self.bn1(out)
        out = self.relu(out)
        out = self.conv2(out)
        out = self.bn2(out)
        out += self.downsample(identity)
        out = self.relu(out)

        return out


class AttributeNetwork(nn.Module):
    def __init__(self):
        super().__init__()
        self.attribute = nn.Sequential(
            nn.Conv2d(1, 4, 7, stride=2),
            nn.BatchNorm2d(4),
            nn.ReLU(),
            nn.MaxPool2d(3, 2),
            ResidualBlock(4, 8)
        )

    def forward(self, x):
        return self.attribute(x)


class FeatureFusion(nn.Module):
    def __init__(self):
        super().__init__()

        self.feature_fusion = nn.Sequential(
            nn.Conv2d(32, 16, 3, stride=1, padding=1),
            nn.BatchNorm2d(16),
            nn.ReLU(),
            nn.AvgPool2d(2, 2)
        )

    def forward(self, x):
        return self.feature_fusion(x)


class FeatureExtractor(nn.Module):
    def __init__(self):
        super().__init__()

        self.dropout = nn.Dropout(0.00)

        self.x_attribute_network = AttributeNetwork()
        self.y_attribute_network = AttributeNetwork()
        self.z_attribute_network = AttributeNetwork()
        self.v_attribute_network = AttributeNetwork()
        self.attribute_networks = [self.x_attribute_network,
                                   self.y_attribute_network,
                                   self.z_attribute_network,
                                   self.v_attribute_network]
        self.flatten = nn.Flatten()
        self.feature_fusion = FeatureFusion()

    def forward(self, points):
        x_axis, y_axis, z_axis, velocity = (points[..., X_IDX],
                                            points[..., Y_IDX],
                                            points[..., Z_IDX],
                                            points[..., V_IDX])
        to_fuse = [self.attribute_networks[0](torch.unsqueeze(self.dropout(x_axis), axis=1)),
                   self.attribute_networks[1](torch.unsqueeze(self.dropout(y_axis), axis=1)),
                   self.attribute_networks[2](torch.unsqueeze(self.dropout(z_axis), axis=1)),
                   self.attribute_networks[3](torch.unsqueeze(self.dropout(velocity), axis=1))]
        fused = torch.cat(to_fuse, dim=1)

        out = self.feature_fusion(self.dropout(fused))

        return self.flatten(out)


class Model(nn.Module):
    """
        Based on paper: "Gait Recognition for Co-Existing Multiple People Using MillimeterWave Sensing, 2020"
        https://ojs.aaai.org/index.php/AAAI/article/view/5430/5286

        Classification pipeline:
            FeatureExtractor -> FC layer
    """
    def __init__(self, num_classes, config, pre_trained_dim=None):
        super().__init__()

        self.num_classes = pre_trained_dim if pre_trained_dim else num_classes
        self.config = config
        self.fc = None

        self.feature_extractor = FeatureExtractor()
        self.create_top_head_classifier(self.num_classes)

    def forward(self, points):
        features = self.feature_extractor(points)

        return self.fc(features), features

    def get_features_size(self):
        mock_data = torch.zeros(
            (1,
             self.config["num_frames_in_window"],
             self.config["num_points_in_frame"],
             len(self.feature_extractor.attribute_networks) + 1),
            device=self.device,
            dtype=torch.float32)

        return self.feature_extractor(mock_data).shape[-1]

    def create_top_head_classifier(self, num_classes):
        self.fc = nn.Linear(self.get_features_size(), num_classes, device=self.device)

    @property
    def device(self):
        if not list(self.parameters()):
            # Default device before force assignment (e.g. model.to("cuda"))
            return "cpu"

        else:
            return list(self.parameters())[0].device.type


class mmGaitNet4D(Base):
    def __init__(self, **kwargs):
        self.config = kwargs['config']
        num_classes = len(self.config['base_labels'])

        super().__init__(Model(num_classes, self.config, pre_trained_dim=kwargs.get('pre_trained_dim')),
                         kwargs['device'],
                         kwargs['pretrained_model_path'],
                         kwargs['mode'])

        if self.mode == 'train' and self.is_transfer_learning:
            self.rebuild_for_transfer_learning()

        self.criterion = self.get_loss(self.config.get('loss_type'))

    def rebuild_for_transfer_learning(self):
        num_labels_for_transfer_learning = len(self.config["labels_for_transfer_learning"])

        self.freeze_feature_extractor_weights()
        self.model.create_top_head_classifier(num_labels_for_transfer_learning + 1)

    def get_loss(self, loss_type):
        if loss_type == 'custom':
            return self.custom_loss

        else:
            return nn.CrossEntropyLoss()

    def custom_loss(self, outputs, targets, coefficient_positive=10, coefficient_negative=50):
        cosine_embedding_loss = nn.CosineEmbeddingLoss(margin=0.5, reduction='mean')
        cross_entropy_loss = nn.CrossEntropyLoss()

        predictions, last_hidden_state = outputs
        ce_loss = cross_entropy_loss(predictions, targets)
        embeddings = torch.nn.functional.normalize(last_hidden_state, p=2, dim=1)
        pair1_positive, pair2_positive, pair_labels_positive, pair1_negative, pair2_negative, pair_labels_negative \
            = self.get_cosine_embedding_pairs(embeddings, targets)
        cosine_loss_positive = cosine_embedding_loss(pair1_positive, pair2_positive, pair_labels_positive)
        cosine_loss_negative = cosine_embedding_loss(pair1_negative, pair2_negative, pair_labels_negative)
        total_loss = ce_loss + coefficient_positive * cosine_loss_positive + coefficient_negative * cosine_loss_negative

        return total_loss

    @staticmethod
    def get_cosine_embedding_pairs(embeddings, labels):
        """
        Generates a balanced set of pairs of embeddings within the batch (equal number of positive and negative pairs).
        - pair1: A tensor containing the first embedding in each pair.
        - pair2: A tensor containing the second embedding in each pair.
        - pair_labels: A tensor of 1s and -1s, where 1 indicates that the pair has the same label and -1 indicates that
            they are different.
        """
        indices = torch.arange(len(labels), device=labels.device)
        i, j = torch.meshgrid(indices, indices, indexing='ij')
        i, j = i.flatten(), j.flatten()
        mask = i != j
        i, j = i[mask], j[mask]

        # Get corresponding pairs of embeddings and labels
        pair1, pair2 = embeddings[i], embeddings[j]
        pair_labels = torch.where(labels[i] == labels[j], 1, -1)
        positive_mask = pair_labels == 1
        negative_mask = pair_labels == -1

        return (pair1[positive_mask], pair2[positive_mask], pair_labels[positive_mask],
                pair1[negative_mask], pair2[negative_mask], pair_labels[negative_mask])

    def freeze_feature_extractor_weights(self):
        for param in self.model.feature_extractor.parameters():
            param.requires_grad = False

    @staticmethod
    def preprocess_data(data):
        if isinstance(data, torch_geometric.data.batch.Batch):
            points = data.x
            batch = data.batch
            batch_size = int(batch[-1]) + 1

        else:
            points = data
            batch_size = 1

        data_shape = points.shape
        processed_data = torch.reshape(
            points, (batch_size, len(points) // batch_size, data_shape[1], data_shape[2])
        )

        return processed_data

    def evaluate(self, args, kwargs):
        data = args[0]
        data = self.preprocess_data(data)

        return self.model(data)
