using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

namespace Assets.Common.Characters.Main.Scripts.Weapons
{
    public class WeaponBasic : PlayerWeapon
    {
        public override int ChargePerUse => 0;

        public override FlipnoteStudioColors.ColorID EquipColor => FlipnoteStudioColors.ColorID.Orange;

        protected override float Use()
        {
            if (!PlayerProjectilesPool.GetBasicShot(out BasicShot basicShot))
                return 0;
            basicShot.transform.position = CannonPosition;
            basicShot.velocity = new Vector2(Mathf.Sign(Helper.MouseWorld.x - GameManager.PlayerPosition.x) * 10, 0);
            basicShot.gameObject.SetActive(true);
            CommonSounds.PlayCommonShotSound(GameManager.PlayerControl.shootAudioSource);
            return .2f;   
        }
    }
}
