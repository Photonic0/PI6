using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IKCircles : MonoBehaviour
{
    /*
     * # Init variables
chain = [50, 70, 60, 30, 20, 50, 60]
vectors = []
end_effector = Vector2D(0, 0)
pole = Vector2D(0, 0)
maximal_distance = sum(chain)
# Fill array of vectors
for i in range(0, chain.__len__()):
    vectors.append(Vector2D(chain[i], 0))
     */
    [SerializeField] float[] chain = new float[3] { 0.5f, .75f, 1f };
    [SerializeField] Vector2[] vectors;
    [SerializeField] Transform endEffectorTransform;
    Vector2 EndEffector => endEffectorTransform.position;
    [SerializeField] float maximalDistance;
    [SerializeField] Transform circle1;
    [SerializeField] Transform circle2;
    [SerializeField] float radius1;
    [SerializeField] float radius2;
    void Start()
    {
        maximalDistance = chain.Sum();
    }

    void Update()
    {

    }

    static void GetIntersections(Vector2 position1, float radius1, Vector2 position2, float radius2, out Vector2 intersection1, out Vector2 intersection2)
    {
        float distance = Vector2.Distance(position1, position2);
        if (distance > radius1 + radius2 || distance < Mathf.Abs(radius1 - radius2))
        {
            intersection1 = Vector2.zero;
            intersection2 = Vector2.zero;
            return;
        }
        float a = (radius1 * radius1 - radius2 * radius2 + distance * distance) / (2 * distance);
        float h = Mathf.Sqrt(radius1 * radius1 - a * a);
        Vector2 pointOnLine = position1 + a * (position2 - position1) / distance;
        intersection1 = new Vector2(pointOnLine.x + h * (position2.y - position1.y) / distance, pointOnLine.y - h * (position2.x - position1.x) / distance);
        intersection2 = new Vector2(pointOnLine.x - h * (position2.y - position1.y) / distance, pointOnLine.y + h * (position2.x - position1.x) / distance);
    }
    static bool CheckTriangleValidity(float a, float b, float c)
    {
        return a + b >= c || a + c >= b || b + c >= a || IsClose(a + b, c) || IsClose(a + c, b) || IsClose(b + c, a);
    }
    public float FindSide(float minimalLength, float maximalLength, float side1, float side2)
    {
        for (float side = maximalLength; side >= minimalLength; side -= 0.05f)
        {
            if (CheckTriangleValidity(side, side1, side2))
            {
                return side;
            }
        }
        return 0;
    }
    /*
     * def resolve_ik(chain, vectors, end_effector, maximal_distance, pole):
    # Init variables
    new_vectors = []
    # Calculate current side
    if end_effector.length() > maximal_distance:
        end_effector = end_effector.normalized() * maximal_distance
    current_side_vector = end_effector

    for i in range(chain.__len__()-1, 0, -1):
        current_side = current_side_vector.length()
        # Find possible side
        new_side = find_side(0, sum(chain[:i]), chain[i], current_side)
        # Find intersection nearest to the pole
        intersections = []
        if i != 1:
            intersections = get_intersections(current_side_vector, chain[i], Vector2D(0, 0), new_side)
        else:
            intersections = get_intersections(current_side_vector, chain[i], Vector2D(0, 0), chain[0])

        intersection = Vector2D(0, 0)
        if intersections[0]-pole < intersections[1]-pole:
            intersection = intersections[0]
        else:
            intersection = intersections[1]
        # Change vector
        new_vectors.insert(0, current_side_vector - intersection)

        # print(new_vectors.__len__())
        current_side_vector = intersection
        print(new_side)

    new_vectors.insert(0, current_side_vector)

    return new_vectors
     */
    //List<Vector2> ResolveIK(float)
    //{

    //}


    static bool IsClose(float a, float b)
    {
        return Mathf.Abs(a - b) < 0.0001f;
    }

    void DrawCircleGizmo(Vector3 origin, float radius, int segments = 36)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = origin + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = origin + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
    private void OnDrawGizmos()
    {
        if (circle1 == null || circle2 == null) return;

        DrawCircleGizmo(circle1.position, radius1);
        DrawCircleGizmo(circle2.position, radius2);
        GetIntersections(circle1.position, radius1, circle2.position, radius2, out Vector2 intersection1, out Vector2 intersection2);
        Gizmos.DrawSphere(intersection1, 0.1f);
        Gizmos.DrawSphere(intersection2, 0.1f);
    }
}
