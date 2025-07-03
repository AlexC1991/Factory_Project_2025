using UnityEngine;
using System.Collections;

public class WaypointManager : MonoBehaviour
{
    [Header("Roller References")]
    public Transform rollerA;
    public Transform rollerB;

    [Header("Bezier Control Anchors")]
    public Transform _splineAnchorB; // Control point 1
    public Transform _splineAnchorC; // Control point 2
    public Transform _splineAnchorD; // Control point 3
    public Transform _splineAnchorE; // Control point 4

    [Header("Belt Settings")]
    public float conveyorLength = 4f;
    public float conveyorWidth = 2f;
    public float beltHeight = 1f;
    [Range(0.1f, 0.8f)]
    public float curveIntensity = 0.3f;
    [Range(0.1f, 2f)]
    public float curveHeight = 0.5f;

    [Header("Pole Settings")]
    public float poleRadius = 0.1f;
    public float poleHeight = 0.3f;
    public Material cPoleMaterial;
    public Material bPoleMaterial;

    private BeltPathTracer _beltTracer;
    private Vector3 lastRollerAPos, lastRollerBPos;

    private void Start()
    {
        _beltTracer = GetComponent<BeltPathTracer>();

        if (rollerA != null) lastRollerAPos = rollerA.position;
        if (rollerB != null) lastRollerBPos = rollerB.position;

        UpdateControlPoints();
        StartCoroutine(MonitorRollerMovement());
    }

    private IEnumerator MonitorRollerMovement()
    {
        while (true)
        {
            bool moved = false;

            if (rollerA != null && Vector3.Distance(rollerA.position, lastRollerAPos) > 0.01f)
            {
                lastRollerAPos = rollerA.position;
                moved = true;
            }

            if (rollerB != null && Vector3.Distance(rollerB.position, lastRollerBPos) > 0.01f)
            {
                lastRollerBPos = rollerB.position;
                moved = true;
            }

            if (moved)
            {
                UpdateControlPoints();
                _beltTracer?.OnRollersChanged();
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    public void UpdateControlPoints()
    {
        if (rollerA == null || rollerB == null) return;

        Vector3 start = rollerA.position;
        Vector3 end = rollerB.position;
        Vector3 direction = (end - start);
        float distance = direction.magnitude;
        Vector3 directionNorm = direction.normalized;

        Vector3 midPoint = (start + end) * 0.5f;
        Vector3 perpendicular = Vector3.Cross(directionNorm, Vector3.up).normalized;
        Vector3 upVector = Vector3.up;

        // Calculate control point positions for smooth natural curve
        float controlOffset = distance * curveIntensity;
        float heightOffset = curveHeight;
        float widthOffset = conveyorWidth * 0.3f;

        // Control points create a smooth S-curve or arc
        if (_splineAnchorB != null)
            _splineAnchorB.position = start + directionNorm * (controlOffset * 0.5f) + perpendicular * widthOffset;

        if (_splineAnchorC != null)
            _splineAnchorC.position = midPoint + upVector * heightOffset + perpendicular * (widthOffset * 0.5f);

        if (_splineAnchorD != null)
            _splineAnchorD.position = midPoint + upVector * heightOffset - perpendicular * (widthOffset * 0.5f);

        if (_splineAnchorE != null)
            _splineAnchorE.position = end - directionNorm * (controlOffset * 0.5f) - perpendicular * widthOffset;
    }

    public Vector3[] GetControlPoints()
    {
        return new Vector3[]
        {
            _splineAnchorB != null ? _splineAnchorB.position : Vector3.zero,
            _splineAnchorC != null ? _splineAnchorC.position : Vector3.zero,
            _splineAnchorD != null ? _splineAnchorD.position : Vector3.zero,
            _splineAnchorE != null ? _splineAnchorE.position : Vector3.zero
        };
    }

    public float GetBendAngle()
    {
        if (rollerA == null || rollerB == null) return 0f;
        Vector3 toB = (rollerB.position - rollerA.position).normalized;
        return Vector3.Angle(Vector3.forward, toB);
    }

    public float GetDistance()
    {
        return rollerA != null && rollerB != null ? 
               Vector3.Distance(rollerA.position, rollerB.position) : 0f;
    }
}