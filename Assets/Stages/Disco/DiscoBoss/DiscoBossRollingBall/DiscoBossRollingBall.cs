using Assets.Common.Consts;
using Assets.Helpers;
using System.Collections.Generic;
using UnityEngine;

public class DiscoBossRollingBall : Projectile
{
    [SerializeField] new GameObject gameObject;
    [SerializeField] new Transform transform;
    VerletSimulator rope;
    [SerializeField] LineRenderer line;
    [SerializeField] AudioSource audioSource;
    [SerializeField] new CircleCollider2D collider;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GameObject outlineObj;
    public float timeLeft;
    public Rigidbody2D rb;
    public override int Damage => 5;
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
            Dot dot = new(ballTopPos + (transform.rotation.eulerAngles.z).PolarVector_Old((i - 1) * (RopeLength / dotCount)), false);
            Dot.Connect(dot, dots[i - 1]);
            dots.Add(dot);
        }
        line.positionCount = dotCount;
        for (int i = 0; i < rope.dots.Count; i++)
        {
            line.SetPosition(i, rope.dots[i].position);
        }
    }
    public void FixedUpdate()
    {
        if (timeLeft < 0)
        {
            if (timeLeft < -1)
            {
                gameObject.SetActive(false);
            }
            return;
        }
        if(Physics2D.OverlapCircle(rb.position, collider.radius, Layers.Tiles))
        {
            rb.gravityScale = 0f;
            Vector2 vel = rb.velocity;
            vel.y = 0;
            rb.velocity = vel;
        }
        Vector2 ballCenter = transform.position;
        transform.rotation = rb.velocity.ToRotation(90);
        rope.dots[0].position = GetBallTop() + rb.velocity * Time.fixedDeltaTime;
        rope.AddForce(Physics2D.gravity);
        rope.Simulate(Time.fixedDeltaTime);
        for (int i = 0; i < rope.dots.Count; i++)
        {
            Dot dot = rope.dots[i];
            if (dot.isLocked)
            {
                line.SetPosition(i, rope.dots[i].position);
                continue;
            }
            Vector2 deltaPos = dot.position - ballCenter;
            float minDist = 0.25f;
            if (deltaPos.magnitude < minDist)
            {
                Vector2 targetPos = ballCenter + deltaPos.normalized * minDist;
                deltaPos = targetPos - dot.position;
                dot.position += deltaPos;
            }
            line.SetPosition(i, rope.dots[i].position);
        }
        timeLeft -= Time.fixedDeltaTime;
        if (timeLeft < 0)
        {
            Explode();
        }
    }
    public void SetTimeLeft(Vector2 from, Vector2 to, Vector2 velocity)
    {
        Vector2 deltaPos = (to - from);
        if (Vector2.Dot(deltaPos.normalized, velocity.normalized) > .9f)
        {
            timeLeft = deltaPos.magnitude / velocity.magnitude;
        }
    }
    private void Explode()
    {
        CommonSounds.PlayBwow(audioSource, 1f, 1f);
        EffectsHandler.SpawnMediumExplosion(FlipnoteStudioColors.Magenta, transform.position);
        rb.velocity = Vector2.zero;
        rb.simulated = false;
        collider.enabled = false;
        spriteRenderer.enabled = false;
        line.enabled = false;
        outlineObj.SetActive(false);
    }
    private void OnEnable()
    {
        outlineObj.SetActive(true);
        spriteRenderer.enabled = true;
        line.enabled = true;
    }
    public void DisablePhysics()
    {
        rb.simulated = false;
        collider.enabled = false;
        rb.isKinematic = true;
    }
    public void EnablePhysics()
    {
        rb.simulated = true;
        rb.isKinematic = false;
        collider.enabled = true;
    }
    Vector2 GetBallTop()
    {
        Vector2 center = transform.position;
        center += (transform.rotation.eulerAngles.z * -Mathf.Deg2Rad).PolarVector_Old(0.4f);//a bit inside the ball is better, makes it more cohesive visually
        return center;
    }

}
