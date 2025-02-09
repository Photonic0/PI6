using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class TyphoonHazardCloudPlatform : MonoBehaviour
{
    const int StateIDHarmless = 0;
    const int StateIDHarmful = 1;
    const float HarmlessDuration = 3f;
    const float HarmfulDuration = 0.4f;
    static readonly int animHarmless = Animator.StringToHash("CloudPlatformHarmless");
    static readonly int animHarmful = Animator.StringToHash("CloudPlatformHarmful");
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Animator animator;
    [SerializeField] new BoxCollider2D collider;
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
        if (timer > HarmlessDuration - 1)
        {
            sprite.color = Helper.Remap(timer, HarmlessDuration - 1, HarmlessDuration, Color.white, Color.black);
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

        if (Input.GetKey(KeyCode.S))
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
}
