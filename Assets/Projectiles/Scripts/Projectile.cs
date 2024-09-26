using Assets.Common.Interfaces;
using UnityEngine;

/// <summary>
/// all projectile collision checks should be triggers.
/// </summary>
public abstract class Projectile : MonoBehaviour
{
    public abstract int Damage { get; }
    public virtual void OnHit(IDamageable damageable)
    {

    }
    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable damageable))
        {
            OnHit(damageable);
            damageable.Damage(Damage);
        }
    }
}
