using UnityEngine;

public class TyphoonEnemyCloudProjectile : Projectile
{
    public override int Damage => 4;
    public Rigidbody2D rb;
    public float timer = 0;
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer > 3)
        {
            timer = 0;
            gameObject.SetActive(false);
        }
    }
    public override void OnHit(GameObject objectHit)
    {
        gameObject.SetActive(false);
    }
}
