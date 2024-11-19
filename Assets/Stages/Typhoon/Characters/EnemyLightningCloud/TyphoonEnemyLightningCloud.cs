using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Common.Effects.Lightning;
using Assets.Helpers;
using UnityEngine;

public class TyphoonEnemyLightningCloud : Enemy
{

    public override int LifeMax => 9;
    public const int StateIDIdle = 0;
    public const int StateIDChasingPlayer = 1;
    public const int StateIDAttacking = 2;
    public const float AggroRange = 6;
    public const float MaxMoveSpeed = 5;
    public const float MinDistForStartAttack = 2;
    public const float AttackTelegraphDuration = 0.8f;
    public const float AttackDuration = 1;
    [SerializeField] Transform parent;
    [SerializeField] ParticleSystem deathParticle;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SimpleLightningRenderer lightningEffect;
    [SerializeField] new Transform transform;
    [SerializeField] ParticleSystem lightningTelegraphParticles;
    private void Awake()
    {
        transform = base.transform;
    }
    float timer;
    int state;
    Vector2 attackDirection;
    private void Update()
    {
        switch (state)
        {
            case StateIDIdle:
                State_Idle();
                break;
            case StateIDChasingPlayer:
                State_ChasingPlayer();
                break;
            case StateIDAttacking:
                State_Attacking();
                break;
        }
        timer += Time.deltaTime;
    }


    private void State_Idle()
    {
        rb.velocity = new Vector2(0, Mathf.Cos(timer));
        if (Helper.EnemyAggroCheck(transform.position, GameManager.PlayerControl.Position, AggroRange))
        {
            state = StateIDChasingPlayer;
            timer = -Time.deltaTime;
        }
    }

    private void State_ChasingPlayer()
    {
        Vector2 playerPos = GameManager.PlayerControl.Position;
        Vector2 targetPos = playerPos + new Vector2(0, 2.5f);
        Vector2 center = transform.position;
        for (int i = 0; i < TyphoonStageSingleton.instance.lightningCloudEnemies.Count; i++)
        {
            TyphoonEnemyLightningCloud otherCloud = TyphoonStageSingleton.instance.lightningCloudEnemies[i];
            Vector2 deltaPos = center - (Vector2)otherCloud.transform.position;
            targetPos += deltaPos.normalized * (3 - Mathf.Clamp(deltaPos.magnitude, 0, 3));
        }
        Vector2 toTargetPos = (targetPos - rb.position).normalized;
        rb.velocity = Vector2.Lerp(rb.velocity, toTargetPos * MaxMoveSpeed, Time.deltaTime * 5);
        if (playerPos.y < transform.position.y && Mathf.Abs(playerPos.x - transform.position.x) < MinDistForStartAttack)
        {
            state = StateIDAttacking;
            timer = -Time.deltaTime;
            attackDirection = (playerPos - (Vector2)transform.position).normalized;
        }
    }

    private void State_Attacking()
    {
        Vector2 vel = rb.velocity;
        vel.x = Helper.Decay(vel.x, 0, 20);
        vel.y = Helper.Decay(vel.y, 0, 20);
        rb.velocity = vel;
        if (timer < AttackTelegraphDuration)
        {
            RaycastHit2D hit = Physics2D.Raycast(rb.position, attackDirection, 20, Layers.Tiles);
            Vector2 start = transform.position + new Vector3(0, -0.3f);
            Vector2 end = hit.point;
            
            Vector2 parentPos = lightningTelegraphParticles.transform.position;
            Helper.TelegraphLightning(timer, start - parentPos, end - parentPos, AttackTelegraphDuration, lightningTelegraphParticles);
        }
        else if (timer < AttackDuration + AttackTelegraphDuration)
        {
            GetBoxParams(out Vector2 start, out Vector2 end, out Vector2 boxCenter, out Vector2 boxSize);
            if (!lightningEffect.gameObject.activeInHierarchy)
            {
                lightningEffect.ActivateAndSetAttributes(0.1f, start, end, AttackDuration);
                EffectsHandler.SpawnSmallExplosion(FlipnoteColors.ColorID.Yellow, end, AttackDuration);
            }
            Collider2D playerCollider = Physics2D.OverlapBox(boxCenter, boxSize, attackDirection.Atan2Deg(), Layers.Player);
            if (playerCollider != null)
            {
                if (playerCollider.TryGetComponent(out PlayerLife player))
                {
                    player.Damage(4);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.Log("lightning collision wasn't player?? was " + playerCollider.name + "instead");
                }
#endif
            }
        }
        else
        {
            timer = 0;
            //not necessary because method ActivateAndSetAttributes already sets a duration in which it will deactivate
            //lightningEffect.enabled = false;
            state = StateIDIdle;
        }
    }

    private void GetBoxParams(out Vector2 start, out Vector2 end, out Vector2 boxCenter, out Vector2 boxSize)
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, attackDirection, 20, Layers.Tiles);
        start = transform.position + new Vector3(0, -0.3f);
        end = hit.point;
        boxCenter = (start + end) / 2;
        boxSize = new Vector2((start - end).magnitude, 0.1f);
    }
    public override bool PreKill()
    {
        EffectsHandler.SpawnSmallExplosion(FlipnoteColors.ColorID.Blue, transform.position, 0.25f);
        return base.PreKill();
    }
    public override void OnHit(int damageTaken)
    {
        if (life <= 0)
        {
            deathParticle.transform.position = transform.position;
            deathParticle.Emit(50);
            Destroy(parent.gameObject, 1);
        }
    }
}
