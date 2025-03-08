using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class WeaponDisco : PlayerWeapon
{
    public override int ChargePerUse => 10;

    public override FlipnoteColors.ColorID EquipColor => FlipnoteColors.ColorID.Magenta;

    protected override float Use()
    {
        if(!PlayerProjectilesPool.GetDiscoShot(out DiscoShot proj))
            return 0f;
        float directionSign = Mathf.Sign(Helper.MouseWorld.x - GameManager.PlayerPosition.x);
        Vector2 velocity = new Vector2(directionSign * 15, 0);
        ScreenShakeManager.AddDirectionalShake(new Vector2(directionSign,0), ScreenShakeManager.SmallShakeMagnitude);
        CommonSounds.PlayBwow(GameManager.PlayerControl.shootAudioSource);
        proj.parent.SetActive(true);
        proj.gameObject.SetActive(true);
        proj.Initialize();
        Vector2 origin = CannonPosition;
        proj.transform.position = origin;
        proj.rb.velocity = velocity;
        return 1f;
    }
}
