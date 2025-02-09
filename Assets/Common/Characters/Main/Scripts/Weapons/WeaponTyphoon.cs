using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class WeaponTyphoon : PlayerWeapon
{
    public override int ChargePerUse => 5;
    public override FlipnoteColors.ColorID EquipColor => FlipnoteColors.ColorID.Blue;
    protected override float Use()
    {
        //
        if(PlayerProjectilesPool.GetTyphoonShot(out TyphoonShot proj))
        {
            proj.gameObject.SetActive(true);
            proj.transform.position = CannonPosition;
            //the velocity of the projectile will be pointed towards the mouse
            //travelling at 2.5 units per second
            Vector2 vel = (Helper.MouseWorld - CannonPosition).normalized * TyphoonShot.MaxVel;
            proj.rb.velocity = vel;
            
        }
        return 2;//firing cooldown
    }
}
