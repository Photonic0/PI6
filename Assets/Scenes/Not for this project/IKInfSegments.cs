using Assets.Helpers;
using UnityEngine;

public class IKInfSegments : MonoBehaviour
{
    [SerializeField] Transform[] segments;
    [SerializeField] Transform origin;
    [SerializeField] Transform target;
    [SerializeField] Transform flipDecider;
    private float[] GetLengthData(out float[] sums, out float minSegmentLength)
    {
        minSegmentLength = 99999999f;
        float sum = 0;
        int max = segments.Length;
        float[] lengths = new float[max];
        sums = new float[max];
        for (int i = 0; i < max; i++)
        {
            float length = segments[i].lossyScale.x;
            lengths[i] = length;
            sums[i] = sum;
            sum += length;
            if (length < minSegmentLength)
            {
                minSegmentLength = length;
            }
        }
        return lengths;
    }

    void Update()
    {
        float[] lengths = GetLengthData(out float[] sums, out float minSegmentLength);
        Vector2 origin = this.origin.position;
        float maxRange = sums[^1] + lengths[^1];
        Vector2 currentTarget = ClampWithinCircle(target.position, origin, maxRange);
        Vector2 deltaPosToTarget = (currentTarget - origin);
        float distToTarget = deltaPosToTarget.magnitude;
        bool flip = Vector2.Dot(deltaPosToTarget.normalized, Vector2.right) < 0;
        float mult = Mathf.InverseLerp(0, maxRange, distToTarget);
        for (int i = lengths.Length - 1; i > 0; i--)
        {
            Transform segment = segments[i];
            Vector2 jointPoint = flip ? GetJointPointInverted(origin, currentTarget, Mathf.Max(minSegmentLength, sums[i] * mult), lengths[i]) : GetJointPoint(origin, currentTarget, Mathf.Max(minSegmentLength, sums[i] * mult), lengths[i]);
            segment.SetPositionAndRotation((jointPoint + currentTarget) * .5f, (jointPoint - currentTarget).ToRotation());
            currentTarget = jointPoint;
        }
        segments[0].SetPositionAndRotation((origin + currentTarget) * .5f, (origin - currentTarget).ToRotation());
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
        Vector2 notFlipped = from + offset;
        return notFlipped;

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
    Vector2 ClampWithinCircle(Vector2 vecToClamp, Vector2 circleCenter, float radius)
    {
        vecToClamp -= circleCenter;
        vecToClamp = Vector2.ClampMagnitude(vecToClamp, radius);
        return vecToClamp + circleCenter;
    }
}
