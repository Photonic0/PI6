using Assets.Helpers;
using Assets.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBossSwingingSpikeBall : Projectile
{
    [SerializeField] new Transform transform;
    [SerializeField] Transform anchor;
    [SerializeField] int verletIterations;
    [SerializeField] GameObject frontFacingChainSegmentPrefab;
    [SerializeField] GameObject sideFacingChainSegmentPrefab;
    [SerializeField] Transform[] chainSegments;
    [SerializeField] Transform arcStart;
    [SerializeField] Transform arcEnd;
    //anchor position will be the arc's center

    float timer;
    float timerSpeed;
    const float ChainSegmentSize = 0.45f;
    const float BallSize = 1.041f;
    VerletSimulator verletSimulator;
    public override int Damage => 5;
    float Progress => Easings.SqrInOut(Mathf.Clamp(timer, 0, 2));


    void Start()
    {
        transform.Rotate(0, 0, Random2.Float(Mathf.PI));
        InitializeVerlet();
    }
    private void InitializeVerlet()
    {
        verletSimulator = new VerletSimulator(1, verletIterations);
        float ballDist = Mathf.Max((anchor.position - arcStart.position).magnitude, (anchor.position - arcEnd.position).magnitude);
        int dotCount = (int)((ballDist - BallSize / 2f + ChainSegmentSize) / ChainSegmentSize);
        Array.Resize(ref chainSegments, dotCount - 1);
        List<Dot> dots = new(dotCount);
        verletSimulator.dots = dots;
        Dot firstDot = new(anchor.position, true);
        dots.Add(firstDot);
        firstDot.isLocked = true;
        Vector2 anchorPos = anchor.position;
        for (int i = 1; i < dotCount; i++)
        {
            Dot dot = new(anchorPos + new Vector2(.71f, .70f) * (i - 1) / (ballDist * dotCount), false);
            if (i == dotCount - 1)
            {
                dot.isLocked = true;
            }
            Dot.Connect(dot, dots[i - 1], ChainSegmentSize);
            dots.Add(dot);
        }
        for (int i = 0; i < chainSegments.Length; i++)
        {
            Vector2 prevDot = verletSimulator.dots[i].position;
            Vector2 currentDot = verletSimulator.dots[i + 1].position;
            chainSegments[i] = Instantiate(i % 2 == 0 ? sideFacingChainSegmentPrefab : frontFacingChainSegmentPrefab, (currentDot + prevDot) / 2, (currentDot - prevDot).ToRotation(90), anchor).transform;
        }
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime * timerSpeed;
        Vector2 anchorPos = anchor.position;
        GetBallPosition(anchorPos, arcStart.position, arcEnd.position, Progress, out float radius, out Vector2 sineOffset);
        transform.Rotate(0, 0, Mathf.Cos(Progress * Helper.Phi) * radius * Helper.Phi * Mathf.Rad2Deg * Time.fixedDeltaTime);
        transform.position = anchorPos + sineOffset;
        UpdateChain(anchorPos, sineOffset);
    }

    private void UpdateChain(Vector2 anchorPos, Vector2 sineOffset)
    {
        int dotCount = verletSimulator.dots.Count;
        verletSimulator.dots[0].position = anchorPos;

        float ballDist = Mathf.Lerp((anchor.position - arcStart.position).magnitude, (anchor.position - arcEnd.position).magnitude, Progress);
        int dotCountForChainDist = (int)((ballDist - BallSize / 2f + ChainSegmentSize) / ChainSegmentSize);

        verletSimulator.dots[^1].position = anchorPos + (sineOffset.normalized * ChainSegmentSize) * dotCountForChainDist;
        verletSimulator.AddForce(new Vector2(0, -50));
        verletSimulator.Simulate(Time.fixedDeltaTime);
        for (int j = 0; j < dotCount - 1; j++)
        {
            Vector2 prevDot = verletSimulator.dots[j].position;
            Vector2 currentDot = verletSimulator.dots[j + 1].position;
            chainSegments[j].SetPositionAndRotation((currentDot + prevDot) / 2, (currentDot - prevDot).ToRotation(90));
        }
    }

    public void StartAnimation(float animationDuration)
    {
        timerSpeed = 1f / animationDuration;
        timer = 0;
        transform.position = arcStart.position;
        Vector2 anchorPos = anchor.position;
        Vector2 sineOffset = (Vector2)arcStart.position - anchorPos;
        for (int i = 0; i < 4; i++)
        {
            int dotCount = verletSimulator.dots.Count;
            verletSimulator.dots[0].position = anchorPos;
            verletSimulator.dots[^1].position = anchorPos + (sineOffset.normalized * ChainSegmentSize) * dotCount;
            verletSimulator.Simulate(0.1f);
            for (int j = 0; j < dotCount - 1; j++)
            {
                Vector2 prevDot = verletSimulator.dots[j].position;
                Vector2 currentDot = verletSimulator.dots[j + 1].position;
                chainSegments[j].SetPositionAndRotation((currentDot + prevDot) / 2, (currentDot - prevDot).ToRotation(90));
            }
        }
    }

    static Vector2 GetBallPosition(Vector2 arcCenter, Vector2 arcStart, Vector2 arcEnd, float progress, out float radius, out Vector2 sineOffset)
    {
        radius = Mathf.Lerp(Vector2.Distance(arcCenter, arcStart), Vector2.Distance(arcCenter, arcEnd), progress);
        float startAngle = Mathf.Atan2(arcStart.y - arcCenter.y, arcStart.x - arcCenter.x);
        float endAngle = Mathf.Atan2(arcEnd.y - arcCenter.y, arcEnd.x - arcCenter.x);
        float currentAngle = Mathf.LerpAngle(startAngle, endAngle, progress);
        sineOffset = new Vector2(Mathf.Cos(currentAngle) * radius, Mathf.Sin(currentAngle) * radius);
        arcCenter += sineOffset;
        return arcCenter;
    }


#if UNITY_EDITOR
    public static void DrawArc(Vector2 arcCenter, Vector2 arcStart, Vector2 arcEnd)
    {
        int segments = (int)(Mathf.Max(Vector2.Distance(arcCenter, arcStart), Vector2.Distance(arcEnd, arcCenter)) * 2);

        float startAngle = Mathf.Atan2(arcStart.y - arcCenter.y, arcStart.x - arcCenter.x);
        float endAngle = Mathf.Atan2(arcEnd.y - arcCenter.y, arcEnd.x - arcCenter.x);
        Vector2 previousPoint = arcStart;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float currentAngle = Mathf.LerpAngle(startAngle, endAngle, t);
            float radius = Mathf.Lerp(Vector2.Distance(arcCenter, arcStart), Vector2.Distance(arcEnd, arcCenter), t);

            Vector2 currentPoint = new Vector2(
                arcCenter.x + Mathf.Cos(currentAngle) * radius,
                arcCenter.y + Mathf.Sin(currentAngle) * radius
            );
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
    

    private void OnDrawGizmos()
    {
        DrawArc(anchor.position, arcStart.position, arcEnd.position);
    }
#endif
}
