import numpy as np

from constants import RANGE_IDX, V_IDX, S_IDX

#MIN_NUM_FRAME = 25
MIN_NUM_FRAME = 50

class Gait:
    def validate_for_task(self, radar_data, recording_metadata=None, recording_folder=None, streamer=None):
        if streamer:
            tracks_positions, tracks_velocities = (
                streamer.extract_track_positions_and_velocities(**{"recording_folder": recording_folder,
                                                             "radar_data": radar_data}))
        else:
            tracks_positions, tracks_velocities = self.extract_track_positions_and_velocities(radar_data)

        valid_frames = []
        number_of_tracks = len(tracks_positions)
        is_gait = False
        if number_of_tracks == 1:
            track_positions = list(tracks_positions.values())[0]
            track_velocities = list(tracks_velocities.values())[0]
            valid_frames, max_length = self.get_valid_gait_frames(track_positions, track_velocities)
            if max_length >= MIN_NUM_FRAME:
                is_gait = True

        if recording_metadata:
            recording_metadata["labels"]["number_of_tracks"] = number_of_tracks
            recording_metadata["labels"]["is_gait"] = is_gait

        return is_gait, valid_frames, recording_metadata

    @staticmethod
    def extract_track_positions_and_velocities(radar_data):
        range_values = []
        doppler_values = []

        for frame in radar_data:
            frame = np.array(frame)

            ranges = frame[:, RANGE_IDX]
            dopplers = frame[:, V_IDX]
            snrs = frame[:, S_IDX]

            weighted_avg_range = np.average(ranges, weights=snrs)
            weighted_avg_doppler = np.average(dopplers, weights=snrs)

            range_values.append(weighted_avg_range)
            doppler_values.append(weighted_avg_doppler)

        return {"1": range_values}, {"1": doppler_values}

    @staticmethod
    def get_valid_gait_frames(positions, velocities, min_distance=0.5,
                              max_distance=7.5, min_velocity=0.2):
        max_length = 0
        current_length = 0
        valid_frames = []

        for frame_idx in range(len(positions)):
            if (abs(velocities[frame_idx]) >= min_velocity and min_distance
                    < positions[frame_idx] < max_distance):
                valid_frames.append(frame_idx)
                current_length += 1
            else:
                if current_length > max_length:
                    max_length = current_length
                current_length = 0

        if current_length > max_length:
            max_length = current_length

        return valid_frames, max_length
