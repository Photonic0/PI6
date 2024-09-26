using Assets.Common.Consts;
using Assets.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BasicShot : Projectile
{
    const float MaxLifetime = .5f;
    public override int Damage => 1;
    public Vector2 velocity;
    float lifetime = MaxLifetime;
    private void OnEnable()
    {
        lifetime = MaxLifetime;
    }
    private void Update()
    {
        lifetime -= Time.deltaTime;
        if(lifetime <= 0)
        {
            Destroy(gameObject);
        }
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.CompareTag(Tags.Tiles) || collision.gameObject.layer == Layers.Enemy_FriendlyProj)
        {
            
            Destroy(gameObject);
        }
    }
}
