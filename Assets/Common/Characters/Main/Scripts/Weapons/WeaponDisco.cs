using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;

public class WeaponDisco : PlayerWeapon
{
    public override int ChargePerUse => 5;

    public override FlipnoteColors.ColorID EquipColor => FlipnoteColors.ColorID.Magenta;

    protected override float Use()
    {
        return 1f;
    }
}
