using Assets.Common.Systems;
using UnityEngine;
using static Assets.Common.Consts.FlipnoteStudioColors;

public static class LevelInfo
{
    public static int currentLevelMusicID;
    public const int LevelIDSpike = 0;
    public const int LevelIDDisco = 1;
    public const int LevelIDTyphoon = 2;
    public static bool[] clearedLevels = new bool[3] { false, false, false };
    public static bool[] permanentlyDestroyedObjsFlags;
    public static GameObject[] permanentlyDestroyableObjs;
    //assigned by multi scene populator
    public static Checkpoint[] checkpoints;
    public static AudioClip currentLevelMusicStart;
    public static AudioClip currentLevelMusicLoop;
    //keep between scene reloads, but not between scene loads
    public static int latestCheckpointIndex;
    static Color levelColor;
    static ColorID levelColorID;
    public static Color LevelColor => levelColor;
    public static ColorID LevelColorID => levelColorID;
    public static Checkpoint LatestCheckpoint => latestCheckpointIndex == -1 ? null : checkpoints[latestCheckpointIndex];
    public static void PrepareStageChange()
    {
        latestCheckpointIndex = -1;
        checkpoints = null;
        permanentlyDestroyedObjsFlags = null;
        levelColor = Orange;
        levelColorID = ColorID.Orange;
        currentLevelMusicLoop = null;
        currentLevelMusicStart = null;
        currentLevelMusicID = -2;
    }
    public static void SetLevelColor(ColorID colorID)
    {
        levelColorID = colorID;
        levelColor = GetColor(levelColorID);
    }
    public static void SetLevelClearFlag(int levelID)
    {
        clearedLevels[levelID] = true;   
    }
}
