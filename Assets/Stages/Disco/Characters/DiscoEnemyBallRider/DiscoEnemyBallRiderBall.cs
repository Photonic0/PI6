using Assets.Common.Consts;
using UnityEngine;

public class DiscoEnemyBallRiderBall : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] new CircleCollider2D collider;
    [SerializeField] new Transform transform;
    [SerializeField] AudioSource audioSource;
    [SerializeField] SpriteRenderer ballSprite;
    [SerializeField] GameObject outlineGameObj;
    private void FixedUpdate()
    {
        if (ballSprite.enabled)
        {
            transform.Rotate(0, 0, rb.velocity.x * -Mathf.Rad2Deg * Time.fixedDeltaTime);

            bool willBeDestroyed = false;
            Collider2D overlappedCollider = Physics2D.OverlapCircle(transform.position, collider.radius, Layers.PlayerHurtbox);
            if (overlappedCollider != null)
            {
                GameManager.PlayerLife.Damage(4);
                DestroyBall();
                willBeDestroyed = true;
            }
            if (!willBeDestroyed)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(Mathf.Sign(rb.velocity.x), 0), collider.radius + 0.05f, Layers.Tiles);
                if (hit.collider != null)
                {
                    DestroyBall();
                }
            }
        }
        if (rb.velocity.y < -50)//fell out of bounds
        {
            Destroy(transform.root.gameObject);
        }

    }
    private void DestroyBall()
    {
        EffectsHandler.SpawnMediumExplosion(FlipnoteColors.Magenta, transform.position);
        CommonSounds.PlayBwow(audioSource);
        ballSprite.enabled = false;
        outlineGameObj.SetActive(false);
        Destroy(transform.root.gameObject, 1f);
        enabled = false;
        collider.enabled = false;
        rb.simulated = false;
    }
    public void Release(Vector2 releaseVel)
    {
        if(releaseVel.sqrMagnitude < .2f)
        {
            releaseVel = (GameManager.PlayerPosition - transform.position).normalized * 4.5f;
        }
        releaseVel *= 1.5f;
        releaseVel.y += 3;
        rb.simulated = true;
        rb.velocity = releaseVel;
        enabled = true;
        collider.enabled = true;
    }
}
