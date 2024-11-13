using Assets.Common.Characters.Main.Scripts.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponButtons : MonoBehaviour
{
    public void ChangeWeaponToDisco()
    {
        PlayerWeaponManager.SetPlayerWeapon(PlayerWeaponManager.PlayerWeaponID.Disco);
        PlayerWeaponManager.CloseMenu();
    }
    public void ChangeWeaponToBasic()
    {
        PlayerWeaponManager.SetPlayerWeapon(PlayerWeaponManager.PlayerWeaponID.Basic);
        PlayerWeaponManager.CloseMenu();
    }
    public void ChangeWeaponToTyphoon()
    {
        PlayerWeaponManager.SetPlayerWeapon(PlayerWeaponManager.PlayerWeaponID.Typhoon);
        PlayerWeaponManager.CloseMenu();
    }
    public void ChangeWeaponToSpike()
    {
        PlayerWeaponManager.SetPlayerWeapon(PlayerWeaponManager.PlayerWeaponID.Spike);
        PlayerWeaponManager.CloseMenu();
    }
}
