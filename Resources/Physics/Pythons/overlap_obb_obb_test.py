import numpy as np

from overlap_obb_obb import overlap_obb_obb
from rotate_vector import rotate_vector
from degrees_to_radians import degrees_to_radians

obb_tests = [
    {
        "obb0": {
            'center': np.array([1.5, 0, 0]),
            'axes': [np.array([1, 0, 0]), np.array([0, 1, 0]), np.array([0, 0, 1])],
            'extents': [1, 1, 1]
        },
        "obb1": {
            'center': np.array([1.500000000001, 0, 0]),
            'axes': [np.array([1, 0, 0]), np.array([0, 1, 0]), np.array([0, 0, 1])],
            'extents': [1, 1, 1]
        },
        "expected": True,
    },
    {
        "obb0": {
            'center': np.array([0, 0, 0]),
            'axes': [np.array([1, 0, 0]), np.array([0, 1, 0]), np.array([0, 0, 1])],
            'extents': [1, 1, 1],
        },
        "obb1": {
            'center': np.array([np.sqrt(2) + 1, 0, 0]),
            'axes': [
                rotate_vector(np.array([1, 0, 0]), np.array([0, 1, 0]), degrees_to_radians(45)),
                np.array([0, 1, 0]), 
                rotate_vector(np.array([0, 0, 1]), np.array([0, 1, 0]), degrees_to_radians(45)),
            ],
            'extents': [1, 1, 1],
        },
        "expected": True,
    },
    {
        "obb0": {
            'center': np.array([0, 0, 0]),
            'axes': [np.array([1, 0, 0]), np.array([0, 1, 0]), np.array([0, 0, 1])],
            'extents': [1, 1, 1],
        },
        "obb1": {
            'center': np.array([np.sqrt(2) + 1.000001, 0, 0]),
            'axes': [
                rotate_vector(np.array([1, 0, 0]), np.array([0, 1, 0]), degrees_to_radians(45)),
                np.array([0, 1, 0]), 
                rotate_vector(np.array([0, 0, 1]), np.array([0, 1, 0]), degrees_to_radians(45)),
            ],
            'extents': [1, 1, 1],
        },
        "expected": False,
    },
    {
        "obb0": {
            'center': np.array([-np.sqrt(2), 0, 0]),
            'axes': [
                rotate_vector(np.array([1, 0, 0]), np.array([0, 1, 0]), degrees_to_radians(45)),
                np.array([0, 1, 0]), 
                rotate_vector(np.array([0, 0, 1]), np.array([0, 1, 0]), degrees_to_radians(45)),
            ],
            'extents': [1, 1, 1],
        },
        "obb1": {
            'center': np.array([np.sqrt(2), 0, 0]),
            'axes': [
                rotate_vector(np.array([1, 0, 0]), np.array([0, 1, 0]), degrees_to_radians(45)),
                np.array([0, 1, 0]), 
                rotate_vector(np.array([0, 0, 1]), np.array([0, 1, 0]), degrees_to_radians(45)),
            ],
            'extents': [1, 1, 1],
        },
        "expected": True,
    },
    {
        "obb0": {
            'center': np.array([-np.sqrt(2), 0, 0]),
            'axes': [
                rotate_vector(np.array([1, 0, 0]), np.array([0, 1, 0]), degrees_to_radians(45)),
                np.array([0, 1, 0]), 
                rotate_vector(np.array([0, 0, 1]), np.array([0, 1, 0]), degrees_to_radians(45)),
            ],
            'extents': [1 - 0.0000000000001, 1, 1],
        },
        "obb1": {
            'center': np.array([np.sqrt(2), 0, 0]),
            'axes': [
                rotate_vector(np.array([1, 0, 0]), np.array([0, 1, 0]), degrees_to_radians(45)),
                np.array([0, 1, 0]), 
                rotate_vector(np.array([0, 0, 1]), np.array([0, 1, 0]), degrees_to_radians(45)),
            ],
            'extents': [1, 1, 1],
        },
        "expected": False,
    },
]

for obb_test in obb_tests:
    obb0 = obb_test["obb0"]
    obb1 = obb_test["obb1"]
    expected = obb_test["expected"]

    result = overlap_obb_obb(obb0, obb1)
    assert result == expected, f"expected {expected} but got {result}"

    print(result)

