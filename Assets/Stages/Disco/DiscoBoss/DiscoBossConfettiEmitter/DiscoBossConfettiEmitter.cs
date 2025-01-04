using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class DiscoBossConfettiEmitter : MonoBehaviour
{
    [SerializeField] new Transform transform;
    [SerializeField] ParticleSystem particles;
    [SerializeField] float height;
    [SerializeField] float width;
    [SerializeField] float timer;
    [SerializeField] bool checkedExplosionCollision;
    [SerializeField] float timeUntilExplosion;
    [SerializeField] Vector3 startingPosition;
    [SerializeField] float maxRiseTime;
    [SerializeField] float maxWaitForExplosionTime;
    [SerializeField] float maxExplodeTime;
    [SerializeField] float maxGoDownTime;
    [SerializeField] float maxWaitForRiseTime;
    [SerializeField] Vector2 positionOffset;
    byte state;
    const byte StateIDRest = 0;
    const byte StateIDRise = 1;
    const byte StateIDWaitingForExplosion = 2;
    const byte StateIDExplode = 3;
    const byte StateIDGoDown = 4;
    const byte StateIDWaitingForRise = 5;

    [Header("debug fields")]
    [SerializeField] bool debug_TriggerExplosion;
    [SerializeField] int debug_particleCount;
    private void Start()
    {
        Vector2 pos = transform.position;
        pos.x = Mathf.Floor(pos.x);
        pos.y = Mathf.Floor(pos.y);
        pos.x += .5f;
        pos.x += positionOffset.x;
        pos.y += positionOffset.y;

        //pos.y += .5f;
        //snaps to bottom middle of tile
        startingPosition.Set(pos.x, pos.y, 0);
        transform.position = pos;
        state = StateIDRest;
    }
    void Update()
    {
#if UNITY_EDITOR

        if (debug_TriggerExplosion)
        {
            ConfettiExplosion(debug_particleCount);
            debug_TriggerExplosion = false;
        }
#endif 
        switch (state)
        {
            case StateIDRise:
                timer += Time.deltaTime;
                transform.position = startingPosition + new Vector3(0, Helper.Remap(timer, 0, maxRiseTime, 0, .5f));
                if (timer > maxRiseTime)
                {
                    if (maxWaitForExplosionTime <= 0)
                    {
                        checkedExplosionCollision = false;
                        ConfettiExplosion();
                        state = StateIDExplode;
                    }
                    else
                    {
                        state = StateIDWaitingForExplosion;
                    }
                    timer -= maxRiseTime;
                }
                break;
            case StateIDWaitingForExplosion:
                timer += Time.deltaTime;
                if(timer > maxWaitForExplosionTime)
                {
                    timer -= maxWaitForExplosionTime;
                    checkedExplosionCollision = false;
                    state = StateIDExplode;
                    ConfettiExplosion();
                }
                break;
            case StateIDExplode:
                timer += Time.deltaTime;
                float yOffset = Helper.Remap(timer,0, Mathf.Min(maxExplodeTime, 0.09f), Mathf.PI, Mathf.PI * 2);
                //add 0.5 from the rise
                transform.position = startingPosition + new Vector3(0, (float)Mathf.Sin(yOffset) * .3f +.5f);
                if (timer > 0.1f && !checkedExplosionCollision)
                {
                    CheckExplosionCollision();
                }
                if (timer > maxExplodeTime)
                {
                    if (!checkedExplosionCollision)
                    {
                        CheckExplosionCollision();
                    }
                    timer -= maxExplodeTime;
                    state = StateIDGoDown;
                }
                break;
            case StateIDGoDown:
                timer += Time.deltaTime;
                transform.position = startingPosition + new Vector3(0, Helper.Remap(timer, 0, maxGoDownTime, .5f, 0));
                if (timer > maxGoDownTime)
                {
                    timer -= maxGoDownTime;
                    state = StateIDRest;
                }
                break;
            case StateIDWaitingForRise:
                timer += Time.deltaTime;
                if (timer > maxWaitForRiseTime)
                {
                    timer -= maxWaitForRiseTime;
                    state = StateIDRise;
                }
                break;
            default://case StateIDRest
                break;
        }
    }

    private void CheckExplosionCollision()
    { 
        Vector2 center = transform.position;
        center.y += height / 2;
        Collider2D player = Physics2D.OverlapBox(center, new Vector2(width, height), 0, Layers.PlayerHurtbox);
        if (player != null)
        {
            GameManager.PlayerLife.Damage(DiscoBossAI.ConfettiDamage);
        }
        checkedExplosionCollision = true;
    }

    public void StartAnimation(float timeSpentRising, float timeSpentWaitingForExplosion, float timeSpentWaitingAfterExplosion, float timeSpentGoingDown, float timeSpentBeforeRising = 0)
    {
        //if was about to explode, trigger explosion
        if (state == StateIDWaitingForExplosion/* && timer + Time.deltaTime >= maxWaitForExplosionTime*/)
        {
            CheckExplosionCollision();
             ConfettiExplosion();
        }
        maxRiseTime = timeSpentRising;
        maxWaitForExplosionTime = timeSpentWaitingForExplosion;
        maxExplodeTime = timeSpentWaitingAfterExplosion;
        maxGoDownTime = timeSpentGoingDown;
        maxWaitForRiseTime = timeSpentBeforeRising;
        timer = 0;
        //if (state == StateIDRest)
        //{
        state = timeSpentBeforeRising <= 0 ? StateIDRise : StateIDWaitingForRise;
        //}
        //else
        //{
        //    switch (state)
        //    {
        //        case StateIDExplode:
        //            timer -= maxExplodeTime;
        //            timer -= maxWaitForExplosionTime;
        //            timer -= maxWaitForRiseTime;
        //            timer -= maxRiseTime;
        //            break;
        //        default:
        //            break;
        //    }
        //}
       
    }
    void ConfettiExplosion(int particleCount = 50)
    {
        ScreenShakeManager.AddTinyShake(transform.position, 1);
        Vector3 origin = transform.position - particles.transform.position;
        float halfWidth = width / 2;
        //float minYVel = grav * grav * .5f;
        //make particles do a bouncy scale?
        //loop and not using count parameter in Emit because needs to be randomized a bunch of times
        for (int i = 0; i < particleCount; i++)
        {
            float lifetime = Random2.Float(0.7f, 0.8f);
            ParticleSystem.EmitParams emitParams = new();
            emitParams.position = origin + new Vector3(Random2.Float(-.4f, .4f), 0);
            emitParams.startSize = Random2.Float(.1f, .2f);
            emitParams.velocity = new Vector3(Random2.Float(-halfWidth, halfWidth) * 4.8f, Random2.Float(height * 6));
            emitParams.startLifetime = lifetime;
            particles.Emit(emitParams, 1);
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        center.y += height / 2;
        Gizmos.DrawWireCube(center, new Vector3(width, height, .1f));
    }
#endif
}
