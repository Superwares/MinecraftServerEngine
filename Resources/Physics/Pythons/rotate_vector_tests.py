import numpy as np

import typing as tp
import numpy.typing as npt


from degrees_to_radians import degrees_to_radians

from rotate_vector import rotate_vector

# Example usage
vectors: tp.List[npt.NDArray[np.float64]] = [
    np.array([1, 0, 0], np.float64),
    np.array([1, 0, 0], np.float64),
    np.array([0.8660254037844387, 0.49999999999999994, 0.0], np.float64),
    np.array([0.8660254037844387, 0.49999999999999994, 0.0], np.float64),
    np.array([0.8660254037844387, 0.49999999999999994, 0.0], np.float64),
    np.array([0.70710678,0.70710678,0.0], np.float64),
]
axes: tp.List[npt.NDArray[np.float64]] = [
    np.array([0, 0, 1], np.float64),
    np.array([0, 0, 1], np.float64),
    np.array([0, 1, 0], np.float64),
    np.array([0, 1, 0], np.float64),
    np.array([0, 1, 0], np.float64),
    np.array([1, 0, 0], np.float64),
]
angles: tp.List[np.float64] = [
    np.pi / 4,  # 45 degrees
    degrees_to_radians(30),
    degrees_to_radians(22),
    degrees_to_radians(40),
    degrees_to_radians(45),
    degrees_to_radians(45),
]

# Rotate each vector
for vector, axis, angle in zip(vectors, axes, angles):
    rotated_vector = rotate_vector(vector, axis, angle)
    # print(rotated_vector)
    print('[' + ', '.join(map(str, rotated_vector)) + ']')
