using UnityEngine;

public class Mover : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float speed = 55.0f;
    private float distanceTraveled;
    public beamManager _manager;

    void Update()
    {
        if(_manager.isMove)
        {
        distanceTraveled += speed * Time.deltaTime;
        float pathLength = GetPathLength();
        float t = distanceTraveled / pathLength;
        if (t > 1.0f) t = 1.0f; // Stop at the end of the path
        Vector3 position = GetPointAt(t);
      //  transform.position =Vector3.MoveTowards(transform.position, position,speed) ;
      transform.LookAt(position);
      transform.position=position;
        }
    }

    float GetPathLength()
    {
        float length = 0.0f;
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            length += Vector3.Distance(lineRenderer.GetPosition(i), lineRenderer.GetPosition(i + 1));
        }
        return length;
    }

    Vector3 GetPointAt(float t)
    {
        int pointCount = lineRenderer.positionCount;
        if (t <= 0) return lineRenderer.GetPosition(0);
        if (t >= 1) return lineRenderer.GetPosition(pointCount - 1);

        float totalDistance = t * GetPathLength();
        float distanceCovered = 0.0f;

        for (int i = 0; i < pointCount - 1; i++)
        {
            Vector3 start = lineRenderer.GetPosition(i);
            Vector3 end = lineRenderer.GetPosition(i + 1);
            float segmentDistance = Vector3.Distance(start, end);
            if (distanceCovered + segmentDistance >= totalDistance)
            {
                float segmentT = (totalDistance - distanceCovered) / segmentDistance;
                return Vector3.Lerp(start, end, segmentT);
            }
            distanceCovered += segmentDistance;
        }

        return lineRenderer.GetPosition(pointCount - 1); // Fallback
    }
}
