import torch

from constants import S_IDX, FEATURES_NUM, PADDING_SIZE


def cyclic_pad_data(frames, frame_size):
    padded_frames = []
    for frame_points in frames:
        frame_points = padd_frame(frame_points, frame_size)
        padded_frames.append(frame_points.tolist())
    return padded_frames


def padd_frame(frame_points, frame_size):
    frame_points = torch.tensor(sorted(frame_points, key=lambda x: x[S_IDX], reverse=True))
    if len(frame_points) == 0:
        padded_tensor = torch.zeros((PADDING_SIZE, FEATURES_NUM))
    else:
        num_padding_rows = PADDING_SIZE - frame_points.size(0)
        if num_padding_rows > 0:
            random_indices = torch.randint(0, frame_points.size(0), (num_padding_rows,))
            padding = frame_points[random_indices]
            padded_tensor = torch.cat([frame_points, padding], dim=0)
        elif num_padding_rows < 0:
            padded_tensor = frame_points[:PADDING_SIZE]
        else:
            padded_tensor = frame_points

    assert len(padded_tensor) == frame_size, f"frame_points size is not {frame_size} but {len(frame_points)}"
    return padded_tensor.numpy()
