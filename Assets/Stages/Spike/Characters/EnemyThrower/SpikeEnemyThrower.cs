using Assets.Helpers;
using Assets.Systems;
using UnityEngine;

public class SpikeEnemyThrower : Enemy
{
    public override int LifeMax => 9;
    const float AggroRange = 9;
    const float VerticalRange = 6;
    [SerializeField] GameObject parentObject;
    [SerializeField] SpikeBall[] spikeBalls;
    [SerializeField] Animator animator;
    [SerializeField] Vector2 throwDirection;
    [SerializeField] float timer;
    [SerializeField] new Transform transform;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] AudioSource audioSource;
    [SerializeField] float targetDetectionOffsetX;
    SpikeBall currentSpikeBall;
    int state = 0;
    const int StateIDIdle = 0;
    const int StateIDGetBall = 1;
    const int StateIDThrow = 3;
    static readonly int animIdle = Animator.StringToHash("spikeEnemyThrowerIdle");
    static readonly int animGetBall1 = Animator.StringToHash("spikeEnemyThrowerGetBall1");
    static readonly int animGetBall2 = Animator.StringToHash("spikeEnemyThrowerGetBall2");
    static readonly int animThrowBallPrepare = Animator.StringToHash("spikeEnemyThrowerThrowBallPrepare");
    static readonly int animThrowBall = Animator.StringToHash("spikeEnemyThrowerThrowBall");
    static readonly int animThrowBallToIdle = Animator.StringToHash("spikeEnemyThrowerThrowBallToIdle");
    bool HasSpikeBall => currentSpikeBall != null;
    float DirectionSign => Mathf.Sign(transform.position.x - GameManager.PlayerPosition.x);
    public override void Start()
    {
        base.Start();
        DiscardCurrentSpikeBall();
    }
    void Update()
    {
        AdjustDirection();
        switch (state)
        {
            case StateIDIdle:
                State_Idle();
                break;
            case StateIDGetBall:
                State_GetBall();
                break;
            case StateIDThrow:
                State_Throw();
                break;
            default:
                state = StateIDIdle;
                timer = 0;
                break;
        }
        timer += Time.deltaTime;
    }
    void State_Idle()
    {
        bool playerClose = Helper.EnemyAggroCheck(transform.position + new Vector3(targetDetectionOffsetX, 0), GameManager.PlayerPosition, AggroRange, VerticalRange);
        animator.CrossFade(animIdle, 0);
        if (playerClose && timer > .3f)
        {
            state = StateIDGetBall;
            timer = 0;
        }
    }
    void State_GetBall()
    {
        float getBall1Duration = .15f;
        float ballGenerateDuration = .15f;
        float ballHoldDuration = .15f;
        if (timer < getBall1Duration)
        {
            animator.CrossFade(animGetBall1, 0);
        }
        else if (timer < getBall1Duration + ballGenerateDuration + ballHoldDuration)
        {
            animator.CrossFade(animGetBall2, 0);
            PrepareSpikeBall();
            if (HasSpikeBall)
            {
                float ballScale = Helper.Remap(timer - getBall1Duration, 0, ballGenerateDuration, 0, 1, Easings.InOutSineClamped);
                currentSpikeBall.transform.localScale = new Vector3(ballScale, ballScale, ballScale);
            }
            else
            {
                //-deltaTime because timer is increased by deltaTime at the end of the update
                timer = getBall1Duration - Time.deltaTime;
            }
        }
        else
        {
            timer = 0;
            state = StateIDThrow;
        }
    }
    void State_Throw()
    {
        float windupDuration = .15f;
        float throwDuration = .15f;
        float transitionToIdleDuration = .15f;
        if (timer < windupDuration)
        {
            animator.CrossFade(animThrowBallPrepare, 0);
            if (HasSpikeBall)
            {
                currentSpikeBall.transform.localPosition = new Vector3(0.723f * DirectionSign, 1.359f);
                currentSpikeBall.rb.rotation = 25;
            }
        }
        else if (timer < windupDuration + throwDuration)
        {
            if (HasSpikeBall)
            {
                Vector2 throwVel = throwDirection.normalized;
                if (throwVel.x == 0)
                {
                    throwVel.x = 1;
                    throwVel.Normalize();
                }
                throwVel /= Mathf.Abs(throwVel.x);
                throwVel *= 5;//ensures always 5 velocity on the x axis, while keeping the throw direction vector
                throwVel.x *= DirectionSign;
                currentSpikeBall.rb.velocity = throwVel;
                currentSpikeBall.transform.localPosition = new Vector3(-0.987f * DirectionSign, 0.92f);
                currentSpikeBall.rb.rotation = 90;
                currentSpikeBall.EnablePhysics();
                currentSpikeBall = null;
                CommonSounds.PlayThrowSound(audioSource);
            }
            animator.CrossFade(animThrowBall, 0);
        }
        else if (timer < windupDuration + throwDuration + transitionToIdleDuration)
        {
            animator.CrossFade(animThrowBallToIdle, 0);
        }
        else//timer >= windupDuration + throwDuration + transitionToIdleDuration
        {
            animator.CrossFade(animIdle, 0);
            timer = 0;
            state = StateIDIdle;
        }
    }
    void DiscardCurrentSpikeBall()
    {
        if (currentSpikeBall != null)
        {
            currentSpikeBall.gameObject.SetActive(false);
            currentSpikeBall = null;
        }
    }
    void PrepareSpikeBall()
    {
        if (currentSpikeBall == null)
        {
            if (Helper.TryFindFreeIndex(spikeBalls, out int index))
            {
                currentSpikeBall = spikeBalls[index];
                currentSpikeBall.transform.localScale = Vector3.zero;
                currentSpikeBall.transform.localPosition = new Vector3(.097f * DirectionSign, 1.364f);
                currentSpikeBall.gameObject.SetActive(true);
                currentSpikeBall.DisablePhysics();
                currentSpikeBall.rb.rotation = -314;
            }
        }
    }
    void AdjustDirection()
    {
        sprite.flipX = DirectionSign < 0;
    }
    public override void OnHit(int damageTaken)
    {
        CommonSounds.PlayRandom(SpikeStageSingleton.instance.hardwoodHit, audioSource, 1, 1f);
    }
    public override bool PreKill()
    {
        CommonSounds.PlayRandom(SpikeStageSingleton.instance.hardwoodHit, audioSource, 1, 1f);
        DiscardCurrentSpikeBall();
        GetComponent<Collider2D>().enabled = false;
        sprite.enabled = false;
        animator.enabled = false;
        if(TryGetComponent(out Rigidbody2D rb))
        {
            rb.simulated = false;
        }
        enabled = false;
        Destroy(parentObject, 5);
        EffectsHandler.SpawnMediumExplosion(Assets.Common.Consts.FlipnoteColors.ColorID.Yellow, transform.position);
        return base.PreKill();
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos2.DrawEnemyAggroArea(transform.position + new Vector3(targetDetectionOffsetX,0), AggroRange, VerticalRange);
    }
#endif
}
