using UnityEngine;

public class CameraAreaLimiter : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform[] polygonVerticesFrom;
    [SerializeField] Transform[] polygonVerticesTo;
    void Start()
    {

    }
    void Update()
    {
        Vector3 currentPos = cameraTransform.position;

    }
    bool IsPointInQuadrilateral(Vector2 topLeft, Vector2 bottomLeft, Vector2 topRight, Vector2 bottomRight, Vector2 point)
    {
        // Check if the point is inside either of the two triangles
        return IsPointInTriangle(topLeft, bottomLeft, topRight, point) || IsPointInTriangle(bottomLeft, bottomRight, topRight, point);
    }
    bool IsPointInTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        // Calculate vectors
        Vector2 v0 = C - A;
        Vector2 v1 = B - A;
        Vector2 v2 = P - A;

        // Compute dot products
        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        // Compute barycentric coordinates
        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if point is in triangle
        return (u >= 0) && (v >= 0) && (u + v < 1);
    }

}
