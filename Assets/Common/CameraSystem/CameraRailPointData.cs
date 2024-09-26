using System;
using UnityEngine;

[Serializable]
public class CameraRailPointData : MonoBehaviour
{
    public Vector2 position;
    public Vector2 offset;
    public Vector2 OffsetPosition => position + offset;
}
