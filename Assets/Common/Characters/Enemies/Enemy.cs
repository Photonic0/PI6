using Assets.Common.Interfaces;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    public float LifePercent => (float)life / LifeMax;
    public abstract int LifeMax { get; }
    public int life;
    public virtual void Start()
    {
        life = LifeMax;
    }
    public void Heal(int life)
    {
        this.life += life;
        if (this.life > LifeMax)
        {
            this.life = LifeMax;
        }
    }
    public virtual bool PreKill()
    {
        return true;
    }
    public virtual void OnHit(int damageTaken)
    {

    }
    public void Damage(int damage)
    {
        life -= damage;
        if (life > LifeMax)
        {
            life = LifeMax;
        }
        OnHit(damage);
        if(life <= 0)
        {
            if (PreKill())
            {
               Destroy(gameObject);
            }
        }
    }
}
