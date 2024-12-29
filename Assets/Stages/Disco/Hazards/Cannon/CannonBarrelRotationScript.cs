using UnityEngine;
using Assets.Helpers;
using UnityEditor;
using Assets.Systems;

public class CannonBarrelRotationScript : MonoBehaviour
{
    public float recoilTimer;
    Vector2 center;
    private void Awake()
    {
        center = transform.position;
    }
    void Update()
    {
        recoilTimer += Time.deltaTime;
        Vector2 position = transform.position;
        Vector2 toTarget = (Vector2)GameManager.PlayerPosition - position;
        Vector2 offset = toTarget.normalized * Helper.Remap(recoilTimer, 0, .3f, -.5f, 0, Easings.SqrInOut);
        float rotationDegrees = toTarget.Atan2Deg();
        rotationDegrees = LimitAngle(rotationDegrees);
        rotationDegrees -= 135;
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotationDegrees);
        float t = 1 - Mathf.Pow(0.00000001f, Time.deltaTime);
        transform.SetPositionAndRotation(center + offset, Quaternion.Lerp(transform.rotation, targetRotation, t));
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
