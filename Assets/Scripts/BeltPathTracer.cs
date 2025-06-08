using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BeltPathTracer : MonoBehaviour
{
    [Header("Components")]
    public WaypointManager waypointManager;
    public BeltMeshGenerator meshGenerator;
    public BeltValidator validator;
    
    [Header("Waypoint Organization")]
    public List<WaypointData> aSideWaypoints = new List<WaypointData>();
    public List<WaypointData> bSideWaypoints = new List<WaypointData>();
    public List<WaypointData> controlPoles = new List<WaypointData>();
    public List<WaypointData> rollers = new List<WaypointData>();
    
    [Header("Final Generation List")]
    public List<WaypointData> allWaypoints = new List<WaypointData>();
    
    [Header("Movement")]
    public GameObject targetObject;
    public float moveSpeed = 2f;
    
    [Header("Settings")]
    public bool autoUpdate = true;
    public bool showPath = true;
    
    private List<Vector3> pathPoints = new List<Vector3>();
    private float pathProgress = 0f;
    private ValidationState currentValidation = ValidationState.Valid;
    
    void Start()
    {
        InitializeComponents();
        RebuildAllWaypoints();
    }
    
    void Update()
    {
        if (targetObject != null && pathPoints.Count > 0)
        {
            UpdateTargetMovement();
        }
        
        if (autoUpdate && HasWaypointsMoved())
        {
            RebuildAllWaypoints();
        }
    }
    
    void InitializeComponents()
    {
        if (waypointManager == null) waypointManager = GetComponent<WaypointManager>();
        if (meshGenerator == null) meshGenerator = GetComponent<BeltMeshGenerator>();
        if (validator == null) validator = GetComponent<BeltValidator>();
        
        if (waypointManager == null) waypointManager = gameObject.AddComponent<WaypointManager>();
        if (meshGenerator == null) meshGenerator = gameObject.AddComponent<BeltMeshGenerator>();
        if (validator == null) validator = gameObject.AddComponent<BeltValidator>();
    }
    
    bool HasWaypointsMoved()
    {
        return allWaypoints.Any(wp => wp.waypoint != null && wp.waypoint.hasChanged);
    }
    
    public void RebuildAllWaypoints()
    {
        // Combine all waypoints into one list for generation
        allWaypoints.Clear();
        
        // Add in belt order: A -> B -> D -> F -> E -> C
        var rollerA = rollers.FirstOrDefault(r => r.type == WaypointType.RollerA);
        var rollerB = rollers.FirstOrDefault(r => r.type == WaypointType.RollerB);
        var bPole = controlPoles.FirstOrDefault(p => p.type == WaypointType.BPole);
        var cPole = controlPoles.FirstOrDefault(p => p.type == WaypointType.CPole);
        var dPole = controlPoles.FirstOrDefault(p => p.type == WaypointType.DPole);
        var ePole = controlPoles.FirstOrDefault(p => p.type == WaypointType.EPole);
        
        // Build the belt path in correct order
        if (rollerA.waypoint != null) allWaypoints.Add(rollerA);
        
        // Add A side waypoints
        allWaypoints.AddRange(aSideWaypoints.Where(wp => wp.waypoint != null));
        
        if (bPole.waypoint != null) allWaypoints.Add(bPole);
        if (dPole.waypoint != null) allWaypoints.Add(dPole);
        
        if (rollerB.waypoint != null) allWaypoints.Add(rollerB);
        
        // Add B side waypoints
        allWaypoints.AddRange(bSideWaypoints.Where(wp => wp.waypoint != null));
        
        if (ePole.waypoint != null) allWaypoints.Add(ePole);
        if (cPole.waypoint != null) allWaypoints.Add(cPole);
        
        // Validate the configuration
        currentValidation = validator.ValidateBeltConfiguration(allWaypoints);
        
        // Generate path and mesh
        if (currentValidation != ValidationState.Invalid)
        {
            GeneratePath();
            GenerateMesh();
        }
        
        Debug.Log($"Rebuilt waypoint system with {allWaypoints.Count} points - Status: {currentValidation}");
    }
    
    void GeneratePath()
    {
        pathPoints = meshGenerator.GeneratePathWithPoles(allWaypoints);
    }
    
    void GenerateMesh()
    {
        if (pathPoints.Count > 0)
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
            Mesh beltMesh = meshGenerator.GenerateBeltMesh(pathPoints);
            meshFilter.mesh = beltMesh;
            
            if (meshGenerator.beltMaterial != null)
                meshRenderer.material = meshGenerator.beltMaterial;
        }
    }
    
    void UpdateTargetMovement()
    {
        pathProgress += Time.deltaTime * moveSpeed / GetPathLength();
        pathProgress = pathProgress % 1f;
        
        if (pathPoints.Count > 1)
        {
            float exactIndex = pathProgress * (pathPoints.Count - 1);
            int index = Mathf.FloorToInt(exactIndex);
            int nextIndex = (index + 1) % pathPoints.Count;
            float t = exactIndex - index;
            
            Vector3 position = Vector3.Lerp(pathPoints[index], pathPoints[nextIndex], t);
            Vector3 direction = (pathPoints[nextIndex] - pathPoints[index]).normalized;
            
            targetObject.transform.position = position;
            if (direction != Vector3.zero)
            {
                targetObject.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
    
    float GetPathLength()
    {
        float length = 0f;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            length += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }
        return Mathf.Max(length, 1f);
    }
    
    [ContextMenu("Create Default Setup")]
    public void CreateDefaultSetup()
    {
        var newWaypoints = waypointManager.CreateConveyorSetup();
        OrganizeWaypoints(newWaypoints);
        RebuildAllWaypoints();
    }
    
    void OrganizeWaypoints(List<WaypointData> waypoints)
    {
        rollers.Clear();
        controlPoles.Clear();
        aSideWaypoints.Clear();
        bSideWaypoints.Clear();
        
        foreach (var wp in waypoints)
        {
            switch (wp.type)
            {
                case WaypointType.RollerA:
                case WaypointType.RollerB:
                    rollers.Add(wp);
                    break;
                case WaypointType.BPole:
                case WaypointType.CPole:
                case WaypointType.DPole:
                case WaypointType.EPole:
                    controlPoles.Add(wp);
                    break;
                default:
                    // Add to A side by default, you can manually reorganize
                    aSideWaypoints.Add(wp);
                    break;
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showPath) return;
        
        // Draw path
        if (pathPoints.Count > 1)
        {
            Gizmos.color = GetValidationColor();
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }
        }
        
        // Draw waypoints by category
        DrawWaypointsByCategory();
    }
    
    void DrawWaypointsByCategory()
    {
        // Rollers (Blue spheres)
        Gizmos.color = Color.blue;
        foreach (var roller in rollers)
        {
            if (roller.waypoint != null)
                Gizmos.DrawWireSphere(roller.waypoint.position, 0.3f);
        }
        
        // Control poles (Yellow/Red cubes)
        foreach (var pole in controlPoles)
        {
            if (pole.waypoint == null) continue;
            
            if (pole.type == WaypointType.BPole || pole.type == WaypointType.DPole)
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.red;
            
            Gizmos.DrawWireCube(pole.waypoint.position, Vector3.one * 0.25f);
        }
        
        // A side waypoints (Green)
        Gizmos.color = Color.green;
        foreach (var wp in aSideWaypoints)
        {
            if (wp.waypoint != null)
                Gizmos.DrawWireCube(wp.waypoint.position, Vector3.one * 0.2f);
        }
        
        // B side waypoints (Magenta)
        Gizmos.color = Color.magenta;
        foreach (var wp in bSideWaypoints)
        {
            if (wp.waypoint != null)
                Gizmos.DrawWireCube(wp.waypoint.position, Vector3.one * 0.2f);
        }
    }
    
    Color GetValidationColor()
    {
        switch (currentValidation)
        {
            case ValidationState.Valid: return Color.green;
            case ValidationState.Warning: return Color.yellow;
            case ValidationState.Invalid: return Color.red;
            default: return Color.white;
        }
    }
    
    [ContextMenu("Log Setup Info")]
    public void LogSetupInfo()
    {
        Debug.Log("=== BELT SETUP INFO ===");
        Debug.Log($"Rollers: {rollers.Count}");
        Debug.Log($"Control Poles: {controlPoles.Count}");
        Debug.Log($"A Side Waypoints: {aSideWaypoints.Count}");
        Debug.Log($"B Side Waypoints: {bSideWaypoints.Count}");
        Debug.Log($"Total Waypoints: {allWaypoints.Count}");
        Debug.Log($"Validation: {currentValidation}");
        Debug.Log($"Path Points: {pathPoints.Count}");
    }
}