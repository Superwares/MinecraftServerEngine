import numpy as np

# Function to check if two ranges overlap
def ranges_overlap(min1, max1, min2, max2):
    return max1 >= min2 and max2 >= min1

# Function to get OBB corners
def get_obb_corners(obb):
    center = obb['center']
    extents = obb['extents']
    axes = obb['axes']
    corners = []
    for i in [-1, 1]:
        for j in [-1, 1]:
            for k in [-1, 1]:
                corner = center + i * extents[0] * axes[0] + j * extents[1] * axes[1] + k * extents[2] * axes[2]
                corners.append(corner)
    return corners

# Function to project an OBB onto an axis
def project_obb(obb, axis):
    corners = get_obb_corners(obb)
    projections = [np.dot(corner, axis) for corner in corners]
    return min(projections), max(projections)

# Function to project an AABB onto an axis
def project_aabb(aabb, axis):
    min_corner = aabb['min']
    max_corner = aabb['max']
    corners = [
        min_corner,
        max_corner,
        np.array([min_corner[0], min_corner[1], max_corner[2]]),
        np.array([min_corner[0], max_corner[1], min_corner[2]]),
        np.array([max_corner[0], min_corner[1], min_corner[2]]),
        np.array([min_corner[0], max_corner[1], max_corner[2]]),
        np.array([max_corner[0], min_corner[1], max_corner[2]]),
        np.array([max_corner[0], max_corner[1], min_corner[2]])
    ]
    projections = [np.dot(corner, axis) for corner in corners]
    return min(projections), max(projections)

# Function to check if OBB and AABB intersect
def intersects_obb_aabb(obb, aabb):
    # Define the axes to check
    axes = [np.array([1, 0, 0]), np.array([0, 1, 0]), np.array([0, 0, 1])]
    
    for i in range(3):
        axes.append(obb['axes'][i])
        for j in range(3):
            cross_product = np.cross(obb['axes'][i], axes[j])
            if np.linalg.norm(cross_product) > 1e-8:
                axes.append(cross_product / np.linalg.norm(cross_product))
    
    for axis in axes:
        axis = axis / np.linalg.norm(axis)
        
        obb_min, obb_max = project_obb(obb, axis)
        aabb_min, aabb_max = project_aabb(aabb, axis)
        
        if not ranges_overlap(obb_min, obb_max, aabb_min, aabb_max):
            return False
    
    return True