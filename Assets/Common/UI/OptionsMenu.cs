using Assets.Common.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour, IUpdatableWhenPaused
{
    [SerializeField] TextMeshProUGUI weaponSelectControlModeText;
    [SerializeField] Button[] buttonsToDisableWhileChangingKeybinds;
    [SerializeField] Slider[] slidersToDisableWhileChangingKeybinds;
    [SerializeField] TextMeshProUGUI shootKeyText;
    [SerializeField] TextMeshProUGUI jumpKeyText;
    [SerializeField] TextMeshProUGUI openWeaponsMenuKeyText;
    [SerializeField] TextMeshProUGUI pauseKeyText;
    [SerializeField] TextMeshProUGUI slowWalkKeyText;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    Settings.KeybindID keybindIDToSet = Settings.KeybindID.None;
    static TextMeshProUGUI textToChangeAfterKeybindSet;
    public static bool optionsMenuOpen;
    public static bool ChangingKeybinds => textToChangeAfterKeybindSet != null;
    public bool IsNull => this == null;
    public GameObject GameObject => gameObject;
    public void SliderChange_SFXVolume(float value)
    {
        value = sfxVolumeSlider.value;
        Settings.sfxVolume = value;
    }
    public void SliderChange_MusicVolume(float value)
    {
        value = musicVolumeSlider.value;
        MusicManager.SetMusicVolume(value);
        DiscoBossMusicHandler.SetVolume(value);
        DiscoMusicEventManager.SetVolume(value);
        Settings.musicVolume = value;
    }
    private void OnEnable()
    {
        musicVolumeSlider.value = Settings.musicVolume;
        sfxVolumeSlider.value = Settings.sfxVolume;
        shootKeyText.text = Settings.shootKey.ToString();
        jumpKeyText.text = Settings.jumpKey.ToString();
        openWeaponsMenuKeyText.text = Settings.openWeaponsMenuKey.ToString();
        slowWalkKeyText.text = Settings.slowWalkKey.ToString();
    }
    public void ChangeWeaponSelectControlMode()
    {
        CommonSounds.PlayUIConfirm();
        Settings.weaponSelectControlModeID = (Settings.weaponSelectControlModeID + 1) % Settings.MaxWeaponSelectControlModeIDs;
        switch (Settings.weaponSelectControlModeID)
        {
            case Settings.WeaponSelectControlModeIDToggle://ID 0
                weaponSelectControlModeText.text = "Toggle";
                break;
            case Settings.WeaponSelectControlModeIDHold:// ID 1
                weaponSelectControlModeText.text = "Hold";
                break;
        }
    }
    public void ChangeKey_Shoot()
    {
        shootKeyText.text = "Press any key to set";
        textToChangeAfterKeybindSet = shootKeyText;
        keybindIDToSet = Settings.KeybindID.Shoot;
        DisableThingsForKeybindChange();
    }
    public void ChangeKey_SlowWalk()
    {
        slowWalkKeyText.text = "Press any key to set";
        textToChangeAfterKeybindSet = slowWalkKeyText;
        keybindIDToSet = Settings.KeybindID.SlowWalk;
        DisableThingsForKeybindChange();
    }
    public void ChangeKey_Jump()
    {
        jumpKeyText.text = "Press any key to set";
        textToChangeAfterKeybindSet = jumpKeyText;
        keybindIDToSet = Settings.KeybindID.Jump;
        DisableThingsForKeybindChange();
    }
    public void ChangeKey_Pause()
    {
        pauseKeyText.text = "Press any key to set";
        textToChangeAfterKeybindSet = pauseKeyText;
        keybindIDToSet = Settings.KeybindID.Pause;
        DisableThingsForKeybindChange();
    }
    public void ChangeKey_OpenWeaponsMenu()
    {
        openWeaponsMenuKeyText.text = "Press any key to set";
        textToChangeAfterKeybindSet = openWeaponsMenuKeyText;
        keybindIDToSet = Settings.KeybindID.OpenWeaponsMenu;
        DisableThingsForKeybindChange();
    }
    public void ResetOptionsToDefault()
    {
        weaponSelectControlModeText.text = "Toggle";
        Settings.weaponSelectControlModeID = Settings.WeaponSelectControlModeIDToggle;
        Settings.openWeaponsMenuKey = KeyCode.E;
        openWeaponsMenuKeyText.text = "E";
        Settings.jumpKey = KeyCode.W;
        jumpKeyText.text = "W";
        Settings.shootKey = KeyCode.Mouse0;
        shootKeyText.text = "Mouse0";
        Settings.slowWalkKey = KeyCode.LeftShift;
        slowWalkKeyText.text = "LeftShift";
        Settings.pauseKey = KeyCode.Escape;
        pauseKeyText.text = "Esc";
        musicVolumeSlider.value = 1;
        Settings.musicVolume = 1f;
        sfxVolumeSlider.value = 1;
        Settings.sfxVolume = 1f;
        SliderChange_MusicVolume(1f);
        SliderChange_SFXVolume(1f);
    }
    void DisableThingsForKeybindChange()
    {
        for (int i = 0; i < buttonsToDisableWhileChangingKeybinds.Length; i++)
        {
            buttonsToDisableWhileChangingKeybinds[i].enabled = false;
        }
        for (int i = 0; i < slidersToDisableWhileChangingKeybinds.Length; i++)
        {
            slidersToDisableWhileChangingKeybinds[i].enabled = false;
        }
    }
    void EnableThingsDisabledForKeybindChange()
    {
        for (int i = 0; i < buttonsToDisableWhileChangingKeybinds.Length; i++)
        {
            buttonsToDisableWhileChangingKeybinds[i].enabled = true;
        }
        for (int i = 0; i < slidersToDisableWhileChangingKeybinds.Length; i++)
        {
            slidersToDisableWhileChangingKeybinds[i].enabled = true;
        }
    }
    public void BackButton()
    {
        UIManager.HidePauseScreen();
        GameManager.UnpauseGame();
    }
    public void PausedUpdate(float unscaledDeltaTime)
    {
        if (keybindIDToSet != Settings.KeybindID.None)
        {
            KeyCode[] keys = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
            for (int i = 0; i < keys.Length; i++)
            {
                KeyCode key = keys[i];
                if (Input.GetKeyDown(key))
                {
                    switch (keybindIDToSet)
                    {
                        case Settings.KeybindID.Jump:
                            Settings.jumpKey = key;
                            break;
                        case Settings.KeybindID.Shoot:
                            Settings.shootKey = key;
                            break;
                        case Settings.KeybindID.SlowWalk:
                            Settings.slowWalkKey = key;
                            break;
                        case Settings.KeybindID.OpenWeaponsMenu:
                            Settings.openWeaponsMenuKey = key;
                            break;
                        case Settings.KeybindID.Pause:
                            Settings.pauseKey = key;
                            break;
                    }
                    textToChangeAfterKeybindSet.text = key.ToString();
                    keybindIDToSet = Settings.KeybindID.None;
                    EnableThingsDisabledForKeybindChange();
                }
            }
        }
    }
    private void OnDestroy()
    {
        optionsMenuOpen = false;
        GameManager.RemoveFromPausedUpdateObjs(this);
    }
    private void Awake()
    {
        GameManager.AddToPausedUpatedObjs(this);
    }
}
