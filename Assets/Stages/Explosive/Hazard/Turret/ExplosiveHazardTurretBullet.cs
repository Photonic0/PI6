using UnityEngine;

public class ExplosiveHazardTurretBullet : Projectile
{
    public override int Damage => 4;
    public Rigidbody2D rb;
    public CircleCollider2D hitbox;
    float timer;
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer > 4)
        {
            gameObject.SetActive(false);
            EffectsHandler.SpawnSmallExplosion(Assets.Common.Consts.FlipnoteStudioColors.ColorID.DarkGreen, transform.position);
        }
    }
    private void OnDisable()
    {
        timer = 0;
    }
    public override void OnHit(GameObject objectHit)
    {
        gameObject.SetActive(false);
        EffectsHandler.SpawnSmallExplosion(Assets.Common.Consts.FlipnoteStudioColors.ColorID.DarkGreen, transform.position);
    }
}
