using Assets.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SwingingSpikeBall : Projectile
{

    [SerializeField] new Transform transform;
    [SerializeField] Transform anchor;
    [SerializeField] float timerOffset;
    [SerializeField] float oscillationSpeed;
    [SerializeField] int verletIterations;
    [SerializeField] float ballDist;
    [SerializeField] GameObject frontFacingChainSegmentPrefab;
    [SerializeField] GameObject sideFacingChainSegmentPrefab;
    [SerializeField] Transform[] chainSegments;
    const float ChainSegmentSize = 0.45f;
    const float BallSize = 1.041f;
    const float CullSimulationDistanceSqr = 500;
    VerletSimulator verletSimulator;
    public override int Damage => 5;


    void Start()
    {
        //random offsets so they aren't all synced up,  but based on transform x so it's consistent between checkpoints
        System.Random rnd = new System.Random((int)transform.position.x * 200);
        timerOffset += (float)rnd.NextDouble() * Mathf.PI * 2;

        transform.Rotate(0, 0, Random2.Float(Mathf.PI));
        InitializeVerlet();
    }
    // make spike shockwave guy
    //make spike ball that detachs
    private void InitializeVerlet()
    {
        verletSimulator = new VerletSimulator(1, verletIterations);
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
#if UNITY_EDITOR
        if (debug_clickToInitializeVerlet)
        {
            InitializeVerlet();
            debug_clickToInitializeVerlet = false;
        }
#endif
        //distance culling
        if((GameManager.PlayerPosition - anchor.position).sqrMagnitude > CullSimulationDistanceSqr)
        {
            return;
        }

        int dotCount = verletSimulator.dots.Count;
        float timer = Time.time + timerOffset;
        Vector2 sineOffset = (Mathf.Sin(timer * oscillationSpeed) + Mathf.PI).PolarVector_Old(ballDist);
        transform.Rotate(0, 0, Mathf.Cos(timer * oscillationSpeed * Helper.Phi) * ballDist * Helper.Phi);
        Vector2 anchorPos = anchor.position;
        Vector2 spikeBallPos = transform.position;
        verletSimulator.iterations = verletIterations;
        verletSimulator.dots[0].position = anchorPos;
        transform.position = anchorPos + sineOffset;
        verletSimulator.dots[^1].position = anchorPos + (sineOffset.normalized * ChainSegmentSize) * dotCount;
#if UNITY_EDITOR
        verletSimulator.dots[^1].position += (sineOffset * debug_TestExtraDistance);
#endif
        verletSimulator.Simulate(Time.fixedDeltaTime);
        for (int j = 0; j < dotCount - 1; j++)
        {
            Vector2 prevDot = verletSimulator.dots[j].position;
            Vector2 currentDot = verletSimulator.dots[j + 1].position;
            chainSegments[j].position = (currentDot + prevDot) / 2;
            chainSegments[j].rotation = (currentDot - prevDot).ToRotation(90);
        }
        timer += Time.fixedDeltaTime;
    }

#if UNITY_EDITOR
    [SerializeField] bool debug_clickToInitializeVerlet;
    [SerializeField] bool debug_clickToSetBallHeightInEditor;
    [SerializeField] float debug_TestExtraDistance;
    private void OnDrawGizmos()
    {
        Vector3 anchorPos = anchor.position;
        float timer = Time.time + timerOffset;
        Vector2 sineOffset = (Mathf.Sin(timer * oscillationSpeed)).PolarVector_Old(ballDist);
        //sineOffset = new Vector2(-sineOffset.y, sineOffset.x);
        Gizmos.color = Color.red;
        float colliderRadius = 0.8f;
        Gizmos2.DrawArc(anchorPos, ballDist, -Mathf.PI / 2f, 2f, 20);
        Gizmos2.DrawCappedArc(anchorPos, ballDist, -Mathf.PI / 2f, 2f, colliderRadius * 2f);
        if (debug_clickToSetBallHeightInEditor && !Application.isPlaying)
        {
            transform.position = anchorPos - (Vector3)sineOffset;
            //debug_clickToSetBallHeightInEditor = false;
        }
        if (verletSimulator != null)
        {
            for (int i = 0; i < verletSimulator.dots.Count; i++)
            {
                Gizmos.DrawWireSphere(verletSimulator.dots[i].position, 0.05f);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(anchor.position, Mathf.Sqrt(CullSimulationDistanceSqr));
    }
#endif
    //private void Chain_Old()
    //{
    //    Vector3 anchorPos = anchor.position;
    //    chainPoints[^1].transform.position = anchorPos - new Vector3(0, ChainSegmentSize * .5f);
    //    float chainLength = ChainSegmentSize * chainPoints.Length + ChainSegmentSize * .5f;
    //    float angle = Mathf.Sin(timer * 2) + Mathf.PI;
    //    Vector3 currentPos = anchorPos + (Vector3)angle.PolarVector(chainLength + BallSize / 2);
    //    Transform firstChain = chainPoints[0];

    //    firstChain.position = currentPos + Vector3.Normalize(anchorPos - currentPos) * BallSize;
    //    for (int i = 1; i < chainPoints.Length; i++)
    //    {
    //        Transform chain = chainPoints[i];
    //        Transform prevChain = chainPoints[i - 1];
    //        Vector3 chainPos = chain.position;
    //        Vector3 prevChainPos = prevChain.position;
    //        chain.position = prevChainPos - Vector3.Normalize(prevChainPos - chainPos) * ChainSegmentSize;
    //    }

    //    transform.position = currentPos;
    //}

    //private class Vertex_Old
    //{
    //    public const float GRAV = 0.9f;          // force of gravity
    //    public const float GROUND = -900f;        // Y position of the ground. Y is down the screen
    //    public const float GROUND_BOUNCE = 0.5f; // Amount of bounce from the ground
    //    public const float DRAG = 0.9f;          // Amount of friction or drag to apply per frame
    //    public float x;
    //    public float y;
    //    public float lx;//last x
    //    public float ly;//last y

    //    void Update()
    //    {
    //        float dx = (x - lx) * DRAG;  // get the speed and direction as a vector 
    //        float dy = (y - ly) * DRAG;  // including drag
    //        lx = x;   // set the last position to the current
    //        ly = y;
    //        x += dx;         // add the current movement
    //        y += dy;
    //        y += GRAV;       // add the gravity
    //    }
    //    void ConstrainToGround(float vx, ref float speed)
    //    {
    //        if (y > GROUND)
    //            return;
    //        // we need the current speed
    //        float dx = (x - lx) * DRAG;
    //        float dy = (y - ly) * DRAG;
    //        speed = Mathf.Sqrt(dx * dx + dy * dy);
    //        y = GROUND;  // put the current y on the ground
    //        ly = GROUND + dy * GROUND_BOUNCE; // set the last point into the 
    //                                                 // ground and reduce the distance 
    //                                                 // to account for bounce
    //        lx += (dy / speed) * vx; // depending on the angle of contact

    //    }
    //}
}
