using Assets.Common.Consts;
using Assets.Helpers;
using System.Collections.Generic;
using UnityEngine;

public class DiscoBossBallProj : MonoBehaviour
{
    VerletSimulator rope;
    [SerializeField] LineRenderer line;
    public Rigidbody2D rb;
    public void Start()
    {
        transform.rotation = rb.velocity.ToRotation(90);
        const float RopeLength = 2;
        Vector2 ballTopPos = GetBallTop();
        rope = new(1, 15);
        int dotCount = 10;
        List<Dot> dots = new(dotCount);
        rope.dots = dots;
        Dot firstDot = new(ballTopPos, true);
        dots.Add(firstDot);
        firstDot.isLocked = true;
        for (int i = 1; i < dotCount; i++)
        {
            Dot dot = new(ballTopPos + (transform.rotation.eulerAngles.z).PolarVector((i - 1) * (RopeLength / dotCount)), false);
            Dot.Connect(dot, dots[i - 1]);
            dots.Add(dot);
        }
        line.positionCount = dotCount;
    }
    public void FixedUpdate()
    {
        Vector2 ballCenter = transform.position;
        transform.rotation = rb.velocity.ToRotation(90);
        rope.dots[0].position = GetBallTop() + rb.velocity * Time.fixedDeltaTime;
        for (int i = 1; i < rope.dots.Count; i++)
        {
            Dot dot = rope.dots[i];
            if (dot.isLocked)
                continue;
            Vector2 deltaPos = dot.position - ballCenter;
            if(deltaPos.magnitude < .4f)
            {
                dot.position = ballCenter + deltaPos.normalized * .4f;
            }
        }
        rope.AddForce(Physics2D.gravity);
        rope.Simulate(Time.fixedDeltaTime);
        for (int i = 0; i < rope.dots.Count; i++)
        {
            line.SetPosition(i, rope.dots[i].position);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag(Tags.Tiles))
        {
            gameObject.SetActive(false);
            EffectsHandler.SpawnMediumExplosion(FlipnoteColors.ColorID.Magenta, transform.position);
        }

    }
    Vector2 GetBallTop()
    {
        Vector2 center = transform.position;
        center += (transform.rotation.eulerAngles.z * -Mathf.Deg2Rad).PolarVector(0.4f);//a bit inside the ball is better, makes it more cohesive visually
        return center;
    }
    static void TimeToReachYPoint(float fromY, float toY, float yAccel, float initialYVel, out float onWayDown, out float onWayUp)
    {
        float a = 0.5f * yAccel;
        float b = initialYVel;
        float c = fromY - toY;
        SolveQuadratic(a, b, c, out onWayDown, out onWayUp);
    }
    public static void GetLaunchVelocity(Vector2 from, Vector2 to, float timeTaken, float yAccel, float peakHeight, out Vector2 onWayUp, out Vector2 onWayDown)
    {
        float deltaX = to.x - from.x;
        float initialYVel = YVelocityNeededToReachTarget(timeTaken, peakHeight, from.y, to.y);
        TimeToReachYPoint(from.y, to.y, yAccel, initialYVel, out float onWayUpY, out float onWayDownY);
        onWayUp = new Vector2(deltaX / onWayUpY, initialYVel);
        onWayDown = new Vector2(deltaX / onWayDownY, initialYVel);
    }
    public static void GetLaunchVelocity(Vector2 from, Vector2 to, float yAccel, float initialYVel, out Vector2 onWayUp, out Vector2 onWayDown)
    {
        float deltaX = to.x - from.x;
        TimeToReachYPoint(from.x, to.y, yAccel, initialYVel, out float onWayDownY, out float onWayUpY);
        onWayUp = new Vector2(deltaX / onWayUpY, initialYVel);
        onWayDown = new Vector2(deltaX / onWayDownY, initialYVel);
    }
    static float YVelocityNeededToReachTarget(float timeTaken, float peakHeight, float fromY, float toY)
    {
        fromY = toY - fromY;
        float timeToPeak = timeTaken / 2;
        float g = (2 * (peakHeight - fromY)) / (timeToPeak * timeToPeak);
        float initialYVel = g * timeToPeak;
        return initialYVel;
    }
    static void SolveQuadratic(float a, float b, float c, out float minX, out float maxX)
    {
        float delta = b * b - 4 * a * c;
        if (delta < 0)
        {
            minX = float.MinValue;
            maxX = float.MaxValue;
            return;
        }
        float sqrtDelta = Mathf.Sqrt(delta);
        float x1 = (-b - sqrtDelta) / (2 * a);
        float x2 = (-b + sqrtDelta) / (2 * a);
        minX = Mathf.Min(x1, x2);
        maxX = Mathf.Max(x1, x2);
    }
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(GetBallTop(), .1f);
    }
#endif

    static float YVelocityNeededToReachYPointIn(float timeTaken, float fromY, float toY, float yAccel) => (toY - fromY - 0.5f * yAccel * timeTaken * timeTaken) / timeTaken;

}
