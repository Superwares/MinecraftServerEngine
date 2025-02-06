import numpy as np

from typing import List, Tuple

from get_potential_axes import get_potential_axes
from get_axis_interval import get_axis_interval
from get_obb_vertices import get_obb_vertices

THREE_DIMENSIONS = 3

def overlap_obb_obb(obb0, obb1) -> bool:
    axes = get_potential_axes(np.concatenate((obb0['axes'], obb1['axes'])))

    for _, ax_i in enumerate(axes):
        (cls_min_i, cls_max_i) = get_axis_interval(ax_i, get_obb_vertices(obb0))
        (obj_min_i, obj_max_i) = get_axis_interval(ax_i, get_obb_vertices(obb1))

        if cls_max_i < obj_min_i or cls_min_i > obj_max_i:
            return False

    return True