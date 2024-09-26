using Assets.Common.Characters.Main.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    const int SegmentsInDisplay = 27;
    public Image lifeBarOutline;
    public Image lifeBarFill;
    public Image lifeBarBack;
    public Image weaponBarOutline;
    public Image weaponBarFill;
    public Image weaponBarBack;
    public Image bossLifeBarOutline;
    public Image bossLifeBarFill;
    public Image bossLifeBarBack;
    static UIManager instance;
    public static UIManager Instance => instance;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public static void UpdatePlayerLifeBar(int currentLife, int lifeMax = PlayerLife.LifeMax)
    {
        if (instance.lifeBarFill == null)
        {
            return;
        }
        float percent = (float)currentLife / lifeMax;
        percent *= SegmentsInDisplay;
        percent = Mathf.Ceil(percent);
        percent /= SegmentsInDisplay;
        instance.lifeBarFill.fillAmount = percent;
    }
    public static void ActivateBossLifeBar(Color color)
    {
        instance.bossLifeBarBack.gameObject.SetActive(true);
        instance.bossLifeBarFill.color = color;
        instance.bossLifeBarOutline.color = Color.white;
    }
    public static void DeactivateBossLifeBar()
    {
        instance.bossLifeBarBack.gameObject.SetActive(false);
    }
    public static void UpdateBossLifeBar(float percent)
    {
        percent *= SegmentsInDisplay;
        percent = Mathf.Ceil(percent);
        percent /= SegmentsInDisplay;
        instance.bossLifeBarFill.fillAmount = percent;
    }
    public static void UpdateWeaponBar()
    {
        PlayerWeapon weapon = GameManager.PlayerControl.weapon;
        float percent = (float)weapon.charge / PlayerWeapon.MaxCharge;
        percent *= SegmentsInDisplay;
        percent = Mathf.Ceil(percent);
        percent /= SegmentsInDisplay;
        instance.weaponBarFill.fillAmount = percent;
    }
    public static void ChangePlayerLifeBarColor(Color color)
    {
        instance.lifeBarOutline.color = color;
    }
}
