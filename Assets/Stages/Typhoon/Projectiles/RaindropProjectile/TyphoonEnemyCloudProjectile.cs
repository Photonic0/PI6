using Assets.Common.Consts;
using UnityEngine;

public class TyphoonEnemyCloudProjectile : Projectile
{
    public override int Damage => 4;
    public Rigidbody2D rb;
    public float timer = 0;
    public new Transform transform;
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer > 3)
        {
            timer = 0;
            gameObject.SetActive(false);
        }
    }
    private void FixedUpdate()
    {
        if (Physics2D.OverlapCircle(transform.position, .1f, Layers.Tiles))
        {
            EffectsHandler.SpawnSmallExplosion(FlipnoteColors.ColorID.Blue, transform.position);
            gameObject.SetActive(false);
        }
    }
    public override void OnHit(GameObject objectHit)
    {
        gameObject.SetActive(false);
    }
    
}
