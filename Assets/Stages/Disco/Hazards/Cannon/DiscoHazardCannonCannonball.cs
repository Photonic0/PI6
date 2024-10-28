using UnityEngine;

public class DiscoHazardCannonCannonball : Projectile
{
    public ParticleSystem destructionParticles;
    public Rigidbody2D rb;
    public override int Damage => 4;
    float lifetime;
    private void OnEnable()
    {
        lifetime = 5;
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);       
        gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        if (destructionParticles != null)
        {
            Instantiate(destructionParticles, transform.position, Quaternion.identity);
        }
    }
    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime < 0)
        {
            gameObject.SetActive(false); 
        }
    }
}
