using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum WaypointType
{
    Manual, SideA, SideB, Fixed
}

[System.Serializable]
public enum RollerType
{
    SideA, SideB
}

[System.Serializable]
public enum PathGenerationType
{
    Auto, Linear, Bezier, CatmullRom
}

[System.Serializable]
public enum ValidationState
{
    Valid, Warning, Invalid
}

[System.Serializable]
public struct WaypointData
{
    public Transform waypoint;
    public WaypointType type;
    public Vector3 initialPosition;
    public Vector3 movementMultiplier;
    public bool isActive;
    [Range(0, 10)]
    public int pathOrder;
    
    public WaypointData(Transform wp, WaypointType wpType, Vector3 multiplier = default, int order = 0)
    {
        waypoint = wp;
        type = wpType;
        initialPosition = wp != null ? wp.position : Vector3.zero;
        movementMultiplier = multiplier == default ? Vector3.one : multiplier;
        isActive = wp != null;
        pathOrder = order;
    }
    
    public void StoreInitialPosition()
    {
        if (waypoint != null)
            initialPosition = waypoint.position;
    }
    
    public void UpdatePosition(Vector3 rollerMovement)
    {
        if (waypoint != null && isActive)
        {
            Vector3 scaledMovement = Vector3.Scale(rollerMovement, movementMultiplier);
            waypoint.position = initialPosition + scaledMovement;
        }
    }
}

[System.Serializable]
public struct RollerData
{
    public Transform roller;
    public RollerType type;
    public Vector3 initialPosition;
    public Vector3 lastPosition;
    public bool isActive;
    
    public RollerData(Transform rollerTransform, RollerType rollerType)
    {
        roller = rollerTransform;
        type = rollerType;
        initialPosition = rollerTransform != null ? rollerTransform.position : Vector3.zero;
        lastPosition = initialPosition;
        isActive = rollerTransform != null;
    }
    
    public void StoreInitialPosition()
    {
        if (roller != null)
        {
            initialPosition = roller.position;
            lastPosition = roller.position;
        }
    }
    
    public Vector3 GetMovementFromInitial()
    {
        return roller != null ? roller.position - initialPosition : Vector3.zero;
    }
    
    public bool HasMoved(float threshold = 0.01f)
    {
        return roller != null && 
               Vector3.Distance(roller.position, lastPosition) > threshold;
    }
    
    public void UpdateLastPosition()
    {
        if (roller != null)
            lastPosition = roller.position;
    }
}

[System.Serializable]
public struct BeltSegment
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    public Vector3 controlPoint1;
    public Vector3 controlPoint2;
    public PathGenerationType curveType;
    public WaypointType startType;
    public WaypointType endType;
}