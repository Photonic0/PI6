using Assets.Helpers;
using UnityEngine;

public class RandomSplineTest : MonoBehaviour
{
    [SerializeField] Transform[] dots;
    [SerializeField] float[] timers;
    [SerializeField] float radius;
    [SerializeField] float moveSpeed;

    //0 is start, 1 is end, 2 is influence1, 3 is influence2
    Vector2[][] bezierPoints;

    Vector2 endPoint;
    Vector2 startPoint;
    Vector2 influence1;
    Vector2 influence2;
    private void Start()
    {
        bezierPoints = new Vector2[dots.Length][];
        timers = new float[dots.Length];
        for (int i = 0; i < bezierPoints.Length; i++)
        {
            timers[i] = (float)i / bezierPoints.Length;
            Color trailColor = Color.HSVToRGB((float)i / bezierPoints.Length, 1, 1);
            dots[i].GetComponent<TrailRenderer>().endColor = trailColor;
            dots[i].GetComponent<TrailRenderer>().startColor = trailColor;
            bezierPoints[i] = new Vector2[4]
            {
                transform.position,
                Random2.Circular(radius),
                Random2.Circular(radius),
                Random2.Circular(radius)
            };
        }
    }
    void Update()
    {


        for (int i = 0; i < bezierPoints.Length; i++)
        {
            timers[i] += Time.deltaTime * moveSpeed;

            if (timers[i] > 1)
            {
                timers[i] %= 1;
                bezierPoints[i][0] = bezierPoints[i][1];
                bezierPoints[i][1] = Random2.Circular(radius);
                bezierPoints[i][2] = bezierPoints[i][0] + (bezierPoints[i][0] - bezierPoints[i][3]);
                bezierPoints[i][3] = Random2.Circular(radius);
            }
            dots[i].position = CubicBezier(bezierPoints[i][3], bezierPoints[i][0], bezierPoints[i][1], bezierPoints[i][2], timers[i]);
        }

    }
    static Vector2 CubicBezier(Vector2 influence2, Vector2 start, Vector2 end, Vector2 influence1, float t)
    {
        float tSqr = t * t;
        float tCube = tSqr * t;

        return start * (-tCube + 3 * tSqr - 3 * t + 1) +
            influence1 * (3 * tCube - 6 * tSqr + 3 * t) +
            influence2 * (-3 * tCube + 3 * tSqr) +
            end * tCube;
    }
    private void OnDrawGizmos()
    {
        if (bezierPoints == null)
            return;
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(bezierPoints[0], .1f);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawSphere(bezierPoints[1], .1f);
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(bezierPoints[2], .1f);
        //Gizmos.color = Color.green;
        //Gizmos.DrawSphere(bezierPoints[3], .1f);
        //Handles.DrawLine(bezierPoints[3], bezierPoints[1] + (bezierPoints[1] - bezierPoints[3]));
        //Handles.DrawLine(bezierPoints[2], bezierPoints[0] + (bezierPoints[0] - bezierPoints[2]));
    }
}
