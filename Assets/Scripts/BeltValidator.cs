using UnityEngine;
using System.Collections;

public class BeltValidator : MonoBehaviour
{
    [Header("Validation Settings")]
    [Range(0.5f, 3f)] public float optimalRollerDistance = 2f;
    [Range(0.2f, 2f)] public float minimumRollerDistance = 0.8f;
    [Range(5f, 15f)] public float maxRollerDistance = 12f;
    [Range(0f, 90f)] public float maxBendAngle = 60f;
    
    [Header("Ghost Preview")]
    public bool showGhostPreview = true;
    public float ghostFadeTime = 0.5f;
    
    [Header("Current State")]
    public ValidationState currentState = ValidationState.Valid;
    
    private WaypointManager waypointManager;
    private float validationTimer = 0f;
    private bool isValidationStable = false;
    
    private void Start()
    {
        waypointManager = GetComponent<WaypointManager>();
    }
    
    public ValidationState ValidateSetup()
    {
        if (waypointManager == null) return ValidationState.Invalid;
        
        ValidationState distanceState = ValidateRollerDistance();
        ValidationState angleState = ValidateBendAngle();
        ValidationState geometryState = ValidateGeometry();
        
        ValidationState newState = GetWorstState(distanceState, angleState, geometryState);
        
        if (newState != currentState)
        {
            currentState = newState;
            ResetValidationTimer();
        }
        
        UpdateValidationTimer();
        return currentState;
    }
    
    private ValidationState ValidateRollerDistance()
    {
        float distance = waypointManager.GetDistance();
        
        if (distance > maxRollerDistance || distance < minimumRollerDistance)
            return ValidationState.Invalid;
        if (Mathf.Abs(distance - optimalRollerDistance) > 1f)
            return ValidationState.Warning;
        
        return ValidationState.Valid;
    }
    
    private ValidationState ValidateBendAngle()
    {
        float angle = waypointManager.GetBendAngle();
        
        if (angle > maxBendAngle)
            return ValidationState.Invalid;
        if (angle > maxBendAngle * 0.75f)
            return ValidationState.Warning;
        
        return ValidationState.Valid;
    }
    
    private ValidationState ValidateGeometry()
    {
        // Additional geometry validation can be added here
        return ValidationState.Valid;
    }
    
    private ValidationState GetWorstState(params ValidationState[] states)
    {
        foreach (var state in states)
        {
            if (state == ValidationState.Invalid) return ValidationState.Invalid;
        }
        foreach (var state in states)
        {
            if (state == ValidationState.Warning) return ValidationState.Warning;
        }
        return ValidationState.Valid;
    }
    
    private void ResetValidationTimer()
    {
        validationTimer = 0f;
        isValidationStable = false;
    }
    
    private void UpdateValidationTimer()
    {
        if (currentState == ValidationState.Valid)
        {
            validationTimer += Time.deltaTime;
            if (validationTimer >= ghostFadeTime)
            {
                isValidationStable = true;
            }
        }
        else
        {
            validationTimer = 0f;
            isValidationStable = false;
        }
    }
    
    public Color GetValidationColor()
    {
        Color baseColor = currentState switch
        {
            ValidationState.Valid => Color.green,
            ValidationState.Warning => Color.yellow,
            ValidationState.Invalid => Color.red,
            _ => Color.white
        };
        
        if (showGhostPreview && !isValidationStable)
        {
            float alpha = currentState == ValidationState.Valid ? 
                         Mathf.Lerp(0.8f, 0.2f, validationTimer / ghostFadeTime) : 0.6f;
            baseColor.a = alpha;
        }
        
        return baseColor;
    }
    
    public bool ShouldGenerateMesh()
    {
        return currentState == ValidationState.Valid && isValidationStable;
    }
    
    private void OnDrawGizmos()
    {
        if (!showGhostPreview) return;
        
        Gizmos.color = GetValidationColor();
        
        if (waypointManager != null && waypointManager.rollerA != null && waypointManager.rollerB != null)
        {
            Gizmos.DrawLine(waypointManager.rollerA.position, waypointManager.rollerB.position);
            Gizmos.DrawWireSphere(waypointManager.rollerA.position, 0.2f);
            Gizmos.DrawWireSphere(waypointManager.rollerB.position, 0.2f);
        }
    }
}