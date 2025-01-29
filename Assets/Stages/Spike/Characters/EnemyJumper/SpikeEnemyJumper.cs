using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class SpikeEnemyJumper : Enemy
{

    const float WalkSpeed = 7f;
    const float AggroRange = 7f;
    const float JumpRange = 2.2f;
    const float WarnTime = .3f;
    const float JumpXSpeed = 8f;
    const float StuckDuration = .2f;
    const float VerticalRange = 9;
    const float MinTimeToReach = 0.4f;
    const float MaxTimeToReach = 0.7f;
    const float TimeForcedIdleAfterUnstuck = 0.5f;
    const float JumpVelForClimbingTiles = 20f;
    public override int LifeMax => 16;
    [SerializeField] Animator animator;
    [SerializeField] float timer;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource footstepAudioSource;
    FootstepSimulator footstepSimulator;
    int state = 0;
    const int StateIDIdle = 0;
    const int StateIDWalkToPlayer = 1;
    const int StateIDPrepareJump = 2;
    const int StateIDJumping = 3;
    const int StateIDStuck = 4;
    static readonly int animIdle = Animator.StringToHash("Idle");
    static readonly int animPrepare = Animator.StringToHash("Prepare");
    static readonly int animJump = Animator.StringToHash("Jump");
    static readonly int animWalk = Animator.StringToHash("Walk");
    bool grounded;
    public override void Start()
    {
        base.Start();
        footstepSimulator = new(CommonSounds.Footstep, .125f, footstepAudioSource, 2f);
    }
    void Update()
    {
        //always set Z to 0 because was having some strange issues.
        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;
        switch (state)
        {
            case StateIDIdle:
                State_Idle();
                break;
            case StateIDWalkToPlayer:
                State_WalkToPlayer();
                break;
            case StateIDPrepareJump:
                State_Jump();
                break;
            case StateIDJumping:
                State_Jumping();
                break;
            case StateIDStuck:
                State_Stuck();
                break;
            default:
                state = StateIDIdle;
                timer = -Time.deltaTime;
                break;
        }
        timer += Time.deltaTime;
    }
    void State_Idle()
    {
        rb.rotation = Helper.Decay(rb.rotation, 0, 20);
        animator.CrossFade(animIdle, 0);
        if (timer > Random.Range(3, 20))
        {
            sprite.flipX = Random2.Bool;
            timer = 0;
        }
        if (timer >= 0 && Helper.EnemyAggroCheck(transform.position, GameManager.PlayerPosition, AggroRange, VerticalRange))
        {
            timer = -Time.deltaTime;
            state = StateIDWalkToPlayer;
            footstepSimulator.QueueSound();
        }
    }
    void State_WalkToPlayer()
    {
        rb.rotation = 0;
        float deltaX = GameManager.PlayerPosition.x - transform.position.x;
        float absDeltaX = Mathf.Abs(deltaX);
        bool inJumpRange = absDeltaX < JumpRange;
        Vector2 vel = new(0, rb.velocity.y);
        if (!inJumpRange)
        {
            vel.x = Mathf.Sign(deltaX) * WalkSpeed;
        }
        if (grounded)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(Mathf.Sign(deltaX), 0), .5f, Layers.Tiles);
            if (hit.collider != null && hit.collider.CompareTag(Tags.Tiles))
            {
                vel.y = JumpVelForClimbingTiles;
            }
        }
        rb.velocity = vel;
        FlipSprite(Mathf.Sign(deltaX));
        if (grounded)
        {
            if (vel.x > 0)
            {
                footstepSimulator.Update();
            }
            animator.CrossFade(animWalk, 0);
        }
        else
        {
            animator.CrossFade(animJump, 0);
        }
        if ((absDeltaX < JumpRange || timer > 2f) && grounded)
        {
            rb.velocity = Vector2.zero;
            timer = -Time.deltaTime;
            state = StateIDPrepareJump;
        }
        else if (absDeltaX > 7f)//if too far away, deaggro
        {
            rb.velocity = Vector2.zero;
            timer = -Time.deltaTime;
            state = StateIDIdle;
        }
    }
    void State_Jump()
    {
        if (timer < WarnTime)
        {
            animator.CrossFade(animPrepare, 0);
        }
        else
        {
            timer = -Time.deltaTime;
            state = StateIDJumping;
        }
    }
    void State_Jumping()
    {
        animator.CrossFade(animJump, 0);
        if (timer == 0)
        {
            float jumpXSpeed = JumpXSpeed;
            Vector2 targetPos = GameManager.PlayerPosition + new Vector3(0, 0.5f);//target the head of the player, not the torso
            Vector2 currentPos = transform.position;
            float fallSpeed = Physics.gravity.y * rb.gravityScale;
            //S = S0 + V0*t + (a*t*t)/2
            float deltaX = targetPos.x - currentPos.x;
            float timeToReach = Mathf.Abs(deltaX) / jumpXSpeed;
            if (timeToReach < MinTimeToReach)
            {
                timeToReach = MinTimeToReach;
                jumpXSpeed = Mathf.Abs(deltaX) / timeToReach;
            }
            if(timeToReach > MaxTimeToReach)
            {
                timeToReach = MaxTimeToReach;
                jumpXSpeed = Mathf.Abs(deltaX) / timeToReach;
            }
            float yVel = (targetPos.y - currentPos.y - .5f * fallSpeed * timeToReach * timeToReach) / timeToReach;
            Vector2 jumpVel = new(jumpXSpeed * Mathf.Sign(deltaX), yVel);
            rb.velocity = jumpVel;
        }

        if (rb.velocity.sqrMagnitude > 1)
        {
            rb.rotation = rb.velocity.Atan2Deg(-90);
        }
        if (timer > 3)
        {
            timer = -1;
            state = StateIDIdle;
            rb.velocity = Vector2.up * 10;
        }
        GetOverlapCircleParams(out Vector2 center, out float radius);
        Collider2D collider = Physics2D.OverlapCircle(center, radius, Layers.Player);
        if (collider != null && collider.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(5);
        }
    }
    void State_Stuck()
    {
        animator.CrossFade(animJump, 0);
        if (timer > StuckDuration)
        {
            timer = -TimeForcedIdleAfterUnstuck;
            state = StateIDIdle;
            rb.mass = 1;
            rb.isKinematic = false;
            rb.velocity = Vector2.up * 10;
        }
    }
    void FlipSprite(float direction)
    {
        sprite.flipX = direction > 0;
    }
    public override bool PreKill()
    {
        CommonSounds.PlayRandom(SpikeStageSingleton.instance.hardwoodHit, audioSource, 1, 1f);
        EffectsHandler.SpawnSmallExplosion(FlipnoteColors.ColorID.Yellow, transform.position, .25f);
        return base.PreKill();
    }
    public override void OnHit(int damageTaken)
    {
        CommonSounds.PlayRandom(SpikeStageSingleton.instance.hardwoodHit, audioSource, 1, 1f);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == StateIDJumping && timer > .1f)
        {
            if (Helper.TileCollision(collision))
            {
                timer = 0;
                state = StateIDStuck;
                rb.rotation = -180;
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
                rb.angularVelocity = 0;
                rb.mass = 1000000;//max mass
            }
            else if (collision.gameObject.CompareTag(Tags.Player))
            {
                //rb.velocity = Vector2.Reflect(rb.velocity, collision.GetContact(0).normal);
                //rb.velocity *= 4;
                GameManager.PlayerLife.Damage(5);
                rb.velocity = collision.GetContact(0).normal * 15;
            }
        }
        if (collision.gameObject.CompareTag(Tags.Tiles))
        {
            grounded = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Tags.Tiles))
        {
            grounded = false;
        }
    }
    void GetOverlapCircleParams(out Vector2 center, out float radius)
    {
        center = rb.position + (Mathf.Deg2Rad * rb.rotation + Mathf.PI / 2f).PolarVector(.5f);
        radius = .25f;
    }
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        GetOverlapCircleParams(out Vector2 center, out float radius);
        Gizmos.DrawWireSphere(center, radius);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos2.DrawEnemyAggroArea(transform.position, AggroRange, VerticalRange);
    }
#endif
}
