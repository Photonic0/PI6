using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using UnityEngine;

public class WeaponTyphoon : PlayerWeapon
{
    public override int ChargePerUse => 5;
    public override FlipnoteColors.ColorID EquipColor => FlipnoteColors.ColorID.Blue;
    protected override float Use()
    {
        //Object.Instantiate(CommonPrefabs.TyphoonShot, CannonPosition, Quaternion.identity);
        return 1;
    }
}
