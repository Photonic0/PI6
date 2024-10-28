using UnityEngine;
using Assets.Helpers;
using UnityEditor;

public class CannonBarrelRotationScript : MonoBehaviour
{
    void Update()
    {
        float rotationDegrees = ((Vector2)(GameManager.PlayerPosition - transform.position)).Atan2Deg();
        if(rotationDegrees < -90)
        {
            rotationDegrees = -180;
        }
        else if(rotationDegrees < 0)
        {
            rotationDegrees = 0;
        }
        rotationDegrees -= 135;
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotationDegrees);
        float t = 1 - Mathf.Pow(0.00000001f, Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
    }
}
