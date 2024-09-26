using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using UnityEngine;

namespace Assets.Common.Characters.Main.Scripts
{
    public abstract class PlayerWeapon
    {
        public const int MaxCharge = 100;
        public abstract int ChargePerUse { get; }
        public abstract FlipnoteColors.ColorID EquipColor { get; }

        public int charge = MaxCharge;
        public Vector2 CannonPosition => GameManager.PlayerControl.rb.position + new Vector2(1f * GameManager.PlayerRenderer.SpriteDirection, 0.2f);
        /// <returns>if the weapon was used</returns>
        public bool TryUse(ref float weaponCooldown)
        {
            if(charge >= ChargePerUse)
            {
                charge -= ChargePerUse;
                weaponCooldown = Use();
                return true;    
            }
            return false;
        }
        /// <returns>weapon cooldown in seconds</returns>
        protected abstract float Use();
    }
}
