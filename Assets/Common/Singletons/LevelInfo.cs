using Assets.Common.Systems;
using UnityEngine;
using static Assets.Common.Consts.FlipnoteColors;

public static class LevelInfo
{
    public static bool[] permanentlyDestroyedObjsFlags;
    public static GameObject[] permanentlyDestroyableObjs;
    //assigned by multi scene populator
    public static Checkpoint[] checkpoints;

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
    }
    public static void SetLevelColor(ColorID colorID)
    {
        levelColorID = colorID;
        levelColor = GetColor(levelColorID);
    }
}
