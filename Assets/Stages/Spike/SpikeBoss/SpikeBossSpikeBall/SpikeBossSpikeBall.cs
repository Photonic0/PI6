using UnityEngine;

public class SpikeBossSpikeBall : Projectile
{
    public new Transform transform;
    [SerializeField] new CircleCollider2D collider;
    public Rigidbody2D rb;
    float timer = 0;
    public override int Damage => 2;
    private void FixedUpdate()
    {
        if (!rb.isKinematic)
        {
            timer += Time.fixedDeltaTime;
            if (timer > 5)
            {
                gameObject.SetActive(false);
                timer = 0;
            }
            transform.Rotate(0, 0, Time.fixedDeltaTime * 720);
        }
    }
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

}
