using Assets.Helpers;
using UnityEngine;

public class DiscoHazardSpotlight : MonoBehaviour
{
    [SerializeField] Transform armFar;
    [SerializeField] Transform armClose;
    [SerializeField] Transform lerpPointA;
    [SerializeField] Transform lerpPointB;
    [SerializeField] Transform spotlightBody;
    [SerializeField] Transform spotlightTargetPoint;
    [SerializeField] float minDist;
    [SerializeField] bool flip;
    [SerializeField] bool flipLight;
    [SerializeField] float angleRelativeToPivot = Mathf.PI / 2;
    [SerializeField] float armLength1;
    [SerializeField] float armLength2;
    [SerializeField] float lightDistanceFromPivotNeeded;
    void Update()
    {

        float t = Mathf.InverseLerp(-1, 1, Mathf.Sin((float)(Time.timeAsDouble % Helper.Tau)));
        Vector2 from = lerpPointA.position;
        Vector2 target = lerpPointB.position;
        Vector2 toPos = Vector2.Lerp(from, target, minDist + (1 - minDist) * t);
        /*float armLength1 = armClose.transform.lossyScale.y;
        float armLength2 = armFar.transform.lossyScale.y;*/
        //float lightDistanceFromPivotNeeded = spotlightBody.lossyScale.x * .5f;
       
        Vector2 jointPoint = flip ? GetJointPointInverted(from, toPos, armLength1, armLength2) : GetJointPoint(from, toPos, armLength1, armLength2);
        Vector2 arm2Tip = jointPoint - (jointPoint - toPos).normalized * armLength2;
        GetVarsToFaceTowards(arm2Tip, lightDistanceFromPivotNeeded, angleRelativeToPivot, spotlightTargetPoint.transform.position, out Quaternion spotlightRotation, out Vector2 spotlightPosition);
        spotlightBody.SetPositionAndRotation(spotlightPosition, spotlightRotation);
        armClose.SetPositionAndRotation((jointPoint + from) / 2, (jointPoint - from).ToRotation(90));
        armFar.SetPositionAndRotation(jointPoint + .5f * armLength2 * (toPos - jointPoint).normalized, (jointPoint - toPos).ToRotation(90));
    }
    void GetVarsToFaceTowards(Vector2 pivot, float distanceFromPivot, float angleRelativeToPivot, Vector2 target, out Quaternion rotation, out Vector2 position)
    {      
        float dist = Vector2.Distance(pivot, target);
        Vector2 toTarget = target - pivot;
        //cap to prevent NaN from blind spot
        if(dist <= distanceFromPivot)
        {
            dist = distanceFromPivot + 0.001f;//increase to avoid any imprecision issues
            toTarget.Normalize();
            toTarget *= dist;
            target = pivot + toTarget;
        }
        float angle = Mathf.PI - Mathf.Asin(distanceFromPivot * Mathf.Sin(angleRelativeToPivot) / dist) - angleRelativeToPivot;
        angle += Mathf.PI * .5f;//compensate for the light facing down
        float angleToTarget = Mathf.Atan2(toTarget.y, toTarget.x);
        angle -= angleToTarget;
        rotation = Quaternion.Euler(0, 0, angle * -Mathf.Rad2Deg - 90);
        Vector2 offset = new(Mathf.Sin(angle) * distanceFromPivot, Mathf.Cos(angle) * distanceFromPivot);
        if (flipLight)
        {
            Vector2 reflectNormal = new Vector2(-toTarget.y, toTarget.x).normalized;//rotated 90 degrees and normalized
            offset = Vector2.Reflect(offset, reflectNormal);
            rotation = Quaternion.Euler(0, 0, Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg);
        }
        position = pivot + offset;
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector2 from = lerpPointA.position;
        Gizmos.DrawWireSphere(from, armLength2 + armLength2);
        float blindspotRadius = Mathf.Abs(armLength2 - armLength1);
        
        Vector2 target = lerpPointB.position;
        Vector2 aimedPoint = spotlightTargetPoint.position;
        Gizmos.color = Gizmos.color = Color.red;
        if (blindspotRadius > 0.00001f)
        {
            Gizmos.DrawWireSphere(from, blindspotRadius);
        }
        Vector2 closestPoint = Vector2.Lerp(from, target, blindspotRadius);
        Gizmos.DrawLine(target, closestPoint);
        Gizmos.color = Gizmos.color = Color.green;
        Gizmos.DrawLine(closestPoint, aimedPoint);
        Gizmos.DrawLine(target, aimedPoint);
    }
#endif
}
