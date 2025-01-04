using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class DiscoEnemyConePopper : Enemy, IMusicSyncable
{
    static readonly int AnimIDRise = Animator.StringToHash("Rise");
    static readonly int AnimIDFall = Animator.StringToHash("Fall");
    static readonly int AnimIDAttack = Animator.StringToHash("Attack");
    static readonly int AnimIDPreAttack = Animator.StringToHash("PreAttack");
    static readonly int AnimIDDefenseless = Animator.StringToHash("Defenseless");
    public int BeatsPerAction => 1;
    public int BeatOffset => 0;

    public override int LifeMax => 14;
    [SerializeField] int state = 0;
    [SerializeField] float timer;
    const int StateIDChasingOrPassive = 0;
    const int StateIDAboutToAttack = 1;
    const int StateIDPostAttack = 2;
    [SerializeField] new Transform transform;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] ParticleSystem confettiParticles;
    [SerializeField] int direction;
    [SerializeField] SpriteRenderer sprite;
    public void DoMusicSyncedAction()
    {
        if (state == StateIDPostAttack || state == StateIDAboutToAttack)
        {
            return;
        }
        Vector2 pos = transform.position;
        Vector2 playerPos = GameManager.PlayerPosition;

        if (state == StateIDChasingOrPassive)
        {
            if (Helper.EnemyAggroCheck(pos, playerPos, 3f))
            {
                state = StateIDAboutToAttack;
                timer = 0;
                return;
            }
        }
        float jumpXSpeed = 6;
        float jumpYSpeed = 25;
        if (Helper.EnemyAggroCheck(pos, playerPos, 6))
        {
            Vector2 vel = rb.velocity;
            vel.x = Mathf.Sign(playerPos.x - pos.x) * jumpXSpeed;
            vel.y = jumpYSpeed;
            rb.velocity = vel;
        }
        else
        {
            int beat = DiscoMusicEventManager.instance.beatCounter;
            Vector2 vel = rb.velocity;
            vel.x = (beat % 2 * 2 - 1) * jumpXSpeed;
            vel.y = jumpYSpeed;
            rb.velocity = vel;
        }
    }

    public override void Start()
    {
        DiscoMusicEventManager.AddSyncableObject(this);
        base.Start();
    }
    void Update()
    {
        direction = (int)Mathf.Sign(GameManager.PlayerPosition.x - transform.position.x);
        UpdateSpriteFlip();
        timer += Time.deltaTime;
        
        switch (state)
        {
            case StateIDChasingOrPassive:
                Vector2 vel = rb.velocity;
                //get rid of very small decimal values
                vel.x = Mathf.Round(vel.x * 1000f) / 1000f;
                vel.y = Mathf.Round(vel.y * 1000f) / 1000f;
                if (vel.y < 0)
                {
                    animator.CrossFade(AnimIDFall, 0);
                }
                else if (vel.y > 0)
                {
                    animator.CrossFade(AnimIDRise, 0);
                }
                else
                {
                    animator.CrossFade(AnimIDDefenseless, 0);
                }
                break;

            case StateIDPostAttack:
                if(timer > .3f)
                {
                    animator.CrossFade(AnimIDDefenseless, 0);
                }
                break;
            default://StateIDAboutToAttack
                if (timer > 0.3f)
                {
                    animator.CrossFade(AnimIDAttack, 0);
                    for (int i = 0; i < 30; i++)
                    {
                        ParticleSystem.EmitParams emitParams = new();
                        float dirSign = direction;
                        Vector3 offset = new Vector2(0.5f * dirSign, 0.1f);
                        emitParams.position = transform.position + offset - confettiParticles.transform.position;
                        Vector2 shootDirection = new(dirSign, 1f);

                        emitParams.velocity = shootDirection.RotatedBy(Random2.Float(-0.35f, 0.35f)) * Random2.Float(8, 18);
                        confettiParticles.Emit(emitParams, 1);
                    }
                    timer = 0;
                    state = StateIDPostAttack;
                    Vector2 pos = transform.position;
                    pos.x += direction;
                    pos.y += .3f;
                    Collider2D playerCollider = Physics2D.OverlapCircle(pos, 1f, Layers.PlayerHurtbox);
                    if(playerCollider == null)
                    {
                        pos.x += direction;
                        pos.y += 1;
                        playerCollider = Physics2D.OverlapCircle(pos, 1f, Layers.PlayerHurtbox);
                    }
                    if(playerCollider != null) 
                    {
                        GameManager.PlayerLife.Damage(4);
                    }
                }
                else
                {
                    animator.CrossFade(AnimIDPreAttack, 0);
                }
                break;
        }


    }
 
    private void UpdateSpriteFlip()
    {
        sprite.flipX = direction == 1;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Helper.TileCollision(collision))
        {
            Vector2 vel = rb.velocity;
            vel.x = 0;
            rb.velocity = vel;
        }
    }
    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        pos.x += direction;
        pos.y += .3f;
        Gizmos.DrawWireSphere(pos, 1f);
        pos.x += direction;
        pos.y += 1;
        Gizmos.DrawWireSphere(pos, 1f);
    }
}
