using Assets.Common.Consts;
using UnityEngine;

public class BasicShot : Projectile
{
    const float MaxLifetime = .5f;
    public override int Damage { get 
        {
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Alpha1))
            {
                return 99999;
            }
#endif
            return 1;
        }
    }
    public Vector2 velocity;
    float lifetime = MaxLifetime;
    private void OnEnable()
    {
        lifetime = MaxLifetime;
    }
    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            gameObject.SetActive(false);
        }
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.CompareTag(Tags.Tiles) || collision.CompareTag(Tags.CharacterHostile))
        {
            EffectsHandler.SpawnSmallExplosion(FlipnoteColors.ColorID.Orange, transform.position);
            gameObject.SetActive(false);
        }
    }
}
