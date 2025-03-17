using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

namespace Assets.Common.Characters.Main.Scripts
{
    public abstract class PlayerWeapon
    {
        public const int MaxCharge = 100;
        public abstract int ChargePerUse { get; }
        public abstract FlipnoteStudioColors.ColorID EquipColor { get; }

        public int charge = MaxCharge;
        public Vector2 CannonPosition => GameManager.PlayerControl.rb.position + new Vector2(Mathf.Sign(Helper.MouseWorld.x - GameManager.PlayerPosition.x), 0.2f);
        /// <returns>if the weapon was used</returns>
        public bool TryUse(ref float weaponCooldown)
        {
            if(charge >= ChargePerUse)
            {
                charge -= ChargePerUse;
                weaponCooldown = Use();
                UIManager.UpdateWeaponBar();
                return true;    
            }
            UIManager.UpdateWeaponBar();
            return false;
        }
        /// <returns>weapon cooldown in seconds</returns>
        protected abstract float Use();
        public void RestoreCharge(int amount)
        {
            charge += amount;
            if(charge > MaxCharge)
            {
                charge = MaxCharge;
            }
            UIManager.UpdateWeaponBar();
        }
        public static Color GetWeaponColorSafely(PlayerWeapon weapon)
        {
            if (weapon == null)
            {
                return FlipnoteStudioColors.Orange;
            }
            return FlipnoteStudioColors.GetColor(weapon.EquipColor);
        }
    }
}
