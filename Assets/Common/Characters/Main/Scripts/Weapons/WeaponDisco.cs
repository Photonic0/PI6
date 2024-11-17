using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class WeaponDisco : PlayerWeapon
{
    public override int ChargePerUse => 5;

    public override FlipnoteColors.ColorID EquipColor => FlipnoteColors.ColorID.Magenta;

    protected override float Use()
    {
        if(!PlayerProjectilesPool.GetDiscoShot(out DiscoShot proj))
            return 0f;
        proj.parent.SetActive(true);
        proj.gameObject.SetActive(true);
        proj.ResetValues();
        Vector2 origin = CannonPosition;
        Vector2 velocity = new Vector2(Mathf.Sign(Helper.MouseWorld.x - GameManager.PlayerPosition.x) * 15, 0);
        proj.transform.position = origin;
        proj.rb.velocity = velocity;
        return 1f;
    }
}
