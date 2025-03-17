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
    [SerializeField] AudioSource audioSource;
    public override void Start()
    {
        base.Start();
        transform = base.transform;
        TyphoonStageSingleton.AddToLightningCloudEnemyList(this);
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
            RaycastHit2D hit = Physics2D.Raycast(playerPos, Vector2.down, 20f, Layers.Tiles);
            Vector2 hitPoint = hit.point;
            if (hit.point == Vector2.zero)
            {
                GetFailsafeScanParams(out float dist, out Vector2[] directions, out int layermask, out Vector2 origin);
                Vector2[] hitPoints = new Vector2[directions.Length];
                for (int i = 0; i < directions.Length; i++)
                {
                    hitPoints[i] = Physics2D.Raycast(origin, directions[i], dist, layermask).point;
                }
                hitPoint = GetClosestPosition(hitPoints, playerPos);
            }

            attackDirection = (hitPoint - (Vector2)transform.position).normalized;
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

            Helper.TelegraphLightning(timer, start, end, AttackTelegraphDuration, lightningTelegraphParticles);
        }
        else if (timer < AttackDuration + AttackTelegraphDuration)
        {
            GetBoxParams(out Vector2 start, out Vector2 end, out Vector2 boxCenter, out Vector2 boxSize);
            if (!lightningEffect.gameObject.activeInHierarchy)
            {
                lightningEffect.ActivateAndSetAttributes(0.1f, start, end, AttackDuration);
                EffectsHandler.SpawnSmallExplosion(FlipnoteStudioColors.ColorID.Yellow, end, AttackDuration);
            }
            Collider2D playerCollider = Physics2D.OverlapBox(boxCenter, boxSize, attackDirection.Atan2Deg(), Layers.PlayerHurtbox);
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
    private void GetFailsafeScanParams(out float dist, out Vector2[] directions, out int layermask, out Vector2 origin)
    {
        const int amountOfScans = 30;
        dist = 10f;
        origin = transform.position;
        layermask = Layers.Tiles;
        directions = new Vector2[amountOfScans];
        for (int i = 0; i < amountOfScans; i++)
        {
            float angle = (float)i / (amountOfScans - 1) * Mathf.PI + Mathf.PI;
            directions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

    }
    public static Vector2 GetClosestPosition(Vector2[] positions, Vector2 target)
    {
        Vector2 closestPosition = positions[0];
        float minDistance = (closestPosition - target).sqrMagnitude;
        for (int i = 1; i < positions.Length; i++)
        {
            float distance = (positions[i] - target).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPosition = positions[i];
            }
        }
        return closestPosition;
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
        CommonSounds.Play(TyphoonStageSingleton.instance.typhoonEnemyDeath, audioSource);
        lightningEffect.Stop();
        TyphoonStageSingleton.RemoveLightningCloudEnemyFromList(this);
        EffectsHandler.SpawnSmallExplosion(FlipnoteStudioColors.ColorID.Blue, transform.position, 0.25f);
        gameObject.SetActive(false);
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
    private void OnDisable()
    {
        TyphoonStageSingleton.RemoveLightningCloudEnemyFromList(this);
    }
    private void OnDestroy()
    {
        TyphoonStageSingleton.RemoveLightningCloudEnemyFromList(this);
    }
}
