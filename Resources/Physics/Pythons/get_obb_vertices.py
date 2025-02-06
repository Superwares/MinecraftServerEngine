import numpy as np

from typing import List, Tuple

def get_obb_vertices(obb) -> List[float]:
    center = np.array(obb['center'])
    axes = [np.array(axis) for axis in obb['axes']]
    extents = np.array(obb['extents'])

    # Calculate the vertices
    vertices = []
    for dx in [-1, 1]:
        for dy in [-1, 1]:
            for dz in [-1, 1]:
                corner = center + dx * extents[0] * axes[0] + dy * extents[1] * axes[1] + dz * extents[2] * axes[2]
                vertices.append(corner)

    return np.array(vertices)