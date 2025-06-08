using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    [Header("Pole Settings")]
    public float poleRadius = 0.1f;
    public float poleHeight = 0.3f;
    public Material cPoleMaterial;
    public Material bPoleMaterial;
    
    [Header("Conveyor Dimensions")]
    public float conveyorLength = 4f;
    public float conveyorWidth = 2f;
    public float beltHeight = 1f;
    public float poleOffset = 1.5f;
    
    private GameObject poleContainer;
    
    public List<WaypointData> CreateConveyorSetup()
    {
        ClearAllWaypoints();
        CreatePoleContainer();
        
        List<WaypointData> newWaypoints = new List<WaypointData>();
        Vector3 center = transform.position;
        
        // Create 6-point belt system
        var waypointConfigs = new[]
        {
            new { pos = center + new Vector3(-conveyorLength/2, beltHeight, 0), type = WaypointType.RollerA, name = "A_RollerStart", order = 0 },
            new { pos = center + new Vector3(-conveyorLength/4, beltHeight, conveyorWidth/2 + poleOffset), type = WaypointType.BPole, name = "B_Pole_TopA", order = 1 },
            new { pos = center + new Vector3(-conveyorLength/4, beltHeight, -conveyorWidth/2 - poleOffset), type = WaypointType.CPole, name = "C_Pole_BottomA", order = 2 },
            new { pos = center + new Vector3(conveyorLength/4, beltHeight, conveyorWidth/2 + poleOffset), type = WaypointType.DPole, name = "D_Pole_TopF", order = 3 },
            new { pos = center + new Vector3(conveyorLength/4, beltHeight, -conveyorWidth/2 - poleOffset), type = WaypointType.EPole, name = "E_Pole_BottomF", order = 4 },
            new { pos = center + new Vector3(conveyorLength/2, beltHeight, 0), type = WaypointType.RollerB, name = "F_RollerEnd", order = 5 }
        };
        
        foreach (var config in waypointConfigs)
        {
            GameObject waypoint = new GameObject(config.name);
            waypoint.transform.position = config.pos;
            waypoint.transform.parent = poleContainer.transform;
            
            // Create visual for poles
            if (config.type != WaypointType.RollerA && config.type != WaypointType.RollerB)
            {
                CreatePoleVisual(waypoint, config.type);
            }
            
            var wpData = new WaypointData(waypoint.transform, config.type, Vector3.one, config.order);
            wpData.isActive = true;
            newWaypoints.Add(wpData);
        }
        
        Debug.Log("Created 6-point belt system");
        return newWaypoints;
    }
    
    void CreatePoleContainer()
    {
        poleContainer = GameObject.Find("Belt Poles");
        if (poleContainer == null)
        {
            poleContainer = new GameObject("Belt Poles");
            poleContainer.transform.parent = transform;
        }
    }
    
    void CreatePoleVisual(GameObject parent, WaypointType poleType)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.transform.parent = parent.transform;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(poleRadius * 2f, poleHeight, poleRadius * 2f);
        visual.name = "Visual";
        
        var renderer = visual.GetComponent<Renderer>();
        
        if ((poleType == WaypointType.BPole || poleType == WaypointType.DPole) && cPoleMaterial != null)
            renderer.material = cPoleMaterial;
        else if ((poleType == WaypointType.CPole || poleType == WaypointType.EPole) && bPoleMaterial != null)
            renderer.material = bPoleMaterial;
    }
    
    public void ClearAllWaypoints()
    {
        if (poleContainer != null)
        {
            if (Application.isPlaying)
                DestroyImmediate(poleContainer);
            else
                DestroyImmediate(poleContainer);
        }
    }
}