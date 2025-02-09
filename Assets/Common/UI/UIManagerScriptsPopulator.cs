using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] UIButtonAnimator[] uiThingsToAnimate;
    [SerializeField] TextMeshProUGUI livesLeftText;
    [SerializeField] RectTransform leftWheelPartTransform;
    [SerializeField] RectTransform leftWheelPartLockTransform;
    [SerializeField] RectTransform rightWheelPartTransform;
    [SerializeField] RectTransform rightWheelPartLockTransform;
    [SerializeField] RectTransform upWheelPartLockTransform;
    [SerializeField] RectTransform upWheelPartTransform;
    [SerializeField] RectTransform bottomWheelPartTransform;
    [SerializeField] RectTransform bottomWheelPartLockTransform;
    [SerializeField] RectTransform wheelRectTransform;
    [SerializeField] GameObject wheelGameObj;
    [SerializeField] RectTransform typhoonWeaponIcon;
    [SerializeField] RectTransform basicWeaponIcon;
    [SerializeField] RectTransform spikeWeaponIcon;
    [SerializeField] RectTransform discoWeaponIcon;
    [SerializeField] Image typhoonWeaponChargeDisplayWheelFill;
    [SerializeField] Image basicWeaponChargeDisplayWheelFill;
    [SerializeField] Image spikeWeaponChargeDisplayWheelFill;
    [SerializeField] Image discoWeaponChargeDisplayWheelFill;
    private void Awake()
    {
#if UNITY_EDITOR
        if (UIManager.Instance == null)
        {
            SceneManager.LoadScene(SceneIndices.MainMenu);
            return;
        }
#endif
        //I love readability
        UIManager.Initialize(lifeBarOutline, lifeBarFill, weaponBarOutline, weaponBarFill, bossLifeBarOutline, bossLifeBarFill, bossLifeBarBack, lifeBarBack, weaponBarBack, livesLeftText);
        PlayerWeaponManager.Initialize(
            leftWheelPartTransform, leftWheelPartLockTransform, rightWheelPartTransform, rightWheelPartLockTransform,
            upWheelPartTransform, upWheelPartLockTransform, bottomWheelPartTransform, bottomWheelPartLockTransform,
            wheelRectTransform, wheelGameObj,
            typhoonWeaponIcon, basicWeaponIcon, spikeWeaponIcon, discoWeaponIcon,
            typhoonWeaponChargeDisplayWheelFill, basicWeaponChargeDisplayWheelFill, spikeWeaponChargeDisplayWheelFill, discoWeaponChargeDisplayWheelFill
        );

        for (int i = 0; i < uiThingsToAnimate.Length; i++)
        {
            GameManager.AddToPausedUpatedObjs(uiThingsToAnimate[i]);
        }
        //gameobj has other scripts, so only destroy the script component, not the game object
        Destroy(this);
    }
}
