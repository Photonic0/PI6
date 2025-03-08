using Assets.Common.Characters.Main.Scripts;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    const int SegmentsInDisplay = 27;
    [SerializeField] TextMeshProUGUI livesLeftText;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] UIVerticalBar lifeBar;
    [SerializeField] UIVerticalBar weaponBar;
    [SerializeField] UIVerticalBar bossLifeBar;
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
        if (instance.lifeBar == null)
        {
            return;
        }
        float percent = (float)currentLife / lifeMax;
        instance.lifeBar.UpdateFill(percent);
    }
    public static void ActivateBossLifeBar(Color color)
    {
        instance.bossLifeBar.gameObject.SetActive(true);
        instance.bossLifeBar.SetSegmentsColor(color);
        instance.bossLifeBar.SetBackColor(Color.white);
    }
    public static void DeactivateBossLifeBar()
    {
        instance.bossLifeBar.gameObject.SetActive(false);
    }
    public static void UpdateBossLifeBar(float percent)
    {
        instance.bossLifeBar.UpdateFill(percent);
    }
    public static void UpdateWeaponBar()
    {
        PlayerWeapon weapon = GameManager.PlayerControl.weapon;
        float percent = (float)weapon.charge / PlayerWeapon.MaxCharge;
        instance.weaponBar.UpdateFill(percent);
    }
    public static void ActivateWeaponBar(Color weaponColor)
    {
        instance.weaponBar.gameObject.SetActive(true);
        instance.weaponBar.SetSegmentsColor(weaponColor);
        instance.weaponBar.SetBackColor(Color.white);
        UpdateWeaponBar();
    }
    public static void DeactivateWeaponBar()
    {
        instance.weaponBar.gameObject.SetActive(false);
    }

    public static void ChangePlayerLifeBarColor(Color color)
    {
        instance.lifeBar.SetBackColor(color);
    }
    public static void DisplayPauseScreen()
    {
        instance.pauseScreen.SetActive(true);
        OptionsMenu.optionsMenuOpen = true;
        instance.livesLeftText.text = $"Lives: {PlayerLife.chances}";
    }
    public static void HidePauseScreen()
    {
        OptionsMenu.optionsMenuOpen = false;
        instance.pauseScreen.SetActive(false);
    }
    public static void Initialize(UIVerticalBar weaponBar, UIVerticalBar lifeBar, UIVerticalBar bossLifeBar, TextMeshProUGUI livesLeftText, GameObject pauseScreen)
    {
        weaponBar.InitializeSegments();
        lifeBar.InitializeSegments();
        bossLifeBar.InitializeSegments();
        instance.weaponBar = weaponBar;
        instance.lifeBar = lifeBar;
        instance.bossLifeBar = bossLifeBar;
        instance.livesLeftText = livesLeftText;
        instance.pauseScreen = pauseScreen;
    }
}
