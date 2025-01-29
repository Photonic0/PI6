using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class ExplosiveHazardTurret : MonoBehaviour
{
    [SerializeField] new Transform transform; // will be assigned in inspector
    [SerializeField] ExplosiveHazardTurretBullet[] bulletProjPool; // will be assigned in inspector
    [SerializeField] Transform turretHead;
    [SerializeField] Transform projOriginTransform;
    [SerializeField] Transform reticle;
    //[SerializeField] SpriteRenderer headSprite;
    float timer;
    const float FireRate = .1f;
    const int NumberOfShotsInARow = 5;
    const float ProjVelocity = 5f;
    const float AimDuration = 2f;
    const float SearchRadius = 6f;
    const float CooldownDuration = 1f;
    int shotsLeftToFire;
    int state = 0;

    const int StateIDSearching = 0;
    const int StateIDFiring = 1;
    const int StateIDAim = 2;
    const int StateIDCooldown = 3;
    void Update()
    {
        switch (state)
        {
            case StateIDSearching:
                State_Searching();
                break;
            case StateIDFiring:
                State_Firing();
                break;
            case StateIDAim:
                State_Aim();
                break;
            case StateIDCooldown:
                State_Cooldown();
                break;
        }
    }

    private void State_Cooldown()
    {
        timer += Time.deltaTime;
        if (timer >= CooldownDuration)
        {
            timer = 0;
            state = StateIDSearching;
        }
    }

    private void State_Aim()
    {
        timer += Time.deltaTime;
        Vector2 playerPos = GameManager.PlayerPosition;
        Vector2 projOrigin = projOriginTransform.position;
        Vector2 shotDirection = (playerPos - projOrigin).normalized;
        float radius = bulletProjPool[0].hitbox.radius;
        RaycastHit2D hit = Physics2D.CircleCast(projOrigin, radius, shotDirection, SearchRadius, Layers.Tiles | Layers.PlayerHurtbox);
        if (hit.collider == null || hit.collider.CompareTag(Tags.Tiles))
        {
            timer = 0;
            state = StateIDSearching;
            reticle.localScale = Vector3.one;
            reticle.gameObject.SetActive(false);
            return;
        }
        RotateTurretHeadTowards(playerPos);
        float scale = Helper.Remap(timer, 0f, AimDuration, 3f, 1f);
        reticle.localScale = new Vector3(scale, scale, scale);
        Vector2 deltaPos = (playerPos - (Vector2)turretHead.position);
        Vector2 turretHeadApparentPos = transform.position;
        turretHeadApparentPos.y += .5f;
        float currentDist = (turretHeadApparentPos - (Vector2)reticle.position).magnitude;
        float dist = deltaPos.magnitude;
        dist = Helper.Decay(currentDist, dist, 30f);
        reticle.transform.position = (turretHead.rotation.eulerAngles.z * -Mathf.Deg2Rad - Mathf.PI * .5f).PolarVector_Old(dist) + turretHeadApparentPos;
        reticle.gameObject.SetActive(true);
        if (timer >= AimDuration)
        {
            timer -= AimDuration;
            shotsLeftToFire = NumberOfShotsInARow;
            state = StateIDFiring;
        }
    }

    private void State_Firing()
    {
        timer += Time.deltaTime;
        if (timer >= FireRate)
        {
            shotsLeftToFire--;
            timer -= FireRate;
            if (shotsLeftToFire >= 0)
            {
                if (Helper.TryFindFreeIndex(bulletProjPool, out int index))
                {
                    ExplosiveHazardTurretBullet bullet = bulletProjPool[index];
                    Vector2 projOrigin = projOriginTransform.position;
                    EffectsHandler.SpawnSmallExplosion(FlipnoteColors.DarkGreen, projOrigin);
                    Vector2 shootDirection = (turretHead.eulerAngles.z * -Mathf.Deg2Rad - Mathf.PI * .5f).PolarVector_Old();
                    bullet.transform.SetPositionAndRotation(projOrigin, shootDirection.ToRotation(90));
                    bullet.gameObject.SetActive(true);
                    shootDirection.Normalize();
                    ScreenShakeManager.AddDirectionalShake(shootDirection, ScreenShakeManager.TinyShakeMagnitude);
                    bullet.rb.velocity = shootDirection * ProjVelocity;
                }
            }
            else
            {
                timer = 0;
                state = StateIDCooldown;
                shotsLeftToFire = NumberOfShotsInARow;
            }
        }
    }

    private void State_Searching()
    {
        Vector2 playerPos = GameManager.PlayerPosition;
        Vector2 projOrigin = projOriginTransform.position;
        Vector2 shotDirection = (playerPos - projOrigin).normalized;
        float radius = bulletProjPool[0].hitbox.radius;
        RaycastHit2D hit = Physics2D.CircleCast(projOrigin, radius, shotDirection, SearchRadius, Layers.Tiles | Layers.PlayerHurtbox);
        if (hit.collider != null && hit.collider.CompareTag(Tags.Player))
        {
            timer = 0;
            state = StateIDAim;
            shotsLeftToFire = NumberOfShotsInARow;
        }
    }

    void RotateTurretHeadTowards(Vector2 target)
    {
        target.y -= .5f;
        Vector2 deltaPos = (target - (Vector2)transform.position);
        Vector2 direction = deltaPos.normalized;
#if UNITY_EDITOR
        debug_ArrowDirection = direction;
#endif
        float t = 1 - Mathf.Pow(0.000001f, Time.deltaTime);
        float xSign = Mathf.Sign(direction.x);
        if (direction.y < 0)
        {
            direction.x = xSign;
            direction.y = 0;
        }
        xSign = -xSign;
        turretHead.localScale = new Vector3(1, xSign, 1);
        float radians = Mathf.Atan2(direction.y, direction.x);
        //25.5f is the distance of the rotation center to the sprite pivot in pixels
        //50f is the pixels per unit of the sprite      
        turretHead.rotation = Quaternion.Lerp(turretHead.rotation, Quaternion.Euler(0, 0, radians * Mathf.Rad2Deg + 180), t);
        Vector2 offset = new Vector2(4f / 50f, xSign * 25.5f / 50f).RotatedBy(Mathf.Deg2Rad * (turretHead.eulerAngles.z - 180));
        offset.y += .5f;
        turretHead.localPosition = offset;
    }
#if UNITY_EDITOR
    [SerializeField] Vector2 debug_ArrowDirection;
    [SerializeField] float debug_turretHeadDecay;
    private void OnDrawGizmos()
    {
        Gizmos2.DrawHDWireCircle(SearchRadius, transform.position);
        Gizmos2.DrawArrow(transform.position, debug_ArrowDirection);
    }
#endif
}
