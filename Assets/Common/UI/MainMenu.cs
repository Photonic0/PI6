using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Helpers;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    [SerializeField] AudioSource cameraAudioSource;
    [SerializeField] Slider loadBar;
    [SerializeField] RectTransform stageSelectButtonsParentTransform;
    [SerializeField] RectTransform startMenuParentTransform;
    [SerializeField] RectTransform globalMovementTransform;
    [SerializeField] RectTransform optionsMenuParentTransform;
    [SerializeField] RectTransform creditsParentTransform;
    [SerializeField] RectTransform difficultySelectScreenParentTransform;
    [SerializeField] GameObject stageSelectBackButton;
    [SerializeField] RectTransform canvasRectTransform;
    [SerializeField] TextMeshProUGUI weaponSelectControlModeText;
    [SerializeField] TextMeshProUGUI optionDescriptionText;
    Vector3 targetGlobalPosition;
    Vector3 globalPositionForStageSelectScreen;
    Vector3 globalPositionForCredits;
    Vector3 globalPositionForOptions;
    Vector3 globalPositionForStartingMenu;
    Vector3 globalPositionForDifficultySelectScreen;
    [SerializeField] Button[] buttonsToDisableWhileChangingKeybinds;
    [SerializeField] Slider[] slidersToDisableWhileChangingKeybinds;
    [SerializeField] TextMeshProUGUI shootKeyText;
    [SerializeField] TextMeshProUGUI jumpKeyText;
    [SerializeField] TextMeshProUGUI openWeaponsMenuKeyText;
    [SerializeField] TextMeshProUGUI pauseKeyText;
    [SerializeField] TextMeshProUGUI slowWalkKeyText;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] GameObject spikeStageCheckmark;
    [SerializeField] Button spikeStageButton;
    [SerializeField] GameObject discoStageCheckmark;
    [SerializeField] Button discoStageButton;
    [SerializeField] GameObject typhoonStageCheckmark;
    [SerializeField] Button typhoonStageButton;
    [SerializeField] GameObject thankYouText;
    [SerializeField] GameObject[] objsToDisableOnThankYouScreen;
    Settings.KeybindID keybindIDToSet = Settings.KeybindID.None;
    TextMeshProUGUI textToChangeAfterKeybindSet;
    //fix typhoon weapon bug where it tries to damage already dead enemies
    //ON OPTIONS MENU:
    //ADD WEAPON SWITCH CONTROL STYLE
    //ADD VOLUME SLIDER
    //ADD MUSIC SLIDER 
#if UNITY_EDITOR
    [SerializeField] GameObject explosiveStageButton;
#endif
    private void Awake()
    {
        Resolution res = Screen.currentResolution;
        Vector3 parentPos = startMenuParentTransform.position;
        //AdjustDistanceForScreenResolution(res, startMenuParentTransform);
        AdjustDistanceForScreenResolution(res, difficultySelectScreenParentTransform, parentPos);
        AdjustDistanceForScreenResolution(res, stageSelectButtonsParentTransform, parentPos);
        AdjustDistanceForScreenResolution(res, optionsMenuParentTransform, parentPos);
        AdjustDistanceForScreenResolution(res, creditsParentTransform, parentPos);
    }
    private void Start()
    {
#if UNITY_EDITOR
        explosiveStageButton.SetActive(true);
      //  Debug.ClearDeveloperConsole();
#endif
        Resolution res = Screen.currentResolution;
        Vector3 canvasPos = canvasRectTransform.position;
        canvasPos.x += res.width / 2f;
        canvasPos.y += res.height / 2f;
        globalPositionForStartingMenu = canvasPos - startMenuParentTransform.position;
        globalPositionForCredits = canvasPos - creditsParentTransform.position;
        globalPositionForStageSelectScreen = canvasPos - stageSelectButtonsParentTransform.position;
        globalPositionForOptions = canvasPos - optionsMenuParentTransform.position;
        globalPositionForDifficultySelectScreen = canvasPos - difficultySelectScreenParentTransform.position;
        targetGlobalPosition = globalPositionForStartingMenu;
        bool allLevelsCleared = PlayerWeaponManager.UnlockedDiscoWeapon && PlayerWeaponManager.UnlockedSpikeWeapon && PlayerWeaponManager.UnlockedTyphoonWeapon;
        if (GameManager.startedGame && !allLevelsCleared)
        {
            targetGlobalPosition = globalPositionForStageSelectScreen;
            globalMovementTransform.position = globalPositionForStageSelectScreen;
            stageSelectBackButton.SetActive(false);
        }
        if (!allLevelsCleared)
        {
            if (PlayerWeaponManager.UnlockedDiscoWeapon)
            {
                discoStageButton.enabled = false;
                discoStageCheckmark.gameObject.SetActive(true);
            }
            if (PlayerWeaponManager.UnlockedSpikeWeapon)
            {
                spikeStageButton.enabled = false;
                spikeStageCheckmark.gameObject.SetActive(true);
            }
            if (PlayerWeaponManager.UnlockedTyphoonWeapon)
            {
                typhoonStageButton.enabled = false;
                typhoonStageCheckmark.gameObject.SetActive(true);
            }
        }
        else
        {
            thankYouText.gameObject.SetActive(true);
            for (int i = 0; i < objsToDisableOnThankYouScreen.Length; i++)
            {
                objsToDisableOnThankYouScreen[i].SetActive(false);
            }
            StartCoroutine(EndThankYouScreen());
        }
    }
    IEnumerator EndThankYouScreen()
    {
        yield return new WaitForSecondsRealtime(8f);
        thankYouText.gameObject.SetActive(false);
        for (int i = 0; i < objsToDisableOnThankYouScreen.Length; i++)
        {
            objsToDisableOnThankYouScreen[i].SetActive(true);
        }
    }
    //ADD 1UP WHICH IS AN UPWARDS POINTING ARROW
    //ADD A BIG LIFE UP WHICH IS A BATTERY
    public void LoadSpikeStage()
    {
        GameManager.startedGame = true;
        stageSelectBackButton.SetActive(false);
        StartCoroutine(LoadSpikeStageInner());
    }
    IEnumerator LoadSpikeStageInner()
    {
        loadBar.gameObject.SetActive(true);
        loadBar.value = 0f;
        CommonSounds.PlayUIConfirm();
        LevelInfo.PrepareStageChange();
        CommonSounds.LoadSpikeStageFootsteps();
        LevelInfo.SetLevelColor(FlipnoteStudioColors.ColorID.Yellow);
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.SpikeStage);
        loadStatus.allowSceneActivation = false;
        while (loadStatus.progress < 0.9f)
        {
            loadBar.value = Helper.Remap(loadStatus.progress, 0, 0.9f, 0f, 1f);
            yield return null;
        }
        loadBar.value = 1f;
        yield return null;
        MusicManager.StartSpikeMusic();
        loadStatus.allowSceneActivation = true;
    }
    public void LoadDiscoStage()
    {
        GameManager.startedGame = true;
        stageSelectBackButton.SetActive(false);
        StartCoroutine(LoadDiscoStageInner());
    }
    IEnumerator LoadDiscoStageInner()
    {
        loadBar.gameObject.SetActive(true);
        loadBar.value = 0f;
        CommonSounds.PlayUIConfirm();
        LevelInfo.PrepareStageChange();
        CommonSounds.LoadDiscoStageFootsteps();
        LevelInfo.SetLevelColor(FlipnoteStudioColors.ColorID.Magenta);
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.DiscoStage);
        loadStatus.allowSceneActivation = false;
        while (loadStatus.progress < 0.9f)
        {
            loadBar.value = Helper.Remap(loadStatus.progress, 0, 0.9f, 0f, 1f);
            yield return null;
        }
        loadBar.value = 1f;
        yield return null;
        MusicManager.StopMusic();
        loadStatus.allowSceneActivation = true;
    }
    public void LoadTyphoonStage()
    {
        GameManager.startedGame = true;
        stageSelectBackButton.SetActive(false);
        StartCoroutine(LoadTyphoonStageInner());
    }
    IEnumerator LoadTyphoonStageInner()
    {
        loadBar.gameObject.SetActive(true);
        loadBar.value = 0f;
        CommonSounds.PlayUIConfirm();
        LevelInfo.PrepareStageChange();
        CommonSounds.LoadTyphoonStageFootsteps();
        LevelInfo.SetLevelColor(FlipnoteStudioColors.ColorID.Blue);
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.TyphoonStage);
        loadStatus.allowSceneActivation = false;
        while (loadStatus.progress < 0.9f)
        {
            loadBar.value = Helper.Remap(loadStatus.progress, 0, 0.9f, 0f, 1f);
            yield return null;
        }
        loadBar.value = 1f;
        yield return null;
        MusicManager.StartTyphoonMusic();
        loadStatus.allowSceneActivation = true;
    }
    public void LoadExplosiveStage()
    {
        stageSelectBackButton.SetActive(false);
        StartCoroutine(LoadExplosiveStageInner());
    }
    IEnumerator LoadExplosiveStageInner()
    {
        loadBar.gameObject.SetActive(true);
        loadBar.value = 0f;
        CommonSounds.PlayUIConfirm();
        LevelInfo.PrepareStageChange();
        //CommonSounds.LoadTyphoonStageFootsteps(); (if needed)
        LevelInfo.SetLevelColor(FlipnoteStudioColors.ColorID.DarkGreen);
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.ExplosiveStage);
        loadStatus.allowSceneActivation = false;
        while (loadStatus.progress < 0.9f)
        {
            loadBar.value = Helper.Remap(loadStatus.progress, 0, 0.9f, 0f, 1f);
            yield return null;
        }
        loadBar.value = 1f;
        yield return null;
        loadStatus.allowSceneActivation = true;
    }
    public void MoveToDifficultySelectScreen()
    {
        targetGlobalPosition = globalPositionForDifficultySelectScreen;
        MoveToStageSelectScreen();
        //CommonSounds.PlayUIConfirm();
    }
    public void MoveToStageSelectScreen()
    {
        targetGlobalPosition = globalPositionForStageSelectScreen;
        CommonSounds.PlayUIConfirm();
    }
    public void MoveToCredits()
    {
        targetGlobalPosition = globalPositionForCredits;
        CommonSounds.PlayUIConfirm();
    }
    public void MoveToOptions()
    {
        targetGlobalPosition = globalPositionForOptions;
        CommonSounds.PlayUIConfirm();
    }
    public void MoveToStartMenu()
    {
        targetGlobalPosition = globalPositionForStartingMenu;
        CommonSounds.PlayUIConfirm();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void SetDifficultyToEasy()
    {
        Settings.EasyDifficulty = true;
    }
    public void SetDifficultyNormal()
    {
        Settings.NormalDifficulty = true;
    }
    private void Update()
    {
        //make changed by difficulty a unique script which has option to disable, enable or move
        //make it so completing a level shows a text on how to switch weapons
        //make every weapon unlock have a video showing how to switch to and how the weapon works
        //add some grid sprite or something? idk. I guess some random square particles too
        globalMovementTransform.position = Helper.DecayVec3(globalMovementTransform.position, targetGlobalPosition, 20f);
        globalMovementTransform.position = Vector3.MoveTowards(globalMovementTransform.position, targetGlobalPosition, Time.deltaTime);

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
    static float SignOrZero(float value)
    {
        if (value > 0f)
        {
            return 1;
        }
        if (value < 0f)
        {
            return -1;
        }
        return 0;
    }
    private static void AdjustDistanceForScreenResolution(Resolution res, RectTransform transform, Vector3 parentPos)
    {
        Vector3 pos = transform.position - parentPos;
        pos.x = SignOrZero(pos.x) * res.width;
        pos.y = SignOrZero(pos.y) * res.height;
        transform.position = pos + parentPos;
    }
    public void SliderChange_SFXVolume(float value)
    {
        value = sfxVolumeSlider.value;
        Settings.sfxVolume = value;
    }
    public void SliderChange_MusicVolume(float value)
    {
        value = musicVolumeSlider.value;
        Settings.musicVolume = value;
        DiscoMusicEventManager.SetVolume(value);
        DiscoBossMusicHandler.SetVolume(value);
        MusicManager.SetMusicVolume(value);
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
        sfxVolumeSlider.value = 1;
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
}
