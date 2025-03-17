using Assets.Common.Consts;
using Assets.Helpers;
using Assets.Systems;
using UnityEngine;

public class FallingSpike : Projectile
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] new Collider2D collider;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] AudioSource audioSOurce;
    Vector2 originalPos;
    public override int Damage => 4;
    short state;
    float timer;
    const short StateIDHang = 0;
    const short StateIDFall = 1;
    const short StateIDRespawning = 2;
    const float TelegraphDuration = .3f;
    const float GravScale = 1.5f;
    const float TelegraphShakeStrength = .4f;
    const float MaxFallDuration = 3;
    const float RespawnDuration = .3f;
    public new Transform transform;
    bool dontRespawn;
    bool dontDestroy;
    bool dontPlaySound;
    public void Start()
    {
        transform = base.transform;
        //snap to tile grid(y is raised a bit to make it look like it's connected to the ceiling
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x - .5f) + 0.5f;
        pos.y = Mathf.Round(pos.y - .5f) + 0.54f;
        transform.position = pos;
        originalPos = transform.position;
    }
    void Update()
    {
        Vector2 spikePos = transform.position;
        switch (state)
        {
            case StateIDHang:
                Vector2 playerPos = GameManager.PlayerPosition;
                if (spikePos.y > playerPos.y && Mathf.Abs(spikePos.x - playerPos.x) < .6f)
                {
                    state = StateIDFall;
                    if (!dontPlaySound)
                    {
                        CommonSounds.PlayRandom(SpikeStageSingleton.instance.spikeBreak, audioSOurce);
                    }
                    timer = 0;
                }
                break;
            case StateIDFall:
                if (timer > TelegraphDuration)
                {
                    rb.gravityScale = GravScale;
                    if (originalPos.y == spikePos.y)
                    {
                        transform.position = originalPos;
                    }
                }
                else
                {
                    float shakeStrengthFade = Helper.Remap(timer, 0, TelegraphDuration, 1, .2f);
                    spikePos = originalPos;
                    spikePos.x += Random2.Float(-TelegraphShakeStrength, TelegraphShakeStrength) * shakeStrengthFade;
                    transform.position = spikePos;
                }
                if(timer > MaxFallDuration)
                {
                    state = StateIDRespawning;
                    timer = 0;
                    rb.velocity = Vector2.zero;
                    transform.localScale = Vector3.zero;
                    rb.gravityScale = 0;
                }
                break;
            case StateIDRespawning:
                if (timer <= RespawnDuration)
                {
                    Vector3 posToSet = originalPos;
                    float scale = Easings.SqrInOut(Mathf.InverseLerp(0, RespawnDuration, timer));
                    transform.localScale = new Vector3(scale, scale, scale);
                    posToSet.y += 0.5f - (scale * .5f);//offset it a bit so it looks like its growing out of the ceiling
                    transform.position = posToSet;
                }
                else
                {
                    transform.position = originalPos;
                    transform.localScale = Vector3.one;
                    playerPos = GameManager.PlayerPosition;
                    if (spikePos.y > playerPos.y && Mathf.Abs(spikePos.x - playerPos.x) < .6f)
                    {
                        state = StateIDFall;
                        CommonSounds.PlayRandom(SpikeStageSingleton.instance.spikeBreak, audioSOurce);
                        timer = 0;
                        break;
                    }
                    state = StateIDHang;
                    timer = 0;
                }
                break;
        }
        timer += Time.deltaTime;
    }
    public void StartFallAndMakeNotRespawnAndNotDestroy(float fallDelay = 0.3f, bool dontPlaySound = false)
    {

        state = StateIDFall;
        if (!dontPlaySound)
        {
            CommonSounds.PlayRandom(SpikeStageSingleton.instance.spikeBreak, audioSOurce);
        }
        timer = TelegraphDuration - fallDelay;
        dontRespawn = true;
        dontDestroy = true;
        enabled = true;
        sprite.enabled = true;
        rb.isKinematic = false;
        collider.enabled = true;
        rb.velocity = Vector2.zero;
        transform.localScale = Vector3.one;
        this.dontPlaySound = dontPlaySound;
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (state == StateIDFall && timer > (TelegraphDuration + .1f) && collision.gameObject.CompareTag(Tags.Tiles))
        {

            if (!dontPlaySound)
            {
                CommonSounds.PlayRandom(SpikeStageSingleton.instance.spikeBreak, audioSOurce);
            }
            //CommonSounds.Play(SpikeStageSingleton.instance.spikeBreakNew, audioSOurce, .5f, Random2.Float(.9f, 1.1f));
            EffectsHandler.SpawnSmallExplosion(FlipnoteStudioColors.ColorID.Yellow, transform.position);
            if (dontRespawn)
            {
                if (!dontDestroy)
                {
                    Destroy(gameObject, 1);//let the sound effect play out
                }
                enabled = false;
                sprite.enabled = false;
                rb.isKinematic = true;
                collider.enabled = false;
                rb.velocity = Vector2.zero;
                transform.localScale = Vector3.zero;
                state = StateIDRespawning;
                rb.gravityScale = 0;
                timer = 0;
            }
            else
            {
                rb.velocity = Vector2.zero; 
                transform.localScale = Vector3.zero;
                state = StateIDRespawning;
                rb.gravityScale = 0;
                timer = 0;
            }
            ScreenShakeManager.AddSmallShake(transform.position, ScreenShakeManager.ShakeGroupIDFallingSpike);
        }
    }
}
