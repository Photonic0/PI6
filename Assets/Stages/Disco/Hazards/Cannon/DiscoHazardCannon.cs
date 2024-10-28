using Assets.Helpers;
using System;
using UnityEngine;

public class DiscoHazardCannon : MonoBehaviour, IMusicSyncable
{
    // Referência ao GameObject que será disparado como projétil
    public GameObject projectilePrefab;

    // Ponto de onde o projétil será disparado
    public GameObject firePoint;

    // Velocidade do projétil
    public float ProjectileSpeed = 6f;

    // Distância máxima permitida para disparar
    public float maxDistanceToFire = 10f;

    [SerializeField] DiscoHazardCannonCannonball[] ammoPool;
    public int BeatsPerAction => 2;
    private void Start()
    {
        DiscoMusicEventManager.AddSyncableObject(this);
    }

    void FireProjectile()
    {
        DiscoHazardCannonCannonball projectile;
        if (Helper.TryFindFreeIndex(ammoPool, out int index))
        {
            projectile = ammoPool[index];
            projectile.transform.position = firePoint.transform.position;
            projectile.transform.rotation = Quaternion.identity;
            projectile.gameObject.SetActive(true);
        }
        else
        {
            projectile = Instantiate(projectilePrefab, firePoint.transform.position, Quaternion.identity)
                .GetComponent<DiscoHazardCannonCannonball>();
            Array.Resize(ref ammoPool, ammoPool.Length + 1);
            ammoPool[^1] = projectile;
        }
        // Calcula a direção em que o projétil deve ser disparado
        Vector2 direction = (GameManager.PlayerControl.transform.position - firePoint.transform.position).normalized;
        projectile.rb.velocity = direction * ProjectileSpeed;
    }

    public void DoMusicSyncedAction()
    {
        
            float distanceToTarget = Vector2.Distance(firePoint.transform.position, GameManager.PlayerControl.transform.position);

            // Verifica se a distância até o alvo é menor ou igual à distância máxima permitida
            if (distanceToTarget <= maxDistanceToFire)
            {
                FireProjectile();
            }
        
    }
}
