using UnityEngine;

public class TyphoonEnemyTornadoLightning : Projectile
{
    public override int Damage => 4;
    public Rigidbody2D rb;
    float timer;
    const float MaxLifetime = 5;
    private void OnEnable()
    {
        timer = 0;
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer > MaxLifetime)
        {
            timer = 0;
            gameObject.SetActive(false);
        }
    }
    public void SetLifetime(float lifetime)
    {
        timer = MaxLifetime - lifetime;
    }
}
