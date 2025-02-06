import numpy as np

from degrees_to_radians import degrees_to_radians
from rotate_vector import rotate_vector

from intersects_obb_aabb import intersects_obb_aabb

obbs = [
    {
        'center': np.array([1.5, 0, 0]),
        'axes': [np.array([1, 0, 0]), np.array([0, 1, 0]), np.array([0, 0, 1])],
        'extents': [1, 1, 1]
    },
    {
        'center': np.array([1.500000000001, 0, 0]),
        'axes': [np.array([1, 0, 0]), np.array([0, 1, 0]), np.array([0, 0, 1])],
        'extents': [1, 1, 1]
    },
    {
        'center': np.array([np.sqrt(2) + 0.5, 0, 0]),
        'axes': [
            rotate_vector(np.array([1, 0, 0]), np.array([0, 1, 0]), degrees_to_radians(45)),
            np.array([0, 1, 0]), 
            rotate_vector(np.array([0, 0, 1]), np.array([0, 1, 0]), degrees_to_radians(45)),
        ],
        'extents': [0.5, 0.5, 0.5]
    },
]

aabbs = [
    {
        'min': np.array([-0.5, -0.5, -0.5]),
        'max': np.array([0.5, 0.5, 0.5])
    },
    {
        'min': np.array([-0.5, -0.5, -0.5]),
        'max': np.array([0.5, 0.5, 0.5])
    },
    {
        'min': np.array([-0.5, -0.5, -0.5]),
        'max': np.array([0.5, 0.5, 0.5])
    },
]

for obb, aabb in zip(obbs, aabbs):
    print(intersects_obb_aabb(obb, aabb))  # Output: True or False
