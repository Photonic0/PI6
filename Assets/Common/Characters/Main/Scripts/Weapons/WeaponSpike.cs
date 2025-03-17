using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class WeaponSpike : PlayerWeapon
{
    public override int ChargePerUse => 10;

    public override FlipnoteStudioColors.ColorID EquipColor => FlipnoteStudioColors.ColorID.Yellow;

    protected override float Use()
    {
        if (!PlayerProjectilesPool.GetSpikeShot(out SpikeShot proj))
            return 0f;
        proj.gameObject.SetActive(true);
        Vector2 mousePos = Helper.MouseWorld;
        Vector2 origin = CannonPosition;
        proj.transform.position = origin;
        Vector2 velocity = (mousePos - origin).normalized * 10;
        proj.rb.velocity = velocity;
        CommonSounds.PlayThrowSound(GameManager.PlayerControl.shootAudioSource);
        return 1f;
    }
}
