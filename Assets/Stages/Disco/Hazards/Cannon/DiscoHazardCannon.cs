using Assets.Common.Consts;
using Assets.Helpers;
using System;
using UnityEngine;

public class DiscoHazardCannon : MonoBehaviour, IMusicSyncable
{
    // Referência ao GameObject que será disparado como projétil
    public GameObject projectilePrefab;

    // Ponto de onde o projétil será disparado
    public GameObject firePoint;

    public float ProjectileSpeed = 6f;

    public float maxDistanceToFire = 10f;

    [SerializeField] DiscoHazardCannonCannonball[] ammoPool;
    [SerializeField] CannonBarrelRotationScript barrelRotation;
    public int BeatsPerAction => 2;
    public int BeatOffset => 0;
    private void Start()
    {
        DiscoMusicEventManager.AddSyncableObject(this);
    }

    void FireProjectile(Vector2 projDirection)
    {
        ScreenShakeManager.AddSmallShake(transform.position, 2);
        DiscoHazardCannonCannonball projectile;
        Vector2 firepoint = firePoint.transform.position;
        if (Helper.TryFindFreeIndex(ammoPool, out int index))
        {
            projectile = ammoPool[index];
            projectile.transform.SetPositionAndRotation(firepoint, Quaternion.identity);
            projectile.gameObject.SetActive(true);
        }
        else
        {
            projectile = Instantiate(projectilePrefab, firepoint, Quaternion.identity)
                .GetComponent<DiscoHazardCannonCannonball>();
            Array.Resize(ref ammoPool, ammoPool.Length + 1);
            ammoPool[^1] = projectile;
        }
        EffectsHandler.SpawnSmallExplosion(FlipnoteColors.ColorID.Magenta, firepoint);
        projectile.rb.velocity = projDirection * ProjectileSpeed;
        barrelRotation.recoilTimer = 0f;
    }

    public void DoMusicSyncedAction()
    {
        Vector2 firePoint = this.firePoint.transform.position;
        Vector2 target = GameManager.PlayerPosition;
        Vector2 direction = (target - firePoint).normalized;
        float zRot = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        direction = direction.RotatedBy(-zRot);
        direction = LimitDirection(direction, default, 0);
        direction = direction.RotatedBy(zRot);
        float colliderRadius = ammoPool[0].collider.radius;
        RaycastHit2D hit = Physics2D.CircleCast(firePoint, colliderRadius, direction, maxDistanceToFire - colliderRadius, Layers.Tiles | Layers.PlayerHurtbox);
        if (hit.collider != null && hit.collider.CompareTag(Tags.Player))
        {
            FireProjectile(direction);
        }

    }
    /// <summary>
    /// expects direction to be normalized
    /// </summary>
    static Vector2 LimitDirection(Vector2 directionToLimit, Vector2 rangeCenter, float range)
    {
        if (directionToLimit.y < 0)
        {
            return new Vector2(Mathf.Sign(directionToLimit.x), 0);
        }
        return directionToLimit;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float radius = maxDistanceToFire;
        int dots = (int)(3 * radius);
        float increment = 1f / dots;
        Vector2 center = transform.position;
        float zRot = Mathf.Deg2Rad * -transform.rotation.eulerAngles.z;
        for (float i = 0; i <= 0.99999f; i += increment)
        {
            Vector2 offset = (i * Mathf.PI - Mathf.PI / 2f + zRot).PolarVector_Old(radius);
            Vector2 nextOffset = ((i + increment) * Mathf.PI - Mathf.PI / 2f + zRot).PolarVector_Old(radius);

            offset += center;
            nextOffset += center;
            Gizmos.DrawLine(nextOffset, offset);
        }
        Vector2 aaa = (Mathf.PI / 2 + zRot).PolarVector_Old(radius);
        Gizmos.DrawLine(center - aaa, center + aaa);
    }
#endif
}
