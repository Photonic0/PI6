using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using System.Collections;
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

    static int debug_sceneToLoadInCoroutine;
    private void Awake()
    {
        if(UIManager.Instance == null)
        {
            debug_sceneToLoadInCoroutine = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(SceneIndices.MainMenu);
            //StartCoroutine(ReloadSceneDelayed());//wont work.... scene change stops coroutines
            return;
        }
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
        for (int i = 0; i < uiThingsToAnimate.Length; i++)
        {
            GameManager.AddToPausedUpatedObjs(uiThingsToAnimate[i]);
        }
    }
    //IEnumerator ReloadSceneDelayed()
    //{
    //    yield return 1f;
    //    SceneManager.LoadScene(debug_sceneToLoadInCoroutine);
    //}
}
