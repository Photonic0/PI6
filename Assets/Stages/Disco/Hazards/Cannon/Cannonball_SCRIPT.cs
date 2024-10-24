using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannonball_SCRIPT : MonoBehaviour
{
    public ParticleSystem destructionParticles;
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHit();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void PlayerHit()
    {
        print("Player atingido");
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
