using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class TyphoonHazardCloudPlatform : MonoBehaviour
{
    const int StateIDHarmless = 0;
    const int StateIDHarmful = 1;
    const float HarmlessDuration = 3f;
    const float HarmfulDuration = 0.4f;
    const float TelegraphDuration = 2f;
    static readonly int animHarmless = Animator.StringToHash("CloudPlatformHarmless");
    static readonly int animHarmful = Animator.StringToHash("CloudPlatformHarmful");
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Animator animator;
    [SerializeField] new BoxCollider2D collider;
    [SerializeField] ParticleSystem particles;
    float timer = 0;
    int state;

    private void Start()
    {
        float posX = (transform.position.x * Helper.Phi) % HarmlessDuration;
        timer = posX;
    }
    void Update()
    {

        switch (state)
        {
            case StateIDHarmful:
                State_Harmful();
                break;
            default://StateIDHarmless
                State_Harmless();
                break;
        }
        UpdateCollider();
        timer += Time.deltaTime;
    }
    private void State_Harmful()
    {
        if (timer > HarmfulDuration)
        {
            state = StateIDHarmless;
            animator.CrossFade(animHarmless, 0);
            timer %= HarmlessDuration;
        }
    }
    private void State_Harmless()
    {
        //flash as warning
        if (timer > HarmlessDuration - TelegraphDuration)
        {
            LightningParticles();
            //sprite.color = Helper.Remap(timer, HarmlessDuration - 1, HarmlessDuration, Color.white, Color.black);
        }
        if (timer > HarmlessDuration)
        {
            sprite.color = Color.white;
            timer %= HarmfulDuration;
            animator.CrossFade(animHarmful, 0);
            state = StateIDHarmful;
        }
    }
    void UpdateCollider()
    {
        if (state == StateIDHarmful)
        {
            collider.enabled = true;
            return;
        }


        if (GameManager.PlayerControl.CanInput && Input.GetKey(KeyCode.S))
        {
            collider.enabled = false;
            return;
        }

        Vector2 playerBottom = GameManager.PlayerControl.transform.position;
        playerBottom.y -= 1;//the actual bottom of the player collider
        Vector2 colliderTop = transform.position;
        colliderTop.y += 0.2195106f;
        colliderTop.y += 0.6481868f * .5f;
        collider.enabled = playerBottom.y >= colliderTop.y;

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForDamage(collision);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckForDamage(collision);
    }
    private void CheckForDamage(Collision2D collision)
    {
        if (state == StateIDHarmful && collision.gameObject.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(5);
        }
    }
    void LightningParticles()
    {
        const float TimerOffset = .2f;
        float chancePer120thSec = .5f;
        Vector2 particlePos = particles.transform.position;
        Vector2 center = transform.position;
        center -= particlePos;
        float timer = this.timer - TimerOffset;
        float timeLeftUntilActivation = HarmlessDuration - timer - TimerOffset;
        float sizeIncrease = Helper.Remap(timer, HarmlessDuration - TelegraphDuration, HarmlessDuration, 0.1f, 0.35f);
        float ringDist = Helper.Remap(timer, HarmlessDuration - TelegraphDuration, HarmlessDuration, 2f, 0f);
        if (Random2.Percent(chancePer120thSec, 120))
        {
            ParticleSystem.EmitParams emitParams = new();
            Vector2 deltaPos = -center;
            float lifetime = Random2.Float(.2f, .45f);
            Vector2 targetPos = center + Random2.Circular(1f);

            Vector2 spawnPos = center + Random2.Ring(ringDist, ringDist + .5f);
            emitParams.position = spawnPos;
            emitParams.velocity = (targetPos - spawnPos) / lifetime + Random2.Circular(.5f);
            emitParams.startLifetime = Mathf.Clamp(timeLeftUntilActivation, 0.01f, lifetime);
            emitParams.startColor = FlipnoteColors.Yellow;
            emitParams.startSize = Random2.Float(.01f, .11f) + sizeIncrease;
            particles.Emit(emitParams, 1);
        }
    }
}
