using Assets.Common.Characters.Main.Scripts;
using Assets.Helpers;
using System.Collections.Generic;
using UnityEngine;

public class SwingingSpikeBall : Projectile
{

    [SerializeField] new Transform transform;
    [SerializeField] Transform anchor;
    [SerializeField] float timer;
    [SerializeField] float oscillationSpeed;
    [SerializeField] int verletIterations;
    [SerializeField] float ballDist;
    [SerializeField] LineRenderer line;
    [SerializeField] bool debug_clickToInitializeVerlet;
    const float ChainSegmentSize = .5f;
    const float BallSize = 1.041f;
    VerletSimulator verletSimulator;
    public override int Damage => 5;

    void Start()
    {
        //random offsets so they aren't all synced up
        timer += Random2.Float(Mathf.PI * 2);
        transform.Rotate(0, 0, Random2.Float(Mathf.PI));
        InitializeVerlet();
    }
    // make spike shockwave guy
    //make spike ball that detachs
    private void InitializeVerlet()
    {
        verletSimulator = new VerletSimulator(1, verletIterations);
        int dotCount = 40;
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
            Dot.Connect(dot, dots[i - 1]);
            dots.Add(dot);
        }
    }

    void FixedUpdate()
    {
        if(debug_clickToInitializeVerlet)
        {
            InitializeVerlet();
            debug_clickToInitializeVerlet = false;
        }
        Vector2 sineOffset = (Mathf.Sin(timer * oscillationSpeed) + Mathf.PI).PolarVector(ballDist);
        transform.Rotate(0, 0, Mathf.Cos(timer * oscillationSpeed * Helper.Phi) * ballDist * Helper.Phi);
        Vector2 anchorPos = anchor.position;
        Vector2 spikeBallPos = transform.position;
        verletSimulator.iterations = verletIterations;
        verletSimulator.dots[0].position = anchorPos;
        transform.position = anchorPos + sineOffset;
        verletSimulator.dots[^1].position = spikeBallPos + .73f * BallSize * (anchorPos - spikeBallPos).normalized;
        verletSimulator.Simulate(Time.fixedDeltaTime);
        Color yellow = new(.99f, .91f, 0);
        int dotCount = verletSimulator.dots.Count;
        line.startColor = yellow;
        line.endColor = yellow;
        Vector3[] positions = new Vector3[dotCount];
        for (int j = 0; j < positions.Length; j++)
        {
            positions[j] = verletSimulator.dots[j].position;      
        }
        line.SetPositions(positions);
        timer += Time.fixedDeltaTime;
    }
    private void OnDrawGizmos()
    {
        Vector3 anchorPos = anchor.position;
        Vector3 pos = transform.position;
        Vector2 sineOffset = (Mathf.Sin(timer * oscillationSpeed)).PolarVector(ballDist);
        //sineOffset = new Vector2(-sineOffset.y, sineOffset.x);
        Gizmos.DrawLine(pos, pos + (Vector3)sineOffset);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(anchorPos, pos);
        Gizmos.DrawSphere(anchorPos, .05f);
        Vector3 relativePos = anchorPos - pos;
        relativePos = new Vector3(-relativePos.y, relativePos.x) * Mathf.Cos(timer * oscillationSpeed);
        Gizmos.DrawLine(pos + relativePos, pos);
        if (verletSimulator != null)
        {
            for (int i = 0; i < verletSimulator.dots.Count; i++)
            {
                Dot dot = verletSimulator.dots[i];
                Gizmos.DrawSphere(dot.position, 0.1f);
                if (i > 0)
                {
                    Gizmos.DrawLine(dot.position, verletSimulator.dots[i - 1].position);
                }
            }
        }
    }
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
