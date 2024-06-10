using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform controlPoint; // A point to control the curve
    public LineRenderer lineRenderer;
    public int segmentCount = 50;
    public beamManager _manager;
   public bool ismove=false;

    void Start()
    {
       
    }
    void Update()
    {
        if(ismove)
        {
            DrawCurve(_manager.points[0].transform,controlPoint,_manager.points[1].transform);
            ismove=false;
        }
    }

    void DrawCurve(Transform pointA,Transform controlPoints,Transform pointB)
    {
        lineRenderer.positionCount = segmentCount + 1;
        for (int i = 0; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            Vector3 position = CalculateBezierPoint(t, pointA.position, controlPoint.position, pointB.position);
            lineRenderer.SetPosition(i, position);
        }
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; // (1-t)^2 * p0
        p += 2 * u * t * p1; // 2(1-t)t * p1
        p += tt * p2;        // t^2 * p2

        return p;
    }
}
