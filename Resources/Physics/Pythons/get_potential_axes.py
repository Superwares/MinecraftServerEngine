import numpy as np

from typing import List, Tuple

THREE_DIMENSIONS = 3

def get_potential_axes(axes: List[List[float]]) -> List[List[float]]:

    assert len(axes[0]) == THREE_DIMENSIONS, "The axes must be a 3D vector."

    potential_axes = [None] * 15
    potential_axes[0] = axes[0]
    potential_axes[1] = axes[1]
    potential_axes[2] = axes[2]
    potential_axes[3] = axes[3]
    potential_axes[4] = axes[4]
    potential_axes[5] = axes[5]

    for i in range(0, THREE_DIMENSIONS):
        potential_axes[6 + i * 3] = np.cross(axes[i], axes[3])
        potential_axes[7 + i * 3] = np.cross(axes[i], axes[4])
        potential_axes[8 + i * 3] = np.cross(axes[i], axes[5])

    return potential_axes