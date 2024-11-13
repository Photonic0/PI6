using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;

public class WeaponSpike : PlayerWeapon
{
    public override int ChargePerUse => 5;

    public override FlipnoteColors.ColorID EquipColor => FlipnoteColors.ColorID.Yellow;

    protected override float Use()
    {
        return 1;
    }
}
