
using Assets.Common.Consts;
using UnityEngine;

public class SpikeShot : Projectile
{
    public override int Damage => 7;
    [SerializeField] new Transform transform;
    
    public Rigidbody2D rb;
    float lifetime;
    private void OnEnable()
    {
        lifetime = 1;
    }
    private void FixedUpdate()
    {
        lifetime -= Time.fixedDeltaTime;
        if (lifetime < 0 || Physics2D.OverlapCircle(rb.position, 0.5f, Layers.Tiles))
        {
            EffectsHandler.SpawnSmallExplosion(FlipnoteColors.Yellow, transform.position);
            gameObject.SetActive(false);
        }
        transform.Rotate(new Vector3(0, 0, Time.fixedDeltaTime * 720));
    }
    public override void OnHit(GameObject objectHit)
    {
        base.OnHit(objectHit);
        if(objectHit.TryGetComponent<Enemy>(out _))
        {
            EffectsHandler.SpawnSmallExplosion(FlipnoteColors.ColorID.Yellow, transform.position);
            gameObject.SetActive(false);
        }
    }
}
