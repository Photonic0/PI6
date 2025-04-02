using Assets.Helpers;
using System;
using UnityEngine;

//Rigidbody velocity not working properly somehow?? wtf?
public class TyphoonEnemyTornado : Enemy
{
    public override int LifeMax => 8;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform parent;
    [SerializeField] TyphoonEnemyTornadoLightning[] lightningProjPool;
    [SerializeField] AudioClip[] thunderShoot;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Sprite[] telegraphSheet;
    [SerializeField] SpriteRenderer telegraphObj;
    int state;
    float timer;
    Vector2 initialPosition;
    Vector2 randomOffset;
    public const int StateIDIdle = 0;
    public const int StateIDAggrod = 1;
    public const float TopSpeed = 4f;
    public const float AggroRange = 6f;
    public const float ProjSpeed = 6f;
    public const float ProjFireDelay = 2f;
    public const float TelegraphDuration = 1.5f;
    public const float Acceleration = 10f;
    public override void Start()
    {
        base.Start();
        initialPosition = transform.position;
        randomOffset = Random2.Circular(2);
        state = StateIDIdle;
        TyphoonStageSingleton.AddToTornadoEnemyList(this);
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
        if (timer > 0.8f)
        {
            randomOffset = Random2.Circular(2);
            timer -= 0.8f;
        }
        if (Helper.EnemyAggroCheck(transform.position, GameManager.PlayerControl.Position, AggroRange))
        {
            state = StateIDAggrod;
            timer = -Time.deltaTime + ProjFireDelay - TelegraphDuration;
        }

    }

    private void State_Aggrod()
    {
        Vector3 plrPos = GameManager.PlayerControl.Position;
        plrPos += (transform.position - plrPos).normalized * 0.7f;
        AccelerateTowards(plrPos);
        int telegraphSpriteIndex = (int)Helper.Remap(timer, ProjFireDelay - TelegraphDuration, ProjFireDelay, telegraphSheet.Length - 1, -1, false);
        if (telegraphSpriteIndex < 0 || telegraphSpriteIndex >= telegraphSheet.Length)
        {
            telegraphObj.enabled = false;
        }
        else
        {
            telegraphObj.enabled = true;
            telegraphObj.sprite = telegraphSheet[telegraphSpriteIndex];
        }
        if (timer > ProjFireDelay)    
        {
            timer -= ProjFireDelay;
            Vector2 toPlayer = (plrPos - transform.position).normalized;
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
            CommonSounds.PlayRandom(thunderShoot, audioSource, 1f, 1f);
        }
    }
    void AccelerateTowards(Vector2 targetPos)
    {
        Vector2 center = transform.position;
        for (int i = 0; i < TyphoonStageSingleton.instance.tornadoEnemies.Count; i++)
        {
            TyphoonEnemyTornado otherTornado = TyphoonStageSingleton.instance.tornadoEnemies[i];
            Vector2 deltaPos = center - (Vector2)otherTornado.transform.position;
            targetPos += deltaPos.normalized * (3 - Mathf.Clamp(deltaPos.magnitude, 0, 3));
        }
        Vector2 toTarget = (targetPos - (Vector2)transform.position).normalized;
        Vector2 velocity = Helper.Decay(rb.velocity, toTarget * TopSpeed, Acceleration);
        //velocity = toTarget * MoveSpeed;
        rb.rotation = velocity.x * -6;
        rb.velocity = velocity;
    }
    private void OnDisable()
    {
        TyphoonStageSingleton.RemoveTornadoEnemyFromList(this);
    }
    private void OnDestroy()
    {
        TyphoonStageSingleton.RemoveTornadoEnemyFromList(this);
    }
    public override bool PreKill()
    {
        telegraphObj.gameObject.SetActive(false);
        CommonSounds.Play(TyphoonStageSingleton.instance.typhoonEnemyDeath, audioSource);
        TyphoonStageSingleton.RemoveTornadoEnemyFromList(this);
        EffectsHandler.SpawnSmallExplosion(Assets.Common.Consts.FlipnoteStudioColors.ColorID.Blue, transform.position, 0.25f);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Animator>().enabled = false;
        rb.simulated = false;
        enabled = false;
        Destroy(parent.gameObject, 1f);
        return base.PreKill();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rb.velocity);
    }
#endif
}
