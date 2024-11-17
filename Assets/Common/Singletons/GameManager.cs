using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Common.Interfaces;
using Assets.Common.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool Paused { get; private set; }
    [SerializeField] int debug_FPS = -1;

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
    [SerializeField] List<IUpdatableWhenPaused> thingsToUpdateWhenPaused;
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
            thingsToUpdateWhenPaused = new();
            Paused = false;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        Application.targetFrameRate = debug_FPS;
    }
    /// <summary>
    /// call when transitioning stages
    /// </summary>
    public static void CleanupCheckpoints()
    {
        latestCheckpointIndex = -1;
        checkpoints = null;
    }
    public static void CleanupCheckpointsButNotIndex()
    {
        checkpoints = null;
    }

    internal static void PauseGame()
    {
        Paused = true;
        Time.timeScale = 0;
        if (SceneManager.GetActiveScene().buildIndex == SceneIndices.DiscoStage)
        {
            DiscoMusicEventManager.PauseMusic();
        }
        instance.StartCoroutine(instance.UpdatePausedObjects());
    }
    IEnumerator UpdatePausedObjects()
    {
        while (Paused)
        {
            for (int i = 0; i < thingsToUpdateWhenPaused.Count; i++)
            {
                thingsToUpdateWhenPaused[i].PausedUpdate();
            }
            yield return new WaitForSecondsRealtime(1f / 30f);//30 fps updating
        }
    }
    internal static void UnpauseGame()
    {
        Paused = false;
        Time.timeScale = 1;
        if (SceneManager.GetActiveScene().buildIndex == SceneIndices.DiscoStage)
        {
            DiscoMusicEventManager.UnPauseMusic();
        }
    }

    public static void AddToPausedUpatedObjs(IUpdatableWhenPaused obj)
    {
        instance.thingsToUpdateWhenPaused.Add(obj);
    }
}
