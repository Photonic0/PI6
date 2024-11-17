using Assets.Common.Characters.Main.Scripts.Weapons;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerScriptsPopulator : MonoBehaviour
{

    [SerializeField] Image lifeBarOutline;
    [SerializeField] Image lifeBarFill;
    [SerializeField] Image lifeBarBack;
    [SerializeField] Image weaponBarOutline;
    [SerializeField] Image weaponBarFill;
    [SerializeField] Image weaponBarBack;
    [SerializeField] Image bossLifeBarOutline;
    [SerializeField] Image bossLifeBarFill;
    [SerializeField] Image bossLifeBarBack;
    [SerializeField] GameObject weaponUpgradePanel;
    [SerializeField] Image basicWeaponBarFill;
    [SerializeField] Image typhoonWeaponBarFill;
    [SerializeField] Image discoWeaponBarFill;
    [SerializeField] Image spikeWeaponBarFill;
    [SerializeField] RectTransform weaponSelectArrow;
    [SerializeField] GameObject typhoonWeaponBarBack;
    [SerializeField] GameObject discoWeaponBarBack;
    [SerializeField] GameObject spikeWeaponBarBack;


    private void Awake()
    {
        UIManager.Instance.lifeBarOutline = lifeBarOutline;
        UIManager.Instance.lifeBarFill = lifeBarFill;
        UIManager.Instance.weaponBarOutline = weaponBarOutline;
        UIManager.Instance.weaponBarFill = weaponBarFill;
        UIManager.Instance.bossLifeBarOutline = bossLifeBarOutline;
        UIManager.Instance.bossLifeBarFill = bossLifeBarFill;
        UIManager.Instance.bossLifeBarBack = bossLifeBarBack;
        UIManager.Instance.lifeBarBack = lifeBarBack;
        UIManager.Instance.weaponBarBack = weaponBarBack;
        PlayerWeaponManager.Initialize(weaponUpgradePanel, basicWeaponBarFill, typhoonWeaponBarFill, discoWeaponBarFill, spikeWeaponBarFill, weaponSelectArrow, typhoonWeaponBarBack, spikeWeaponBarBack, discoWeaponBarBack);
    }
}
