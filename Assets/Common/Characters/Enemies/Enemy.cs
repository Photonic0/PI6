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
    /// <summary>
    /// return false to make the enemy not die. <br></br>
    /// You can also do whatever else you want here. <br>
    /// if you return false, the chance for restore drop will not be rolled, so keep that in mind.</br>
    /// </summary>
    public virtual bool PreKill()
    {
        return true;
    }
    /// <summary>
    /// is called when the enemy takes damage.
    ///<br>The life of the enemy is subtracted before this method is called.
    ///</br> 
    ///<br>The base method is empty</br>
    /// </summary>
    /// <param name="damageTaken"></param>
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
        if (life <= 0)
        {
            if (PreKill())
            {
                OnDeathEvents();
                Destroy(gameObject, 1f);
            }
        }
    }

    protected void OnDeathEvents()
    {
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Alpha1))
        {
            GameManager.EnemyKillCount += 5;
        }
#endif
        if(++GameManager.EnemyKillCount >= 5)
        {
            GameManager.EnemyKillCount -= 5;
            RestoreDrop.SpawnRestore(transform.position);
        }
    }
}
