using Assets.Common.Consts;
using UnityEngine;

namespace Assets.Common.Characters.Main.Scripts.Weapons
{
    public class WeaponBasic : PlayerWeapon
    {
        public override int ChargePerUse => 0;

        public override FlipnoteColors.ColorID EquipColor => FlipnoteColors.ColorID.Orange;

        protected override float Use()
        {
            GameObject obj = UnityEngine.Object.Instantiate(CommonPrefabs.BasicShot, CannonPosition, Quaternion.identity);
            BasicShot basicShot = obj.GetComponent<BasicShot>();
            basicShot.velocity = new Vector2(GameManager.PlayerRenderer.SpriteDirection * 9, 0);
            CommonSounds.PlayCommonShotSound(GameManager.PlayerControl.shootAudioSource);
            return .2f;   
        }
    }
}
