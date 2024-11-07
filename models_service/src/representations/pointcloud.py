import numpy as np
import torch

from representations.point_cloud_utils import cyclic_pad_data
from representations.base import Base
from constants import PADDING_SIZE, S_IDX, Y_IDX, FEATURES_NUM, FRAME_IDX, RANGE_IDX, ELEVATION_IDX, AZIMUTH_IDX, \
    V_IDX

MAX_Y_DISTANCE_IN_PC = 3

class PointCloud(Base):

    def __init__(self, args):
        super().__init__(args)
        self.window_size = args["num_frames_in_window"]
        self.window_shift = args["num_overlapped_frames_between_windows"]
        self.bad_frames_threshold = args["bad_frames_threshold"]
        self.min_points_in_frame_threshold = args["min_points_in_frame_threshold"]
        self.no_data_padding = args["no_data_padding"]
        self.polar = args["polar"]
        self.frame_size = getattr(args, 'frame_size', PADDING_SIZE)

    def get_samples_validator(self):
        return self.PointCloudSampleValidator(self.min_points_in_frame_threshold,
                                              self.window_size,
                                              self.window_shift,
                                              self.bad_frames_threshold)

    def create_sample_representation(self, data):
        if not self.polar:
            data = [[point[:FEATURES_NUM] for point in frame] for frame in data]
        else:
            data = [[[point[i] for i in
                      [FRAME_IDX, AZIMUTH_IDX, RANGE_IDX, ELEVATION_IDX, V_IDX, S_IDX]]
                     for point in frame] for frame in data]
        data = self.normalize_range_and_eliminate_outliers(data)
        if not self.no_data_padding:
            data = cyclic_pad_data(data, self.frame_size)
        data = torch.tensor(data).to(torch.float32)
        return {"x": data}

    @staticmethod
    def normalize_range_and_eliminate_outliers(data, lower_threshold=-0.25, upper_threshold=MAX_Y_DISTANCE_IN_PC):

        frame_averages = []
        for frame_data in data:
            frame_data = torch.tensor(frame_data)
            top_5_snr_data = frame_data[torch.argsort(frame_data[:, S_IDX], descending=True)][:5]
            avg_range = torch.mean(top_5_snr_data[:, Y_IDX]).item()
            frame_averages.append(avg_range)

        filtered_points = []
        if frame_averages[0] > frame_averages[-1]:
            max_average = max(frame_averages)
            for frame in data:
                frame_points = []
                for point in frame:
                    point[Y_IDX] -= max_average
                    if -upper_threshold <= point[Y_IDX] <= -lower_threshold:
                        frame_points.append(point)
                filtered_points.append(frame_points)
        else:
            min_average = min(frame_averages)
            for frame in data:
                frame_points = []
                for point in frame:
                    point[Y_IDX] -= min_average
                    if lower_threshold <= point[Y_IDX] <= upper_threshold:
                        frame_points.append(point)
                filtered_points.append(frame_points)

        return filtered_points

    class PointCloudSampleValidator:

        def __init__(self, min_points_in_frame_threshold, window_size, window_shift, bad_frames_threshold):
            self.min_points_in_frame_threshold = min_points_in_frame_threshold
            self.window_size = window_size
            self.window_shift = window_shift
            self.bad_frames_threshold = bad_frames_threshold
            self.past_frames = []
            self.bad_frames_in_window = []
            self.last_frame_doppler_sign = 0
            self.valid_frame_count = 1

        def is_valid(self, frame_points, frame_id, last_valid_frame):
            if self.window_size == -1:
                return self.is_valid_full_turn(frame_points, frame_id)

            if type(frame_points) != list:
                frame_points = frame_points.tolist()

            empty_arr = len(frame_points) < self.min_points_in_frame_threshold
            first_frame_of_window = frame_id - self.window_size
            if len(frame_points) < self.min_points_in_frame_threshold:
                return False, first_frame_of_window, self.past_frames
            self.past_frames.append(frame_points)
            self.past_frames = self.past_frames[-self.window_size:]
            self.bad_frames_in_window.append(empty_arr)
            self.bad_frames_in_window = self.bad_frames_in_window[-self.window_size:]
            if (sum(self.bad_frames_in_window) >= self.bad_frames_threshold) or (self.bad_frames_in_window[0]):
                return False, first_frame_of_window, self.past_frames
            window = self.past_frames
            return (len(window) == self.window_size) \
                   and (first_frame_of_window - last_valid_frame >= self.window_shift), first_frame_of_window, window

        def is_valid_full_turn(self, frame_points, frame_id):
            frame_points = np.array(frame_points)
            first_frame_of_window = frame_id - self.valid_frame_count
            if first_frame_of_window == 1700:
                print("now")
            if len(frame_points) == 0:
                return False, first_frame_of_window, self.past_frames
            self.past_frames.append(frame_points.tolist())
            window = self.past_frames[-self.valid_frame_count:]
            self.valid_frame_count = 1
            return len(window) > 30, first_frame_of_window, window