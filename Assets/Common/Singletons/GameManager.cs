using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Systems;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PlayerControl playerControl;
    public static PlayerControl PlayerControl => instance.playerControl;
    public PlayerRenderer playerRenderer;
    public static PlayerRenderer PlayerRenderer => instance.playerRenderer;
    public PlayerLife playerLife;
    public static PlayerLife PlayerLife => instance.playerLife;
    public static Vector3 PlayerPosition => instance.playerControl.Position;
    //assigned by multi scene populator
    public static Checkpoint[] checkpoints;

    //keep between scene reloads, but not between scene loads
    public static int latestCheckpointIndex;

    public static Checkpoint LatestCheckpoint => latestCheckpointIndex == -1 ? null : checkpoints[latestCheckpointIndex];
    private void Awake()//assign player scripts refs in awake of player scripts so it goes fine when changing scenes
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            latestCheckpointIndex = -1;
            PlayerLife.chances = PlayerLife.StartingChances;
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    //call when transitioning stages
    public static void CleanupCheckpoints()
    {
        latestCheckpointIndex = -1;
        checkpoints = null;
    }
    public static void CleanupCheckpointsButNotIndex()
    {
        checkpoints = null;
    }
}
