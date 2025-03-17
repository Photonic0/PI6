using Assets.Common.Consts;
using UnityEngine;

public class SpikeBall : Projectile
{
    public override int Damage => 4;
    [SerializeField] new Collider2D collider;
    public Rigidbody2D rb;
    float timer = 0;
    private void OnEnable()
    {
        timer = 0;
    }
    public void DisablePhysics()
    {
        collider.enabled = false;
        rb.isKinematic = true;
    }
    public void EnablePhysics()
    {
        collider.enabled = true;
        rb.isKinematic = false;
    }
    private void Update()
    {
        float rotateAmount = Time.deltaTime * rb.velocity.x * -Mathf.Rad2Deg;
        transform.Rotate(0, 0, rotateAmount);
        if(timer > 5)
        {
            DisablePhysics();
            gameObject.SetActive(false);
            timer = 0;
        }
        timer += Time.deltaTime;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //doing this because projectiles are usually triggers
        //but this projectile is an exception
        if (collision.gameObject.CompareTag(Tags.Player))
        {
            base.OnTriggerEnter2D(collision.collider);
            gameObject.SetActive(false);
            EffectsHandler.SpawnSmallExplosion(FlipnoteStudioColors.ColorID.Yellow, transform.position);
        }
        if (Mathf.Abs(rb.velocity.x) < 1f)
        {
            DisablePhysics();
            gameObject.SetActive(false);
        }
    }
}
