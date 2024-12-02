using Assets.Common.Interfaces;
using UnityEngine;

/// <summary>
/// all projectile collision checks should be triggers.
/// </summary>
public abstract class Projectile : MonoBehaviour
{
    public abstract int Damage { get; }
    public virtual void OnHit(GameObject objectHit)
    {

    }
    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable damageable))
        {
            OnHit(collision.gameObject);
            damageable.Damage(Damage);
        }
        
    }
}
