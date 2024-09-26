using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraNewModifier : MonoBehaviour
{
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    public float testField;
    private void OnDrawGizmos()
    {
        DrawWithYOffset(0, "minX", minX);
        DrawWithYOffset(.5f, "maxX", maxX);
        DrawWithYOffset(1, "minY", minY);
        DrawWithYOffset(1.5f, "maxY", maxY);
    }
    void DrawWithYOffset(float yOffset, string name, float valueToDraw)
    {
        if (!float.IsNaN(valueToDraw))
        {
            //Handles.Label(transform.position + new Vector3(0, yOffset), name + ": " + valueToDraw.ToString());
        }
    }
}
