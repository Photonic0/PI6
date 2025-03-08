using Assets.Common.Consts;
using Assets.Helpers;
using System;
using UnityEngine;

public class SpikeEnemySlam : Enemy
{
    const int stateIDIdle = 0;
    const int stateIDPrepareAttack = 1;
    const int stateIDAttack = 2;
    const int stateIDFollowThrough = 3;
    const float AggroDistX = 7.1f;
    const float AggroYOffset = .5f;
    private const float AggroDistY = 3;

    const float attackFramesDuration = .15f;
    static readonly int animIdle = Animator.StringToHash("idle");
    static readonly int animAttack0 = Animator.StringToHash("attack0");
    static readonly int animAttack1 = Animator.StringToHash("attack1");
    static readonly int animAttack2 = Animator.StringToHash("attack2");
    static readonly int animAttack3 = Animator.StringToHash("attack3");
    static readonly int animAttack4 = Animator.StringToHash("attack4");
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource shockwaveAudioSource;
    [SerializeField] Animator animator;
    [SerializeField] new Transform transform;
    [SerializeField] new BoxCollider2D collider;
    [SerializeField] SpriteRenderer sprite;
    public override int LifeMax => 10;
    float timer;
    int state;
    void Update()
    {
        switch (state)
        {
            case stateIDIdle:
                State_Idle();
                break;
            case stateIDPrepareAttack:
                State_PrepareAttack();
                break;
            case stateIDAttack:
                State_Attack();
                break;
            case stateIDFollowThrough:
                State_FollowThrough();
                break;
            default:
                state = animIdle;
                timer = -Time.deltaTime;
                break;
        }
        timer += Time.deltaTime;
    }
    bool IsPlayerInAggroRange()
    {
        Vector2 currentPos = transform.position;
        Vector2 playerpos = GameManager.PlayerPosition;
        return Mathf.Abs(playerpos.x - currentPos.x) < AggroDistX && MathF.Abs(playerpos.y - (currentPos.y + AggroYOffset)) < AggroDistY;
    }
    private void State_Idle()
    {
        animator.CrossFade(animIdle, 0);


        if (IsPlayerInAggroRange())
        {
            state = stateIDPrepareAttack;
            timer = -Time.deltaTime;
        }
    }
    private void State_PrepareAttack()
    {
        if (timer < attackFramesDuration)
        {
            animator.CrossFade(animAttack0, 0);
        }
        else if (timer < attackFramesDuration * 2)
        {
            animator.CrossFade(animAttack1, 0);
        }
        else if (timer < attackFramesDuration * 3)
        {
            animator.CrossFade(animAttack2, 0);
        }
        else if (timer < attackFramesDuration * 4)
        {
            animator.CrossFade(animAttack3, 0);
        }
        else
        {
            state = stateIDAttack;
            timer = -Time.deltaTime;
            animator.CrossFade(animAttack4, 0);
        }
    }
    private void State_Attack()
    {
        animator.CrossFade(animAttack4, 0);
        if(timer <= float.Epsilon)
        {
            ScreenShakeManager.AddSmallShake();
            SpikeWaveSpike.StartSpikeWave(transform.position - new Vector3(0, 1.5f), 3f, 8, .25f, .1f, shockwaveAudioSource);
            if(Physics2D.OverlapCircle(transform.position, 1f, Layers.PlayerHurtbox) != null)
            {
                GameManager.PlayerLife.Damage(5);
            }
        }
        if(timer > attackFramesDuration)
        {
            if (IsPlayerInAggroRange())
            {
                state = stateIDPrepareAttack;
                timer = attackFramesDuration - Time.deltaTime;
            }
            else
            {
                timer = -Time.deltaTime;
                state = stateIDFollowThrough;
            }
        }
    }
    private void State_FollowThrough()
    {
        if (timer < attackFramesDuration)
        {
            animator.CrossFade(animAttack1, 0);
        }
        else if (timer < attackFramesDuration * 2)
        {
            animator.CrossFade(animAttack0, 0);
        }
        else if (timer < attackFramesDuration * 3)
        {
            animator.CrossFade(animIdle, 0);
        }
        else
        {
            state = stateIDIdle;
            timer = -Time.deltaTime;
        }
    }
    public override void OnHit(int damageTaken)
    {
        CommonSounds.PlayRandom(SpikeStageSingleton.instance.hardwoodHit, audioSource, 1, 1f);
    }
    public override bool PreKill()
    {
        CommonSounds.PlayRandom(SpikeStageSingleton.instance.hardwoodHit, audioSource, 1, 1f);
        Vector2 explosionPos = transform.position;
        //explosionPos.x -= 1;
        EffectsHandler.SpawnMediumExplosion(Assets.Common.Consts.FlipnoteColors.ColorID.Yellow, explosionPos);
        //explosionPos.x += 2;
        //EffectsHandler.SpawnSmallExplosion(Assets.Common.Consts.FlipnoteColors.ColorID.Yellow, transform.position, .3f);
        enabled = false;
        collider.enabled = false;
        sprite.enabled = false;
        Destroy(gameObject, 1f);//let any remaining sound effects play out
        RollForDrop();
        return false;
    }
#if UNITY_EDITOR
 private void OnDrawGizmosSelected()
    {
        Vector2 pos = transform.position;
        pos.y += AggroYOffset;
        Gizmos2.DrawRectangle(pos.x - AggroDistX, pos.x + AggroDistX, pos.y - AggroDistY, pos.y + AggroDistY);
    }
#endif
   
}
