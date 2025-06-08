using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BeltMeshGenerator : MonoBehaviour
{
    [Header("Belt Settings")]
    public float beltWidth = 2.0f;
    public float beltThickness = 0.2f;
    public Material beltMaterial;
    public int segmentsPerCurve = 20;
    
    [Header("Curve Settings")]
    public PathGenerationType pathType = PathGenerationType.Bezier;
    
    public List<Vector3> GeneratePathWithPoles(List<WaypointData> waypoints)
    {
        List<Vector3> path = new List<Vector3>();
        
        if (waypoints.Count < 2)
        {
            Debug.LogWarning("Need at least 2 waypoints for path generation");
            return path;
        }
        
        // Generate path through all waypoints in order
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 start = waypoints[i].waypoint.position;
            Vector3 end = waypoints[(i + 1) % waypoints.Count].waypoint.position;
            
            List<Vector3> segmentPath = GenerateSegmentPath(start, end);
            
            // Add segment points (except last to avoid duplicates)
            for (int j = 0; j < segmentPath.Count - 1; j++)
            {
                path.Add(segmentPath[j]);
            }
        }
        
        Debug.Log($"Generated belt path with {path.Count} points");
        return path;
    }
    
    List<Vector3> GenerateSegmentPath(Vector3 start, Vector3 end)
    {
        List<Vector3> segmentPath = new List<Vector3>();
        
        if (pathType == PathGenerationType.Bezier)
        {
            Vector3 direction = (end - start);
            Vector3 control1 = start + direction * 0.25f;
            Vector3 control2 = end - direction * 0.25f;
            
            for (int i = 0; i <= segmentsPerCurve; i++)
            {
                float t = (float)i / segmentsPerCurve;
                Vector3 point = CalculateBezierPoint(t, start, control1, control2, end);
                segmentPath.Add(point);
            }
        }
        else
        {
            for (int i = 0; i <= segmentsPerCurve; i++)
            {
                float t = (float)i / segmentsPerCurve;
                Vector3 point = Vector3.Lerp(start, end, t);
                segmentPath.Add(point);
            }
        }
        
        return segmentPath;
    }
    
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
    }
    
    public Mesh GenerateBeltMesh(List<Vector3> path)
    {
        if (path.Count < 2) return new Mesh();
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 current = path[i];
            Vector3 next = path[(i + 1) % path.Count];
            Vector3 forward = (next - current).normalized;
            
            if (forward.magnitude < 0.001f)
                forward = i > 0 ? (current - path[i-1]).normalized : Vector3.forward;
            
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            float halfWidth = beltWidth * 0.5f;
            float halfThickness = beltThickness * 0.5f;
            
            vertices.Add(current + right * halfWidth + Vector3.up * halfThickness);
            vertices.Add(current - right * halfWidth + Vector3.up * halfThickness);
            vertices.Add(current + right * halfWidth - Vector3.up * halfThickness);
            vertices.Add(current - right * halfWidth - Vector3.up * halfThickness);
            
            float uvProgress = (float)i / path.Count;
            uvs.Add(new Vector2(0, uvProgress));
            uvs.Add(new Vector2(1, uvProgress));
            uvs.Add(new Vector2(0, uvProgress));
            uvs.Add(new Vector2(1, uvProgress));
        }
        
        for (int i = 0; i < path.Count; i++)
        {
            int current = i * 4;
            int next = ((i + 1) % path.Count) * 4;
            
            AddQuad(triangles, current + 0, current + 1, next + 0, next + 1);
            AddQuad(triangles, next + 2, next + 3, current + 2, current + 3);
            AddQuad(triangles, next + 0, next + 2, current + 0, current + 2);
            AddQuad(triangles, current + 1, current + 3, next + 1, next + 3);
        }
        
        Mesh mesh = new Mesh();
        mesh.name = "Generated Belt Mesh";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    void AddQuad(List<int> triangles, int v0, int v1, int v2, int v3)
    {
        triangles.Add(v0); triangles.Add(v1); triangles.Add(v2);
        triangles.Add(v1); triangles.Add(v3); triangles.Add(v2);
    }
}