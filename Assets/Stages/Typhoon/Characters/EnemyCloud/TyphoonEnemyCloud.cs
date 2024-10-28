using Assets.Helpers;
using System;
using UnityEngine;

public class TyphoonEnemyCloud : Enemy
{
    public override int LifeMax => 6;
    public const int StateIDIdle = 0;
    public const int StateIDChasingPlayer = 1;
    public const float AggroRange = 6;
    public const float MaxMoveSpeed = 5;
    public const float MinDistForRain = 2;
    public const float RainDelay = 0.25f;
    public const float RainFallSpeed = 7;
    [SerializeField] Transform parent;
    [SerializeField] TyphoonEnemyCloudProjectile[] projPool;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] ParticleSystem deathParticle;
    int state;
    float timer;
    public override void Start()
    {
        base.Start();
        state = 0;
        timer = 0;
    }


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
        }
    }

    private void State_Idle()
    {
        timer += Time.deltaTime;
        rb.velocity = new Vector2(0, Mathf.Cos(timer));
        if (Helper.EnemyAggroCheck(transform.position, GameManager.PlayerControl.Position, AggroRange))
        {
            state = StateIDChasingPlayer;
            timer = 0;
        }
    }

    private void State_ChasingPlayer()
    {
        if(timer < RainDelay)
        {
            timer += Time.deltaTime;
        }
        Vector2 playerPos = GameManager.PlayerControl.Position;
        Vector2 toTargetPos = (playerPos + new Vector2(0, 2.5f) - rb.position).normalized;
        rb.velocity = Vector2.Lerp(rb.velocity, toTargetPos * MaxMoveSpeed, Time.deltaTime * 3);
        if(playerPos.y < transform.position.y && Mathf.Abs(playerPos.x - transform.position.x) < MinDistForRain && timer >= RainDelay)
        {
            timer -= RainDelay;
            if(!Helper.TryFindFreeIndex(projPool, out int index))
            {
                index = projPool.Length;
                Array.Resize(ref projPool, projPool.Length + 1);
                projPool[^1] = Instantiate(projPool[0], parent);
            }
            TyphoonEnemyCloudProjectile proj = projPool[index];
            proj.gameObject.SetActive(true);
            proj.transform.position = transform.position + new Vector3(Random2.Float(-0.6f, 0.6f), -1);
            proj.rb.velocity = new Vector2(0, -RainFallSpeed);
        }
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
