using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannonball_SCRIPT : Projectile
{
    public ParticleSystem destructionParticles;
    public override int Damage => 4;
    
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (destructionParticles != null)
        {
            Instantiate(destructionParticles, transform.position, Quaternion.identity);
        }
    }
}
