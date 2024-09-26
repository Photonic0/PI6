using Assets.Helpers;
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
    const float TelegraphDuration = .3f;
    const float GravScale = 1.5f;
    const float TelegraphShakeStrength = .4f;
    new Transform transform;
    void Start()
    {
        transform = base.transform;
        //snap to tile grid(y is raised a bit to make it look like it's connected to the ceiling
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x - .5f) + 0.5f; 
        pos.y = Mathf.Round(pos.y - .5f) + 0.54f;
        transform.position = pos;
        timer = 0;
        state = 0;
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
                    CommonSounds.PlayRandom(SpikeStageSingleton.instance.spikeBreak, audioSOurce);
                    timer = 0;
                }
                break;
            case StateIDFall:   
                if (timer > TelegraphDuration)
                {
                    rb.gravityScale = GravScale;
                    if(originalPos.y == spikePos.y)
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
                break;
        }
        timer += Time.deltaTime;
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(state == StateIDFall && timer > (TelegraphDuration + .1f) && collision is CompositeCollider2D)
        {
            sprite.enabled = false;
            rb.isKinematic = true;
            collider.enabled = false;
            rb.velocity = Vector2.zero;
            CommonSounds.PlayRandom(SpikeStageSingleton.instance.spikeBreak, audioSOurce, .5f);
            Destroy(gameObject, 1);//let the sound effect play out
        }
    }
}
