import numpy as np

def rotate_vector(vector, axis, angle):
    """
    Rotate a vector around a given axis by a certain angle.

    Parameters:
    vector (np.ndarray): The vector to be rotated.
    axis (np.ndarray): The axis around which to rotate.
    angle (float): The angle of rotation in radians.

    Returns:
    np.ndarray: The rotated vector.
    """
    # Normalize the axis
    axis = axis / np.linalg.norm(axis)
    
    # Compute the rotation matrix using the Rodrigues' rotation formula
    cos_theta = np.cos(angle)
    sin_theta = np.sin(angle)
    one_minus_cos_theta = 1 - cos_theta
    
    ux, uy, uz = axis
    rotation_matrix = np.array([
        [cos_theta + ux**2 * one_minus_cos_theta,
         ux*uy*one_minus_cos_theta - uz*sin_theta,
         ux*uz*one_minus_cos_theta + uy*sin_theta],
        [uy*ux*one_minus_cos_theta + uz*sin_theta,
         cos_theta + uy**2 * one_minus_cos_theta,
         uy*uz*one_minus_cos_theta - ux*sin_theta],
        [uz*ux*one_minus_cos_theta - uy*sin_theta,
         uz*uy*one_minus_cos_theta + ux*sin_theta,
         cos_theta + uz**2 * one_minus_cos_theta]
    ])
    
    # Rotate the vector
    rotated_vector = np.dot(rotation_matrix, vector)
    
    return rotated_vector
