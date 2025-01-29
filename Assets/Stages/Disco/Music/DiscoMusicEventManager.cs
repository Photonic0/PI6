using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class DiscoMusicEventManager : MonoBehaviour
{
    public enum SyncableObjAddFlags
    {
        BossOnly = 0b01,
        LevelOnly = 0b10,
        Both = BossOnly | LevelOnly
    }
    public const float BPM = 144f;
    public const double SecondsPerBeat = 60.0 / BPM;
    public static double SecondsPerFootstepCheck => 60.0 / (BPM * 2);
    public const int BeatsPerMusicSplit = 16;
    public bool Paused { get; private set; }
    public DiscoBossMusicHandler discoBossMusicHandler;
    bool DiscoBossMusicStarted => discoBossMusicHandler != null && discoBossMusicHandler.StartedMusic;
    public static DiscoMusicEventManager instance;
    private double beatTimer;
    private double footstepCheckTimer;

    private float delayTimer;//need to delay the execution of the music synced action a bit
    public int beatCounter;
    private List<IMusicSyncable> syncableObjects;
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioClip intro;
    [SerializeField] AudioClip[] musicSplits;
    [SerializeField] TilemapRenderer[] tileRenderers;
    [SerializeField] Material discoTileMaterialAsset;
    static readonly int discoTileMaterialFlipColFloatHash = Shader.PropertyToID("_FlipColFloat");

    void Awake()
    {
        syncableObjects = new List<IMusicSyncable>();
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            beatTimer = 0;
            instance = this;
        }

        SceneManager.sceneUnloaded += UnloadSingleton;
    }
    private void Start()
    {
        if (LevelInfo.latestCheckpointIndex < 0)
        {
            musicAudioSource.PlayOneShot(intro);
            beatTimer = -SecondsPerBeat * 10;//time for the beat timer to wait out the intro
        }
        else
        {
            beatTimer = 0;
        }
    }

    private void UnloadSingleton(Scene arg0)
    {
        if (arg0.buildIndex == SceneIndices.DiscoStage)
        {
            instance = null;
            SceneManager.sceneUnloaded -= UnloadSingleton;
        }
    }

    void Update()
    {
        //GetPitchMultiplierForFootsteps();
        if (Paused)
        {
            return;
        }
        float deltaTime = Time.deltaTime;
        footstepCheckTimer += deltaTime;
        if (footstepCheckTimer > SecondsPerFootstepCheck)
        {
            footstepCheckTimer -= SecondsPerFootstepCheck;
            Vector2 playerVel = GameManager.PlayerControl.rb.velocity;
            playerVel.x = (int)(10000 * playerVel.x) / 10000;
            playerVel.y = (int)(10000 * playerVel.y) / 10000;
            if (playerVel.x != 0 && playerVel.y == 0 && GameManager.PlayerControl.NotInKBAnim && (Mathf.Repeat((float)beatTimer, (float)SecondsPerBeat) > SecondsPerBeat * .75f || !GameManager.PlayerControl.SlowWalkKeyInput))
            {
                CommonSounds.PlayFootstep(GameManager.PlayerRenderer.FootstepAudioSource);
            }
        }
        if (DiscoBossMusicStarted)
        {
            return;
        }
        delayTimer += deltaTime;
        beatTimer += deltaTime;
       
        if (beatTimer > SecondsPerBeat)
        {
            beatTimer -= SecondsPerBeat;
            delayTimer = -0.1f;
            if (beatCounter % BeatsPerMusicSplit == 0)
            {
                musicAudioSource.PlayOneShot(musicSplits[(beatCounter / BeatsPerMusicSplit) % musicSplits.Length]);
            }
            beatCounter++;

        }
        if (delayTimer > 0 && delayTimer < 9999999)
        {
            delayTimer = float.MaxValue;
            if (syncableObjects != null && syncableObjects.Count > 0)
            {
                for (int i = 0; i < syncableObjects.Count; i++)
                {
                    IMusicSyncable syncableObj = syncableObjects[i];
                    int offsetBeat = beatCounter + syncableObj.BeatOffset;
                    if (Mathf.Repeat(offsetBeat, syncableObj.BeatsPerAction) == 0)
                    {
                        syncableObj.DoMusicSyncedAction();
                    }
                }
            }
            discoTileMaterialAsset.SetFloat(discoTileMaterialFlipColFloatHash, beatCounter % 2);
            for (int i = 0; i < tileRenderers.Length; i++)
            {
                tileRenderers[i].material.SetFloat(discoTileMaterialFlipColFloatHash, beatCounter % 2);
            }
        }
    }
#if UNITY_EDITOR
    private static void Lag(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            Debug.Log(iterations);
        }
    }
#endif

    public static void AddSyncableObject(IMusicSyncable syncableObj, SyncableObjAddFlags flags = SyncableObjAddFlags.LevelOnly)
    {
        if (instance != null && syncableObj != null && instance.syncableObjects != null)
        {
            if (instance.syncableObjects.Contains(syncableObj))
            {
                return;
            }
            if ((flags & SyncableObjAddFlags.LevelOnly) == SyncableObjAddFlags.LevelOnly)
            {
                instance.syncableObjects.Add(syncableObj);
            }
            if ((flags & SyncableObjAddFlags.BossOnly) == SyncableObjAddFlags.BossOnly)
            {
                instance.discoBossMusicHandler.AddToSyncables(syncableObj);
            }
        }
    }

    public static void PauseMusic()
    {
        if (instance == null) return;
        if (instance.DiscoBossMusicStarted)
        {
            instance.discoBossMusicHandler.Pause();
            return;
        }
        instance.Paused = true;
        instance.musicAudioSource.Pause();
    }
    public static void UnPauseMusic()
    {
        if (instance == null) return;
        if (instance.DiscoBossMusicStarted)
        {
            instance.discoBossMusicHandler.UnPause();
            return;
        }
        instance.Paused = false;
        instance.musicAudioSource.UnPause();
    }
    public static void Disable()
    {
        instance.enabled = false;
        instance.syncableObjects.Clear();
        instance.syncableObjects = null;
    }
    static float GetPitchMultiplierForFootsteps()
    {
        int beat = instance.beatCounter;
        //float time = (float)(instance.beatTimer + (beat - 1) * SecondsPerBeat);

        
        return 1;
    }
}
