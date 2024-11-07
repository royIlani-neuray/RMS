import numpy as np

NORMALIZE = "normalize"
USE_AUGMENTATIONS = "use_augmentations"
EARLY_STOPPING_PATIENCE = "early_stopping_patience"
DATASET_FILTERS = "dataset_filters"
SUBJECT_FILTERS = "subject_filters"
ENVIRONMENT_FILTERS = "environment_filters"
BASE_LABELS = "base_labels"
LEARNING_RATE = "learning_rate"
BATCH_SIZE = "batch_size"
TRAIN_PATH = "train_data_map_path"
TEST_PATH = "test_data_map_path"
SEED = "seed"
EPOCHS = "epochs"
ROTATION_ANGLE_DEGREE = "rotation_angle_deg"
SCHEDULER_STEP_INTERVAL = "scheduler_step_interval"
SCHEDULER_GAMMA = "scheduler_gamma"
TASK_NAME = "task_name"
PRE_TRAINED_PATH = "pretrained_model_path"
FILTERS = "filters"
AUGMENTATION_PARAM = "augmentation_params"

STRATEGIES = {'MM_GAIT_NET_4D': 'mmGaitNet4D',
              'CONV_GAIT_MODEL': 'ConvGaitModel',
              'GAIT_GEST': 'GaitGest',
              'GAIT_GNN': 'GaitGNN',
              'POSE_ESTIMATION': 'PoseEstimation'}

TASKS = {"GAIT": "Gait"}

REPRESENTATIONS = {'GRAPH': 'Graph',
                   'POINT_CLOUD': 'PointCloud',
                    'RANGE_DOPPLER': 'RangeDoppler',
                   'SPECTOGRAMA': 'Spectograma'}

X = 'x'
Y = 'y'
Z = 'z'
V = 'v'
S = 's'
FRAME_ID = 'frame_id'

PADDING_SIZE = 128
FEATURES_NUM = 6
FRAME_IDX = 0
X_IDX = 1
Y_IDX = 2
Z_IDX = 3
V_IDX = 4
S_IDX = 5
AZIMUTH_IDX = 6
ELEVATION_IDX = 7
RANGE_IDX = 8

GAIT_X = 'X'
GAIT_Y = 'Y'
GAIT_Z = 'Z'
DOPPLER = 'Doppler'
INTENSITY = 'Intensity'
GAIT_FRAME_ID = 'Frame #'
RADAR = 'radar'

YEAR = 'y'
MONTH = 'month'
DAY = 'd'
HOUR = 'h'
MINUTE = 'm'
SECOND = 's'

POINT_SIZE = 10 # including frame idx as first index

## TX Output Power Across Azimuth table ##
gain_data_azimuth = np.array([[-80,0], [-70,5], [-60,7], [-50, 9.5], [-40, 11.5], [-30, 13.5],
                    [-20, 15], [-10, 16], [0, 16],[10,15.5], [20, 15.4], [30,15], [40,13],[50,11], [60,8],[70,7], [80,0]])

## TX Output Power Across Elevation table ##
gain_data_elevation = np.array([[-80,0], [-70,7], [-60,9], [-50, 11], [-40, 12], [-30, 14],
                    [-20, 15], [-10, 16], [0, 16],[10,15.5], [20, 14], [30,15], [40,11],[50,11], [60,11],[70,9],[75, 4] ,[80,2]])

X_headers = ["sample_ID", "time_stamp", "origin", "subject", "environment", "video", "radar_ID"]