using Assets.Helpers;
using UnityEngine;

public class IK3Segments : MonoBehaviour
{
    [SerializeField] Transform armClose;
    [SerializeField] Transform armFar;
    [SerializeField] Transform arm3;
    [SerializeField] Transform joint1;
    [SerializeField] Transform joint2;
    [SerializeField] Transform origin;
    [SerializeField] Transform target;
    [SerializeField] Transform previewThing;
    [SerializeField] Transform circleIntersectionChooser;
    [SerializeField] float spinSpeed = .2f;
    [Range(0.0f, 1.0f)]
    [SerializeField] float first2SegmentLengthMult;
    [SerializeField] bool flip;
    void Update()
    {
        //try spinning segment as the 3rd segment then attach a 2 segment IK to it, tip of spinning segment as the target of 2 segment IK
        float armLength1 = armClose.lossyScale.y;
        float armLength2 = armFar.lossyScale.y;
        float armLengthSum = (armLength1 + armLength2);
        float length3 = arm3.lossyScale.y;
        Vector2 origin = this.origin.position;
        Vector2 target = this.target.position;
        Vector2 toPos = target;
        float distToTarget = (origin - target).magnitude;
        first2SegmentLengthMult = Mathf.InverseLerp(-armLengthSum - length3, armLengthSum + length3, distToTarget);
        armLengthSum *= first2SegmentLengthMult;
        Vector2 jointPoint = flip ? GetJointPointInverted(origin, toPos, armLengthSum, length3) : GetJointPoint(origin, toPos, armLengthSum, length3);
        arm3.SetPositionAndRotation(jointPoint + .5f * length3 * (toPos - jointPoint).normalized, (jointPoint - toPos).ToRotation(90));
        toPos = jointPoint;
        jointPoint = flip ? GetJointPointInverted(origin, toPos, armLength1, armLength2) : GetJointPoint(origin, toPos, armLength1, armLength2);
        armFar.SetPositionAndRotation(jointPoint + .5f * armLength2 * (toPos - jointPoint).normalized, (jointPoint - toPos).ToRotation(90));
        armClose.SetPositionAndRotation((jointPoint + origin) / 2, (jointPoint - origin).ToRotation(90));
        joint1.position = jointPoint;
        joint2.position = toPos;
    }
    Vector2 GetJointPointInverted(Vector2 from, Vector2 target, float armLength1, float armLength2)
    {
        float dist = Vector2.Distance(from, target);
        Vector2 toTarget = target - from;
        //if out of reach from being too far away
        if (dist >= armLength1 + armLength2)
        {
            dist = armLength1 + armLength2 - 0.00001f;//subtract to prevent imprecision issues
            toTarget.Normalize();
            toTarget *= dist;
        }
        //if out of reach from being inside blind spot
        else if (dist <= Mathf.Abs(armLength1 - armLength2))
        {
            dist = Mathf.Abs(armLength1 - armLength2) + 0.00001f;//add to prevent imprecision issues
            toTarget.Normalize();
            toTarget *= dist;
        }
        float angleToTarget = Mathf.Atan2(toTarget.y, toTarget.x);
        float armAngle = Mathf.Asin(
            (armLength1 * armLength1 + dist * dist - armLength2 * armLength2)
            /
            (2 * armLength1 * dist)
            );
        float finalAngle = armAngle - angleToTarget;
        Vector2 offset = new(Mathf.Sin(finalAngle) * armLength1, Mathf.Cos(finalAngle) * armLength1);
        Vector2 reflectNormal = new Vector2(-toTarget.y, toTarget.x).normalized;
        offset = Vector2.Reflect(offset, reflectNormal);
        return from + offset;
    }
    Vector2 GetJointPoint(Vector2 from, Vector2 target, float armLength1, float armLength2)
    {
        float dist = Vector2.Distance(from, target);
        Vector2 toTarget = target - from;

        //if out of reach from being too far away
        if (dist >= armLength1 + armLength2)
        {
            dist = armLength1 + armLength2 - 0.00001f;//subtract to prevent imprecision issues
            toTarget.Normalize();
            toTarget *= dist;
        }
        //if out of reach from being inside blind spot
        else if (dist <= Mathf.Abs(armLength1 - armLength2))
        {
            dist = Mathf.Abs(armLength1 - armLength2) + 0.00001f;//add to prevent imprecision issues
            toTarget.Normalize();
            toTarget *= dist;
        }
        float angleToTarget = Mathf.Atan2(toTarget.y, toTarget.x);
        float armAngle = Mathf.Asin(
            (armLength1 * armLength1 + dist * dist - armLength2 * armLength2)
            /
            (2 * armLength1 * dist)
            );
        float finalAngle = armAngle - angleToTarget;
        Vector2 offset = new(Mathf.Sin(finalAngle) * armLength1, Mathf.Cos(finalAngle) * armLength1);
        return from + offset;
    }
    static void GetTriangleAngles(float a, float b, float c, out float angleA, out float angleB, out float angleC)
    {
        angleA = Mathf.Acos((b * b + c * c - a * a) / (2 * b * c));
        angleB = Mathf.Acos((a * a + c * c - b * b) / (2 * a * c));
        angleC = Mathf.Acos((a * a + b * b - c * c) / (2 * a * b));
    }
    static void GetCircleIntersections(Vector2 origin1, float radius1, Vector2 origin2, float radius2, out Vector2 intersection1, out Vector2 intersection2)
    {
        float distance = Vector2.Distance(origin1, origin2);
        if (distance > radius1 + radius2 || distance < Mathf.Abs(radius1 - radius2))
        {
            intersection1 = Vector2.zero;
            intersection2 = Vector2.zero;
            return;
        }
        float a = (radius1 * radius1 - radius2 * radius2 + distance * distance) / (2 * distance);
        float h = Mathf.Sqrt(radius1 * radius1 - a * a);
        Vector2 pointOnLine = origin1 + a * (origin2 - origin1) / distance;
        intersection1 = new Vector2(pointOnLine.x + h * (origin2.y - origin1.y) / distance, pointOnLine.y - h * (origin2.x - origin1.x) / distance);
        intersection2 = new Vector2(pointOnLine.x - h * (origin2.y - origin1.y) / distance, pointOnLine.y + h * (origin2.x - origin1.x) / distance);
    }
    static void GetJointCircleIntersect(Vector2 o1, Vector2 o2, float r1, float r2, out Vector2 o3, out Vector2 o4, out Vector2 o5)
    {
        float d = (o1 - o2).magnitude;
        float aSqr = (
            r1 * r1 - r2 * r2 + d * d
            ) / (2 * d);
        float h = Mathf.Sqrt(r1 * r1 - aSqr);
        float hx = (o2.y - o1.y) / d * -h;
        float hy = (o2.x - o1.x) / d * h;
        Vector2 hVec = new(hx, hy);
        float a = Mathf.Sqrt(aSqr);
        o3 = o1 + a * ((o1 * o2) / d);
        o4 = o3 + hVec;
        o5 = o4 - hVec;
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
        if (!Application.isPlaying)
        {
            Update();
        }
    }

    /*previous attempt
     *         GetTriangleAngles(distToTarget, armLength1n2Sum * first2SegmentLengthMult, length3, out float angleA, out _, out _);
     *         Vector2 spinnyOffset = (origin - target).RotatedBy(angleA).normalized * length3;
        Vector2 toPos = target + spinnyOffset;
        if ((toPos - origin).magnitude > armLength1n2Sum)
        {
            toPos = origin + (toPos - origin).normalized * armLength1n2Sum;
            spinnyOffset = target - toPos;
        }

        arm3.SetPositionAndRotation(target + spinnyOffset * .5f, Quaternion.Euler(0, 0, spinnyOffset.Atan2Deg(90)));

     */
}

