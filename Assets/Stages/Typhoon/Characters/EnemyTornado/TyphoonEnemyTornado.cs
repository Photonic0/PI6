using Assets.Helpers;
using System;
using UnityEngine;

//Rigidbody velocity not working properly somehow?? wtf?
public class TyphoonEnemyTornado : Enemy
{
    public override int LifeMax => 10;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform parent;
    [SerializeField] TyphoonEnemyTornadoLightning[] lightningProjPool;
    [SerializeField] AudioClip[] thunderShoot;
    [SerializeField] AudioSource audioSource;
    int state;
    float timer;
    Vector2 initialPosition;
    Vector2 randomOffset;
    public const int StateIDIdle = 0;
    public const int StateIDAggrod = 1;
    public const float MoveSpeed = 4;
    public const float AggroRange = 6;
    public const float ProjSpeed = 6;
    public const float ProjFireDelay = 2;
    public override void Start()
    {
        base.Start();
        initialPosition = transform.position;
        randomOffset = Random2.Circular(2);
        state = StateIDIdle;
    }
    private void Update()
    {
        switch (state)
        {
            case StateIDIdle:
                State_Idle();
                break;
            case StateIDAggrod:
                State_Aggrod();
                break;
        }
        timer += Time.deltaTime;
    }

    private void State_Idle()
    {
        Vector2 targetPos = initialPosition + randomOffset;
        AccelerateTowards(targetPos);
        if (timer > 1)
        {
            randomOffset = Random2.Circular(2);
            timer -= 1;
        }
        if (Helper.EnemyAggroCheck(transform.position, GameManager.PlayerControl.Position, AggroRange))
        {
            state = StateIDAggrod;
            timer = -Time.deltaTime;
        }

    }

    private void State_Aggrod()
    {
        AccelerateTowards(GameManager.PlayerControl.Position);
        if (timer > ProjFireDelay)    
        {
            timer -= ProjFireDelay;
            Vector2 toPlayer = (GameManager.PlayerControl.Position - transform.position).normalized;
            if(!Helper.TryFindFreeIndex(lightningProjPool, out int index))
            {
                index = lightningProjPool.Length;
                Array.Resize(ref lightningProjPool, lightningProjPool.Length + 1);
                lightningProjPool[^1] = Instantiate(lightningProjPool[0], parent); 
            }
            TyphoonEnemyTornadoLightning proj = lightningProjPool[index];
            proj.gameObject.SetActive(true);
            proj.rb.velocity = toPlayer * ProjSpeed;
            proj.transform.position = transform.position;
            proj.rb.rotation = toPlayer.Atan2Deg();
            CommonSounds.PlayRandom(thunderShoot, audioSource);
        }
    }
    void AccelerateTowards(Vector2 targetPos)
    {
        Vector2 toTarget = (targetPos - (Vector2)transform.position).normalized;
        Vector2 velocity = Vector2.Lerp(rb.velocity, toTarget * MoveSpeed, Time.deltaTime);
        //velocity = toTarget * MoveSpeed;
        rb.rotation = velocity.x * -6;
        rb.velocity = velocity;
    }
    public override bool PreKill()
    {
        EffectsHandler.SpawnSmallExplosion(Assets.Common.Consts.FlipnoteColors.ColorID.Blue, transform.position, 0.25f);
        return base.PreKill();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rb.velocity);
    }
#endif
}
