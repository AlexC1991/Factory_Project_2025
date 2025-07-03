using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BeltPathTracer : MonoBehaviour
{
    [Header("Components")]
    public WaypointManager waypointManager;
    public BeltMeshGenerator meshGenerator;
    public BeltValidator validator;

    [Header("Waypoint Lists (Pre-populated)")]
    public List<WaypointData> aSidePoints = new List<WaypointData>();
    public List<WaypointData> bSidePoints = new List<WaypointData>();
    public List<WaypointData> allWaypoints = new List<WaypointData>();
    public List<RollerData> rollers = new List<RollerData>();

    [Header("Settings")]
    public bool autoUpdate = true;
    public bool showPath = true;

    private List<Vector3> pathPoints = new List<Vector3>();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private bool isInitialized = false;

    private void Start()
    {
        InitializeComponents();
        Invoke(nameof(DelayedInitialization), 0.1f);
    }

    private void DelayedInitialization()
    {
        SetupInitialConfiguration();
        isInitialized = true;
    }

    private void InitializeComponents()
    {
        if (waypointManager == null) waypointManager = GetComponent<WaypointManager>();
        if (meshGenerator == null) meshGenerator = gameObject.AddComponent<BeltMeshGenerator>();
        if (validator == null) validator = gameObject.AddComponent<BeltValidator>();

        meshFilter = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
    }

    private void SetupInitialConfiguration()
    {
        InitializeRollers();
        StoreInitialWaypointPositions();
        RebuildPath();
    }

    private void InitializeRollers()
    {
        rollers.Clear();

        if (waypointManager.rollerA != null)
            rollers.Add(new RollerData(waypointManager.rollerA, RollerType.SideA));

        if (waypointManager.rollerB != null)
            rollers.Add(new RollerData(waypointManager.rollerB, RollerType.SideB));

        for (int i = 0; i < rollers.Count; i++)
        {
            var roller = rollers[i];
            roller.StoreInitialPosition();
            rollers[i] = roller;
        }
    }

    private void StoreInitialWaypointPositions()
    {
        for (int i = 0; i < aSidePoints.Count; i++)
        {
            var point = aSidePoints[i];
            point.StoreInitialPosition();
            aSidePoints[i] = point;
        }

        for (int i = 0; i < bSidePoints.Count; i++)
        {
            var point = bSidePoints[i];
            point.StoreInitialPosition();
            bSidePoints[i] = point;
        }
    }

    public void OnRollersChanged()
    {
        if (!autoUpdate || !isInitialized) return;

        UpdateWaypointPositions();
        RebuildPath();
    }

    private void UpdateWaypointPositions()
    {
        var rollerA = rollers.FirstOrDefault(r => r.type == RollerType.SideA);
        if (rollerA.roller != null && rollerA.HasMoved())
        {
            Vector3 movement = rollerA.GetMovementFromInitial();

            for (int i = 0; i < aSidePoints.Count; i++)
            {
                var point = aSidePoints[i];
                point.UpdatePosition(movement);
                aSidePoints[i] = point;
            }

            rollerA.UpdateLastPosition();
            int index = rollers.FindIndex(r => r.type == RollerType.SideA);
            rollers[index] = rollerA;
        }

        var rollerB = rollers.FirstOrDefault(r => r.type == RollerType.SideB);
        if (rollerB.roller != null && rollerB.HasMoved())
        {
            Vector3 movement = rollerB.GetMovementFromInitial();

            for (int i = 0; i < bSidePoints.Count; i++)
            {
                var point = bSidePoints[i];
                point.UpdatePosition(movement);
                bSidePoints[i] = point;
            }

            rollerB.UpdateLastPosition();
            int index = rollers.FindIndex(r => r.type == RollerType.SideB);
            rollers[index] = rollerB;
        }
    }

    private void RebuildWaypointList()
    {
        allWaypoints.Clear();

        // Add ALL A side waypoints in order
        var orderedASide = aSidePoints.Where(wp => wp.isActive).OrderBy(wp => wp.pathOrder);
        allWaypoints.AddRange(orderedASide);

        // Add ALL B side waypoints in order
        var orderedBSide = bSidePoints.Where(wp => wp.isActive).OrderBy(wp => wp.pathOrder);
        allWaypoints.AddRange(orderedBSide);

        Debug.Log($"Rebuilt waypoint list: {allWaypoints.Count} total waypoints");
    }

    public void RebuildPath()
    {
        // Combine A Side Points and B Side Points into All Waypoints
        RebuildWaypointList();

        // Ensure there are enough waypoints to generate the path
        if (allWaypoints.Count >= 2)
        {
            GeneratePath();
            GenerateMesh();
        }
    }

    private void GeneratePath()
    {
        pathPoints = meshGenerator.GeneratePathWithWaypoints(allWaypoints);
    }

    private void GenerateMesh()
    {
        if (pathPoints.Count > 0)
        {
            Mesh beltMesh = meshGenerator.GenerateBeltMesh(pathPoints);
            meshFilter.mesh = beltMesh;

            if (meshGenerator.beltMaterial != null)
                meshRenderer.material = meshGenerator.beltMaterial;
        }
    }

    [ContextMenu("Manual Rebuild")]
    public void ManualRebuild()
    {
        if (isInitialized)
            RebuildPath();
    }

    private void OnDrawGizmos()
    {
        if (!showPath || !isInitialized) return;

        if (pathPoints.Count > 1)
        {
            Gizmos.color = validator != null ? validator.GetValidationColor() : Color.green;

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }
        }

        DrawWaypoints();
    }

    private void DrawWaypoints()
    {
        // Draw A side waypoints (Green cubes)
        Gizmos.color = Color.green;
        foreach (var wp in aSidePoints)
        {
            if (wp.waypoint != null && wp.isActive)
                Gizmos.DrawWireCube(wp.waypoint.position, Vector3.one * 0.15f);
        }

        // Draw B side waypoints (Magenta cubes)
        Gizmos.color = Color.magenta;
        foreach (var wp in bSidePoints)
        {
            if (wp.waypoint != null && wp.isActive)
                Gizmos.DrawWireCube(wp.waypoint.position, Vector3.one * 0.15f);
        }
    }
}