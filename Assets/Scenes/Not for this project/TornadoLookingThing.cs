using Assets.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoLookingThing : MonoBehaviour
{
    [SerializeField] int pointsAmount;
    [SerializeField] float spirals;
    [SerializeField] float height;
    [SerializeField] float maxRadius;
    [SerializeField] float yCompression;
    [SerializeField] float scrollSpeed;
    [SerializeField] float timer; 
    private void OnDrawGizmos()
    {
        timer += scrollSpeed;
        Vector2 center = transform.position;
        Vector2[] points = new Vector2[pointsAmount];
        for (int i = 0; i < pointsAmount; i++)
        {
            float progress = (float)i / pointsAmount;
            float angle = Mathf.Lerp(0, Mathf.PI * 2 * spirals, progress) + timer;
            progress *= progress;
            points[i] = center 
            + new Vector2(0, progress * height)//Y offset
            + (new Vector2(Mathf.Cos(angle) * progress, Mathf.Sin(angle) * yCompression * progress) * maxRadius)//spiral offset
            ;
        }
        for (int i = 0; i < pointsAmount - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
}
