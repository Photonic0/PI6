using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Helpers;

public class CannonBarrelRotationScript : MonoBehaviour
{
    void Update()
    {
        float rotationDegrees = ((Vector2)(GameManager.PlayerPosition - transform.position)).Atan2Deg(-135);
        rotationDegrees = Mathf.Clamp(rotationDegrees, -135, 45);
        transform.rotation = Quaternion.Euler(0, 0, rotationDegrees);

    }
}
