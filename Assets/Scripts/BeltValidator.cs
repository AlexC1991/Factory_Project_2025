using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BeltValidator : MonoBehaviour
{
    [Header("Validation Settings")]
    [Range(0.5f, 3f)]
    public float optimalRollerDistance = 2f;
    [Range(0.2f, 2f)]
    public float minimumRollerDistance = 0.8f;
    [Range(0.2f, 1f)]
    public float minPoleDistance = 0.3f;
    [Range(5f, 15f)]
    public float maxRollerDistance = 12f;
    
    public ValidationState ValidateBeltConfiguration(List<WaypointData> waypoints)
    {
        var mainPoints = waypoints
            .Where(wp => wp.waypoint != null && wp.isActive && 
                   (wp.type == WaypointType.RollerA || wp.type == WaypointType.RollerB))
            .ToList();
        
        var poles = waypoints
            .Where(wp => wp.waypoint != null && wp.isActive && 
                   (wp.type == WaypointType.BPole || wp.type == WaypointType.CPole || 
                    wp.type == WaypointType.DPole || wp.type == WaypointType.EPole))
            .ToList();
        
        // Invalid cases
        if (mainPoints.Count < 2)
        {
            Debug.Log("Validation: INVALID - Need at least 2 rollers");
            return ValidationState.Invalid;
        }
        
        var distanceCheck = CheckRollerDistances(mainPoints);
        if (distanceCheck == ValidationState.Invalid)
        {
            Debug.Log("Validation: INVALID - Rollers too close or too far");
            return ValidationState.Invalid;
        }
        
        if (HasOverlappingPositions(waypoints))
        {
            Debug.Log("Validation: INVALID - Overlapping waypoints");
            return ValidationState.Invalid;
        }
        
        // Warning cases
        if (distanceCheck == ValidationState.Warning)
        {
            Debug.Log("Validation: WARNING - Suboptimal roller distances");
            return ValidationState.Warning;
        }
        
        if (poles.Count < 4)
        {
            Debug.Log($"Validation: WARNING - Need 4 control poles, have {poles.Count}");
            return ValidationState.Warning;
        }
        
        if (!ArePolePositionsOptimal(mainPoints, poles))
        {
            Debug.Log("Validation: WARNING - Poles not optimally positioned");
            return ValidationState.Warning;
        }
        
        // Valid case
        Debug.Log("Validation: VALID - Good 6-point belt configuration");
        return ValidationState.Valid;
    }
    
    ValidationState CheckRollerDistances(List<WaypointData> rollers)
    {
        bool hasWarning = false;
        
        for (int i = 0; i < rollers.Count; i++)
        {
            for (int j = i + 1; j < rollers.Count; j++)
            {
                float distance = Vector3.Distance(
                    rollers[i].waypoint.position, 
                    rollers[j].waypoint.position
                );
                
                if (distance < minimumRollerDistance)
                {
                    Debug.Log($"Rollers too close: {distance:F2} < {minimumRollerDistance}");
                    return ValidationState.Invalid;
                }
                
                if (distance > maxRollerDistance)
                {
                    Debug.Log($"Rollers too far: {distance:F2} > {maxRollerDistance}");
                    return ValidationState.Invalid;
                }
                
                if (distance < optimalRollerDistance * 0.7f || distance > optimalRollerDistance * 1.5f)
                {
                    Debug.Log($"Suboptimal roller distance: {distance:F2} (optimal: {optimalRollerDistance})");
                    hasWarning = true;
                }
            }
        }
        
        return hasWarning ? ValidationState.Warning : ValidationState.Valid;
    }
    
    bool HasOverlappingPositions(List<WaypointData> waypoints)
    {
        var activeWaypoints = waypoints.Where(wp => wp.waypoint != null && wp.isActive).ToList();
        
        for (int i = 0; i < activeWaypoints.Count; i++)
        {
            for (int j = i + 1; j < activeWaypoints.Count; j++)
            {
                float distance = Vector3.Distance(
                    activeWaypoints[i].waypoint.position,
                    activeWaypoints[j].waypoint.position
                );
                
                if (distance < 0.1f)
                {
                    Debug.Log("Found overlapping waypoints");
                    return true;
                }
            }
        }
        
        return false;
    }
    
    bool ArePolePositionsOptimal(List<WaypointData> rollers, List<WaypointData> poles)
    {
        if (poles.Count < 4) return false;
        
        Vector3 rollerCenter = Vector3.zero;
        foreach (var roller in rollers)
        {
            rollerCenter += roller.waypoint.position;
        }
        rollerCenter /= rollers.Count;
        
        foreach (var pole in poles)
        {
            float distanceFromCenter = Vector3.Distance(pole.waypoint.position, rollerCenter);
            
            if (distanceFromCenter < 1f || distanceFromCenter > 5f)
            {
                Debug.Log($"Pole {pole.type} not optimally positioned: distance {distanceFromCenter:F2} from center");
                return false;
            }
            
            foreach (var roller in rollers)
            {
                float distanceToRoller = Vector3.Distance(pole.waypoint.position, roller.waypoint.position);
                if (distanceToRoller < minPoleDistance)
                {
                    Debug.Log($"Pole {pole.type} too close to roller: {distanceToRoller:F2}");
                    return false;
                }
            }
        }
        
        return true;
    }
}