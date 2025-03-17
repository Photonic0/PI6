using UnityEngine;

public static class Settings
{
    public enum KeybindID
    {
        None = -1,
        Jump,
        Shoot,
        SlowWalk,
        OpenWeaponsMenu,
        Pause
    }
    //public static float MusicVolume => musicVolume;
    public static float musicVolume = 1f;
    //public static float SfxVolume => sfxVolume;
    public static float sfxVolume = 1f;

    static int difficultyID;
    public const int DifficultyIDNormal = 0;
    public const int DifficultyIDEasy = 1;
    public static bool NormalDifficulty { get => difficultyID == DifficultyIDNormal; set => difficultyID = value ? DifficultyIDNormal : DifficultyIDEasy; }
    public static bool EasyDifficulty { get => difficultyID == DifficultyIDEasy; set => difficultyID = value ? DifficultyIDEasy : DifficultyIDNormal; }

    public static int weaponSelectControlModeID = WeaponSelectControlModeIDToggle;
    public const int WeaponSelectControlModeIDHold = 1;
    public const int WeaponSelectControlModeIDToggle = 0;
    public const int MaxWeaponSelectControlModeIDs = 2;
    public static bool IsWeaponSelectControlModeToggle => weaponSelectControlModeID == WeaponSelectControlModeIDToggle;
    public static bool IsWeaponSelectControlModeHold => weaponSelectControlModeID == WeaponSelectControlModeIDHold;
    public static KeyCode jumpKey = KeyCode.W;
    public static KeyCode shootKey = KeyCode.Mouse0;
    public static KeyCode slowWalkKey = KeyCode.LeftShift;
    public static KeyCode openWeaponsMenuKey = KeyCode.E;
    public static KeyCode pauseKey = KeyCode.Escape;
    public static void ResetVolumeLevels()
    {
        sfxVolume = 1f;
        musicVolume = 1f;
    }
    //public static Settings instance;
    //void Awake()
    //{
    //    if (instance != null && instance != this)
    //    {
    //        Destroy(gameObject);
    //    }
    //    else
    //    {
    //        instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //}
}
