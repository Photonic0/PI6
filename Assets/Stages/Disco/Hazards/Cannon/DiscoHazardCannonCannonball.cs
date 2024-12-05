using UnityEngine;

public class DiscoHazardCannonCannonball : Projectile
{
    public Rigidbody2D rb;
    public new CircleCollider2D collider;
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
        EffectsHandler.SpawnSmallExplosion(Assets.Common.Consts.FlipnoteColors.ColorID.Magenta, transform.position);
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
