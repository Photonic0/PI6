
namespace Assets.Common.Interfaces
{
    public interface IDamageable
    {
        public void Heal(int life);
        public void Damage(int damage);
    }
}