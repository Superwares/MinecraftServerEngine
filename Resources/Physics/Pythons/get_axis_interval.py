import numpy as np

import numpy.typing as npt
from typing import List, Tuple

def get_axis_interval(
        axis: List[float], 
        vertices: List[List[float]],
    ) -> Tuple[float, float]:
    """
    Description:
        The function to get the interval of the shapes specified by the axis.

    Args:
        (1) axis [Vector<float> 1x3]: The individual axes of the bounding box (OBB, AABB) generated 
                                      from the Overlap function.
        (2) verts [Vector<float> 8x3]: The vertices of the object.

    Returns:
        (1, 2) parameter [float]: Minimum and maximum projection in an interval structure.
    """
            
    out_min = np.dot(axis, vertices[0]);
    out_max = out_min.copy()

    # Projection of individual vertices on the specified axes.
    for _, verts_i in enumerate(vertices):
        # Projection of the axis onto the individual vertices 
        # of the bounding box (OBB, AABB).
        projection = np.dot(axis, verts_i);
        
        # Store the minimum and maximum projection in an interval structure.
        out_min = projection if projection < out_min else out_min
        out_max = projection if projection > out_max else out_max

    return [out_min, out_max]