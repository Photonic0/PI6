using Assets.Common.Characters.Main.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    const int SegmentsInDisplay = 27;
    [SerializeField] Image lifeBarOutline;
    [SerializeField] Image lifeBarFill;
    [SerializeField] Image lifeBarBack;
    [SerializeField] Image weaponBarOutline;
    [SerializeField] Image weaponBarFill;
    [SerializeField] Image weaponBarBack;
    [SerializeField] Image bossLifeBarOutline;
    [SerializeField] Image bossLifeBarFill;
    [SerializeField] Image bossLifeBarBack;
    [SerializeField] TextMeshProUGUI livesLeftText;
    public static TextMeshProUGUI LivesLeftText => instance.livesLeftText;
    static UIManager instance;
    [SerializeField] AudioSource audioSource;
    public static AudioSource AudioSource => instance.audioSource;
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

    public static void Initialize(Image lifeBarOutline, Image lifeBarFill, Image weaponBarOutline, Image weaponBarFill, Image bossLifeBarOutline, Image bossLifeBarFill, Image bossLifeBarBack, Image lifeBarBack, Image weaponBarBack, TextMeshProUGUI livesLeftText)
    {
        instance.lifeBarOutline = lifeBarOutline;
        instance.lifeBarFill = lifeBarFill;
        instance.weaponBarOutline = weaponBarOutline;
        instance.weaponBarFill = weaponBarFill;
        instance.bossLifeBarOutline = bossLifeBarOutline;
        instance.bossLifeBarFill = bossLifeBarFill;
        instance.bossLifeBarBack = bossLifeBarBack;
        instance.lifeBarBack = lifeBarBack;
        instance.weaponBarBack = weaponBarBack;
        instance.livesLeftText = livesLeftText;
    }
}
