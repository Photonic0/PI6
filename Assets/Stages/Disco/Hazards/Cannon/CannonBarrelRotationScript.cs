using UnityEngine;
using Assets.Helpers;
using UnityEditor;
using Assets.Systems;

public class CannonBarrelRotationScript : MonoBehaviour
{
    public float recoilTimer;
    Vector2 center;
    [SerializeField] Transform parent;
    private void Awake()
    {
        parent = transform.parent;
        center = transform.position;
    }
    void Update()
    {
        recoilTimer += Time.deltaTime;
        Vector2 position = transform.position;
        Vector2 toTarget = (Vector2)GameManager.PlayerPosition - position;
        Vector2 offset = toTarget.normalized * Helper.Remap(recoilTimer, 0, .3f, -.5f, 0, Easings.SqrInOut);
        toTarget = LimitAngle(Mathf.PI, ((parent.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad).PolarVector(), toTarget);
        float rotationDegrees = toTarget.Atan2Deg();
        rotationDegrees -= 135;
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotationDegrees);
        float t = 1 - Mathf.Pow(0.00000001f, Time.deltaTime);
        transform.SetPositionAndRotation(center + offset, Quaternion.Lerp(transform.rotation, targetRotation, t));
    }

    static Vector2 LimitAngle(float angleRangeRadians, Vector2 angleCenter, Vector2 angleToLimit)
    {
        float dot = Vector2.Dot(angleCenter, angleToLimit);
        float dotNormal = Vector2.Dot(angleToLimit, new Vector2(-angleCenter.y, angleCenter.x));
        float dotThreshold = Mathf.Cos(angleRangeRadians * .5f);

        if(dot >= dotThreshold)
        {
            return angleToLimit;
        }
        if(dotNormal < 0)
        {
            return angleCenter.RotatedBy(-angleRangeRadians * .5f);
        }
        return angleCenter.RotatedBy(angleRangeRadians * .5f);
    }
    private static float LimitAngle(float rotationDegrees)
    {
        if (rotationDegrees < -90)
        {
            rotationDegrees = -180;
        }
        else if (rotationDegrees < 0)
        {
            rotationDegrees = 0;
        }
        return rotationDegrees;
    }
}
