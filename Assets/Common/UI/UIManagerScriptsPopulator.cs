using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using System.Collections;
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
        PlayerWeaponManager.Initialize(weaponUpgradePanel, basicWeaponBarFill, typhoonWeaponBarFill, discoWeaponBarFill, spikeWeaponBarFill, weaponSelectArrow, typhoonWeaponBarBack, spikeWeaponBarBack, discoWeaponBarBack);
        for (int i = 0; i < uiThingsToAnimate.Length; i++)
        {
            GameManager.AddToPausedUpatedObjs(uiThingsToAnimate[i]);
        }
        //gameobj has other scripts, so only destroy the script component, not the game object
        Destroy(this);
    }
}
