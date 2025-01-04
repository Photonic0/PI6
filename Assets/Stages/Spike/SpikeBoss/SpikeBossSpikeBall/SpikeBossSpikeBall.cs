using Assets.Common.Consts;
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
            if(timer > .2f)
            {
                Vector2 pos = transform.position;

                float radius = collider.radius;
                Collider2D tilesCollider = Physics2D.OverlapCircle(pos, radius, Layers.Tiles);
                if(tilesCollider != null)
                {
                    EffectsHandler.SpawnSmallExplosion(FlipnoteColors.ColorID.Yellow, pos);
                    gameObject.SetActive(false);
                    timer = 0;
                    DisablePhysics();
                    SpikeWaveSpike.StartSpikeWave(pos, 1f, 8, .25f, .1f, null);
                    return;
                }
            }
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
