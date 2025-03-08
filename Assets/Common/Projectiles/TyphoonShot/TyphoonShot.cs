
using Assets.Common.Consts;
using Assets.Common.Effects.Lightning;
using Assets.Common.Interfaces;
using Assets.Helpers;
using UnityEngine;

public class TyphoonShot : Projectile
{
    public override int Damage => 2;

    //references for the information we need to read from/write to
    [SerializeField] SimpleLightningRenderer[] lightningRendererPool;
    public Rigidbody2D rb; // handles physics
    [SerializeField] new Transform transform; //stores position, rotation and scaling
    float[] hitTimers;//timer to track the hit rate
    [SerializeField] Transform[] enemiesTargeted;//this is so we can adjust the position of the lightning visual
    [SerializeField] SpriteRenderer spriteRenderer;
    float lifetimeTimer;
    [SerializeField] SpriteRenderer backSprite;
    const float SecondsPerHit = .3f;
    const float LightningVisualDuration = 0.3f;
    const float TotalDuration = 2f;//orb will last 2 seconds before fading out
    const float FadeOutDuration = 0.2f;//after 2 seconds, fade out over the course of 0.2 seconds
    const float DistForLosingTrackOfEnemy = 7f;
    public const float MaxVel = 2.5f;
    const float DistForDetectingEnemy = 5f;
    const int MaxTargets = 5;
    //executes when the object that has this code is first loaded
    private void Awake()
    {
        //we will be able to store up to 5 transform components in this list
        enemiesTargeted = new Transform[MaxTargets];
        hitTimers = new float[MaxTargets];
    }
    private void Start()
    {
        OnEnable();
    }
    private void OnEnable()
    {
        lifetimeTimer = 0;
        for (int i = 0; i < MaxTargets; i++)
        {
            hitTimers[i] = 0;
            enemiesTargeted[i] = null;
            lightningRendererPool[i].Stop();
        }
    }
    private void Update()
    {
        lifetimeTimer += Time.deltaTime;
        //interpolate the color of the sprite between normal and transparent
        //based on if it the projectile duration has expired and how long the fadeout should last
        float t = Mathf.InverseLerp(TotalDuration, TotalDuration + FadeOutDuration, lifetimeTimer);
        Color color = Color.Lerp(Color.white, Color.clear, t);
        backSprite.color = color;
        spriteRenderer.color = color;
        if (t >= 1)//if the projectile is transparent, deactivate the projectile
        {
            gameObject.SetActive(false);
        }
    }
    //executes every physics tick
    void FixedUpdate()
    {
        for (int i = 0; i < enemiesTargeted.Length; i++)
        {
            //get (i)th entry on the list
            Transform enemy = enemiesTargeted[i];
            //if that entry actually has an enemy in it
            if (enemy != null)
            {
                //check if the enemy is over our DistForLosingTrackOfEnemy constant (7)
                if (Vector2.Distance(enemy.position, transform.position) > DistForLosingTrackOfEnemy)
                {
                    //remove the enemy from the list
                    enemiesTargeted[i] = null;
                }
                enemy.TryGetComponent(out Enemy enm);
                if (enm == null || enm.life <= 0)
                {
                    enemiesTargeted[i] = null;
                }
            }
        }
        AdjustLigthtningVisuals();
        //increase the timer by the interval in seconds from the last frame to the current one
        //so this makes the timer increase by 1 every second
        for (int i = 0; i < MaxTargets; i++)
        {
            hitTimers[i] += Time.fixedDeltaTime;
        }
        //use this to only execute the hit detection and attacking code at the rate we set SecondsPerHit
        //also check if the projectile duration hasn't expired (and therefore began fading out)
        if (lifetimeTimer <= TotalDuration)
        {
            DetectEnemies();
            DamageTargetedEnemies();
        }
        Vector2 dirToTarget = Vector2.zero;
        for (int i = 0; i < MaxTargets; i++)
        {
            Transform target = enemiesTargeted[i];
            if (target == null)
            {
                continue;
            }
            dirToTarget += (Vector2)(target.position - transform.position).normalized;
        }
        if (dirToTarget.sqrMagnitude > 0.1f)
        {
            rb.velocity = Helper.Decay(rb.velocity, dirToTarget * MaxVel, 2f);
        }
        CapTimers();
    }

    private void CapTimers()
    {
        for (int i = 0; i < MaxTargets; i++)
        {
            if (hitTimers[i] > SecondsPerHit)
            {
                hitTimers[i] = SecondsPerHit;
            }
        }
    }
    void AdjustLigthtningVisuals()
    {
        for (int i = 0; i < lightningRendererPool.Length; i++)
        {
            Transform enemy = enemiesTargeted[i];
            if (enemy != null)
            {
                lightningRendererPool[i].Move(transform.position, enemy.position);
            }
            else
            {
                lightningRendererPool[i].Stop();
            }
        }
    }
    private void DamageTargetedEnemies()
    {
        for (int i = 0; i < enemiesTargeted.Length; i++)
        {
            //get the i th entry of the list
            Transform enemy = enemiesTargeted[i];
            //if this entry actually has an enemy in it
            if (enemy != null && hitTimers[i] >= SecondsPerHit)
            {
                //based on the transform component of the enemy, try to get the component that
                //handles being damaged
                if (enemy.TryGetComponent(out IDamageable enemyDamageableComponent))
                {
                    //if we successfully get it, then deal damage to the enemy
                    enemyDamageableComponent.Damage(Damage);
                    lightningRendererPool[i].ActivateAndSetAttributes(0.1f, transform.position, enemy.position, LightningVisualDuration);
                }
                hitTimers[i] -= SecondsPerHit;
            }
        }

    }
    private void DetectEnemies()
    {
        GetOverlapCircleParams(out Vector2 center, out float radius);
        //this detects all enemy hitboxes within a specific radius
        //the function gives us a list of all the enemy hitboxes detected
        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(center, radius, Layers.Enemy);
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            //for every enemy hitbox in the hitboxes detected list
            Collider2D enemyHitbox = enemyColliders[i];
            if (enemyHitbox.TryGetComponent(out Enemy enemy))
            {
                if (enemy.life <= 0)
                {
                    continue;
                }
            }
            else
            {
                continue;
            }
            //check if it isn't already in the targeted enemies list
            //also let's see which slot of the list we can put the enemy detected without overriding anything
            int freeIndex = -1;
            for (int j = 0; j < enemiesTargeted.Length; j++)
            {
                if (enemiesTargeted[j] == enemyHitbox.transform)
                {
                    //this enemy detected is a repeat. so we shouldn't use the index and check for the next enemy detected
                    freeIndex = -1;
                    break;
                }
                if (enemiesTargeted[j] == null)
                {
                    freeIndex = j;
                }
            }
            //it's not a repeat, and we have a slot available in our enemies targeted list
            if (freeIndex != -1)
            {
                enemiesTargeted[i] = enemyHitbox.transform;
            }
        }
    }
    void GetOverlapCircleParams(out Vector2 center, out float radius)
    {
        center = transform.position;
        radius = DistForDetectingEnemy;
    }


#if UNITY_EDITOR
    //for displaying stuff like developer indicators
    //we'll use it to display the detection area for the attacking
    private void OnDrawGizmos()
    {
        GetOverlapCircleParams(out Vector2 center, out float radius);
        Gizmos2.DrawHDWireCircle(radius, center);
    }
#endif
}

