using UnityEngine;
using System.Collections.Generic;

public class BeltMeshGenerator : MonoBehaviour
{
    public float beltWidth = 2.0f;
    public float beltThickness = 0.2f;
    public Material beltMaterial;
    public int segmentsPerCurve = 20;
    public PathGenerationType pathType = PathGenerationType.Linear; // Or Bezier/CatmullRom/Auto

    public List<Vector3> GeneratePathWithWaypoints(List<WaypointData> waypoints, Vector3[] controlPoints = null)
    {
        List<Vector3> path = new List<Vector3>();
        if (waypoints.Count < 2) return path;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 start = waypoints[i].waypoint.position;
            Vector3 end = waypoints[i + 1].waypoint.position;

            List<Vector3> segmentPath = null;
            switch (pathType)
            {
                case PathGenerationType.Bezier:
                    segmentPath = GenerateBezierSegment(start, end);
                    break;
                case PathGenerationType.CatmullRom:
                    segmentPath = GenerateCatmullRomSegment(waypoints, i);
                    break;
                default:
                    segmentPath = GenerateLinearSegment(start, end);
                    break;
            }

            for (int j = 0; j < segmentPath.Count - 1; j++)
                path.Add(segmentPath[j]);
        }
        path.Add(waypoints[waypoints.Count - 1].waypoint.position);
        return path;
    }

    private List<Vector3> GenerateLinearSegment(Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();
        for (int i = 0; i <= segmentsPerCurve; i++)
        {
            float t = (float)i / segmentsPerCurve;
            path.Add(Vector3.Lerp(start, end, t));
        }
        return path;
    }

    private List<Vector3> GenerateBezierSegment(Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 direction = (end - start);
        Vector3 control1 = start + direction * 0.25f;
        Vector3 control2 = end - direction * 0.25f;
        for (int i = 0; i <= segmentsPerCurve; i++)
        {
            float t = (float)i / segmentsPerCurve;
            path.Add(CalculateBezierPoint(t, start, control1, control2, end));
        }
        return path;
    }

    private List<Vector3> GenerateCatmullRomSegment(List<WaypointData> waypoints, int i)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 p0 = i > 0 ? waypoints[i - 1].waypoint.position : waypoints[i].waypoint.position;
        Vector3 p1 = waypoints[i].waypoint.position;
        Vector3 p2 = waypoints[i + 1].waypoint.position;
        Vector3 p3 = (i + 2 < waypoints.Count) ? waypoints[i + 2].waypoint.position : waypoints[i + 1].waypoint.position;
        for (int j = 0; j <= segmentsPerCurve; j++)
        {
            float t = (float)j / segmentsPerCurve;
            path.Add(CalculateCatmullRomPoint(t, p0, p1, p2, p3));
        }
        return path;
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
    }

    private Vector3 CalculateCatmullRomPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ((2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
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
            Vector3 forward = (i < path.Count - 1) ? (path[i + 1] - current).normalized : (current - path[i - 1]).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            float halfWidth = beltWidth * 0.5f;
            float halfThickness = beltThickness * 0.5f;

            vertices.Add(current + right * halfWidth + Vector3.up * halfThickness);
            vertices.Add(current - right * halfWidth + Vector3.up * halfThickness);
            vertices.Add(current + right * halfWidth - Vector3.up * halfThickness);
            vertices.Add(current - right * halfWidth - Vector3.up * halfThickness);

            float uvProgress = (float)i / (path.Count - 1);
            uvs.Add(new Vector2(1, uvProgress));
            uvs.Add(new Vector2(0, uvProgress));
            uvs.Add(new Vector2(1, uvProgress));
            uvs.Add(new Vector2(0, uvProgress));
        }

        for (int i = 0; i < path.Count - 1; i++)
        {
            int current = i * 4;
            int next = (i + 1) * 4;
            AddQuad(triangles, current, current + 1, next, next + 1);
            AddQuad(triangles, next + 2, next + 3, current + 2, current + 3);
            AddQuad(triangles, next, next + 2, current, current + 2);
            AddQuad(triangles, current + 1, current + 3, next + 1, next + 3);
        }

        Mesh mesh = new Mesh { name = "Conveyor Belt Mesh" };
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void AddQuad(List<int> triangles, int v0, int v1, int v2, int v3)
    {
        triangles.Add(v0); triangles.Add(v1); triangles.Add(v2);
        triangles.Add(v1); triangles.Add(v3); triangles.Add(v2);
    }
}