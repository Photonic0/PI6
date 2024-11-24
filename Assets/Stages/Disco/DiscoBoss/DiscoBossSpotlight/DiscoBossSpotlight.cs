using Assets.Helpers;
using UnityEngine;
public class DiscoBossSpotlight : MonoBehaviour
{
#if UNITY_EDITOR
    private enum TimerSampleMode
    {
        Default,
        PingPong,
        Repeat,
        Clamp
    }
    [SerializeField] TimerSampleMode timerSampleMode;
#endif

    [SerializeField] Transform armFar;
    [SerializeField] Transform armClose;
    [SerializeField] Transform spotlightBody;
    [SerializeField] DiscoBossSpotlightSplinePathing spotlightFaceTargetPath;
    [SerializeField] bool flip;
    [SerializeField] bool flipLight;
    [SerializeField] float angleRelativeToPivot = Mathf.PI / 2;
    [SerializeField] float armLength1;
    [SerializeField] float armLength2;
    [SerializeField] float lightDistanceFromPivotNeeded;
    [SerializeField] float timer;
    [SerializeField] float timerIncreaseMultiplier;
    [SerializeField] DiscoBossSpotlightSplinePathing positionPathing;
    [SerializeField] float[] lightChangeTs;
    [SerializeField] int[] lightChangeTsIndicesToChangeCollision;
    [SerializeField] GameObject spotlightLight;
    [SerializeField] PolygonCollider2D spotlightLightHitbox;
    float Timer
    {
        get
        {
#if UNITY_EDITOR
            return timerSampleMode switch
            {
                TimerSampleMode.Default => timer,
                TimerSampleMode.PingPong => Mathf.PingPong(timer, positionPathing.SplineCount),
                TimerSampleMode.Repeat => Mathf.Repeat(timer, positionPathing.SplineCount),
                TimerSampleMode.Clamp => Mathf.Clamp(timer, 0, positionPathing.SplineCount),
                _ => timer,
            };
#endif
            return timer;
        }
    }
    void Update()
    {

        this.timer += Time.deltaTime * timerIncreaseMultiplier;
        float timer = Timer;
        if (timer > positionPathing.SplineCount)
        {
            gameObject.SetActive(false);
            return;
        }
        Vector2 from = transform.position;
        Vector2 toPos = positionPathing.SampleClamped(timer);
        float armLength1 = this.armLength1 * armClose.lossyScale.y;
        float armLength2 = this.armLength2 * armFar.lossyScale.y;
        //float lightDistanceFromPivotNeeded = spotlightBody.lossyScale.x * .5f;

        Vector2 jointPoint = flip ? GetJointPointInverted(from, toPos, armLength1, armLength2) : GetJointPoint(from, toPos, armLength1, armLength2);
        Vector2 arm2Tip = jointPoint - (jointPoint - toPos).normalized * armLength2;

        GetVarsToFaceTowards(arm2Tip, lightDistanceFromPivotNeeded, angleRelativeToPivot,
            spotlightFaceTargetPath.SampleClamped(GetRemappedT(timer, positionPathing, spotlightFaceTargetPath)),
            out Quaternion spotlightRotation, out Vector2 spotlightPosition);

        spotlightBody.SetPositionAndRotation(spotlightPosition, spotlightRotation);
        armClose.SetPositionAndRotation((jointPoint + from) / 2, (jointPoint - from).ToRotation(90));
        armFar.SetPositionAndRotation(jointPoint + .5f * armLength2 * (toPos - jointPoint).normalized, (jointPoint - toPos).ToRotation(90));
        UpdateLightOnOffState();
    }
    void UpdateLightOnOffState()
    {
        if(lightChangeTs == null)
        {
            return;
        }
        float timer = Timer;
        if (timer < lightChangeTs[0])
        {
            spotlightLight.SetActive(false);
        }
        else if (timer > lightChangeTs[^1])
        {
            spotlightLight.SetActive(lightChangeTs.Length % 2 == 1);
        }
        else
        {
            for (int i = 1; i < lightChangeTs.Length; i++)
            {
                if (timer > lightChangeTs[i - 1] && timer < lightChangeTs[i])
                {
                    spotlightLight.SetActive(i % 2 == 1);
                }
            }
        }
        if (lightChangeTsIndicesToChangeCollision == null)
        {
            return;
        }
        if (timer < lightChangeTs[lightChangeTsIndicesToChangeCollision[0]])
        {
            spotlightLightHitbox.enabled = false;
            return;
        }
        if (timer > lightChangeTs[lightChangeTsIndicesToChangeCollision[^1]])
        {
            spotlightLightHitbox.enabled = lightChangeTs.Length % 2 == 1;
            return;
        }
        for (int i = 1; i < lightChangeTsIndicesToChangeCollision.Length; i++)
        {
            if (timer > lightChangeTs[lightChangeTsIndicesToChangeCollision[ i - 1]] && timer < lightChangeTs[lightChangeTsIndicesToChangeCollision[i]])
            {
                spotlightLightHitbox.enabled = (i % 2 == 1);
            }
        }
    }
    void GetVarsToFaceTowards(Vector2 pivot, float distanceFromPivot, float angleRelativeToPivot, Vector2 target, out Quaternion rotation, out Vector2 position)
    {
        float dist = Vector2.Distance(pivot, target);
        Vector2 toTarget = target - pivot;
        //cap to prevent NaN from blind spot
        if (dist <= distanceFromPivot)
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
    float GetRemappedT(float timer, DiscoBossSpotlightSplinePathing fromPath, DiscoBossSpotlightSplinePathing toPath)
    {
        return timer / fromPath.SplineCount * toPath.SplineCount;
    }
    public void StartAnimation(float duration)
    {
        gameObject.SetActive(true);
        positionPathing.Start();
        float timeToReachEnd = positionPathing.SplineCount;
        //if duration is 2s, and time to reach end is 1s, timeIncreaseMultiplier will be 0.5
        timerIncreaseMultiplier = timeToReachEnd / duration;
        timer = 0;
    }
#if UNITY_EDITOR
    [Header("Debug params")]
    [SerializeField] bool debug_SetTimer;
    [SerializeField] float debug_timerValueToSet;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float timer = Timer;
        
        Gizmos.DrawLine(positionPathing.SampleClamped(timer), spotlightFaceTargetPath.SampleClamped(GetRemappedT(timer, positionPathing, spotlightFaceTargetPath)));
        if (debug_SetTimer)
        {
            debug_SetTimer = false;
            this.timer = debug_timerValueToSet;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(spotlightFaceTargetPath.SampleClamped(GetRemappedT(timer, positionPathing, spotlightFaceTargetPath)), .1f);
    }
#endif

}
