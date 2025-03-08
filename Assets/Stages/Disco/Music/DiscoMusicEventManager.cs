using Assets.Common.Consts;
using System.Collections.Generic;
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
    public const int BeatsInMusic = 304;
    public bool Paused { get; private set; }
    public DiscoBossMusicHandler discoBossMusicHandler;
    bool DiscoBossMusicStarted => discoBossMusicHandler != null && discoBossMusicHandler.StartedMusic;
    public static DiscoMusicEventManager instance;
    private double beatTimer;
    private double footstepCheckTimer;
    private double errorCorrectionTimer;
    private float delayTimer;//need to delay the execution of the music synced action a bit
    public int beatCounter;
    private List<IMusicSyncable> syncableObjects;
    private List<IMusicSyncableWithoutSlightDelay> syncableObjectsWithoutSlightDelay;
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioClip intro;
    [SerializeField] AudioClip[] musicSplits;
    [SerializeField] TilemapRenderer[] tileRenderers;
    [SerializeField] Material discoTileMaterialAsset;
    static readonly int discoTileMaterialFlipColFloatHash = Shader.PropertyToID("_FlipColFloat");

    void Awake()
    {
        syncableObjects = new List<IMusicSyncable>();
        syncableObjectsWithoutSlightDelay = new();
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
        musicAudioSource.volume = Settings.musicVolume;
        if (LevelInfo.latestCheckpointIndex < 0)
        {
            musicAudioSource.PlayOneShot(intro);
            beatTimer = -SecondsPerBeat * 10;//time for the beat timer to wait out the intro
            errorCorrectionTimer = -SecondsPerBeat * 11;
        }
        else
        {
            beatTimer = 0;
            errorCorrectionTimer = -SecondsPerBeat;
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
        errorCorrectionTimer += deltaTime;

        if (beatTimer > SecondsPerBeat)
        {
            beatTimer -= SecondsPerBeat;
            delayTimer = -0.1f;
            if (beatCounter % BeatsInMusic == 0)
            {
                musicAudioSource.Play();
#if UNITY_EDITOR
                Debug.Log("played");
#endif
                //musicAudioSource.PlayOneShot(musicSplits[(beatCounter / BeatsPerMusicSplit) % musicSplits.Length]);
            }
            float time = musicAudioSource.time;
            errorCorrectionTimer %= ((BeatsInMusic) * 60.0) / BPM;
            double targetTime = errorCorrectionTimer;
            //todo: fix being weird on respawn
            //targetTime > 1f is a dumb probably not good fix for the error correction having some conflicts with looping the song
            if (System.Math.Abs(time - targetTime) > 0.1 && targetTime > 1f)//error correction
            {
#if UNITY_EDITOR
                if (Time.timeScale == 1)
                {
                    Debug.Log($"error correct, time: {time}, targetTime: {targetTime}");
                }
#endif
                musicAudioSource.time = (float)targetTime;
            }
            beatCounter++;
            if (syncableObjectsWithoutSlightDelay != null && syncableObjectsWithoutSlightDelay.Count > 0)
            {
                for (int i = 0; i < syncableObjectsWithoutSlightDelay.Count; i++)
                {
                    IMusicSyncableWithoutSlightDelay syncableObj = syncableObjectsWithoutSlightDelay[i];
                    int offsetBeat = beatCounter + syncableObj.BeatOffset;
                    if (Mathf.Repeat(offsetBeat, syncableObj.BeatsPerAction) == 0)
                    {
                        syncableObj.DoMusicSyncedActionWithoutDelay();
                    }
                }
            }
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
    public static void RemoveLevelSyncableObject(IMusicSyncable syncableObj)
    {
        if (instance != null && syncableObj != null)
        {
            if (instance.syncableObjects != null)
            {
                instance.syncableObjects.Remove(syncableObj);
            }
            if (instance.syncableObjectsWithoutSlightDelay != null && syncableObj is IMusicSyncableWithoutSlightDelay withoutDelay)
            {
                instance.syncableObjectsWithoutSlightDelay.Remove(withoutDelay);
            }
        }
    }
    public static void AddSyncableObjectWithoutSlightDelay(IMusicSyncableWithoutSlightDelay syncableObj)
    {
        if (instance != null && syncableObj != null && instance.syncableObjectsWithoutSlightDelay != null)
        {
            if (instance.syncableObjectsWithoutSlightDelay.Contains(syncableObj))
            {
                return;
            }
            instance.syncableObjectsWithoutSlightDelay.Add(syncableObj);
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
    public static void SetVolume(float volMult)
    {
        if (instance != null)
        {
            instance.musicAudioSource.volume = volMult;
        }
    }
    static float GetPitchMultiplierForFootsteps()
    {
        int beat = instance.beatCounter;
        //float time = (float)(instance.beatTimer + (beat - 1) * SecondsPerBeat);


        return 1;
    }
}
