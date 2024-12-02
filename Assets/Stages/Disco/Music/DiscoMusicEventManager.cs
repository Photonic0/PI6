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
    public const int BeatsPerMusicSplit = 16;
    public bool Paused { get; private set; }
    public DiscoBossMusicHandler discoBossMusicHandler;
    bool DiscoBossMusicStarted => discoBossMusicHandler != null && discoBossMusicHandler.StartedMusic;
    public static DiscoMusicEventManager instance;
    private double beatTimer;
    
    private float delayTimer;//need to delay the execution of the music synced action a bit
    public int beatCounter;
    private List<IMusicSyncable> syncableObjects;
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioClip intro;
    [SerializeField] AudioClip[] musicSplits;
    [SerializeField] TilemapRenderer[] tileRenderers;
    [SerializeField] Material discoTileMaterialAsset;
    [SerializeField] TextMeshProUGUI debugtext;
    static readonly int discoTileMaterialFlipColFloatHash = Shader.PropertyToID("_FlipColFloat");

    void Awake()
    {
#if UNITY_EDITOR
        debugtext.gameObject.SetActive(true);
#endif
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
        musicAudioSource.PlayOneShot(intro);
        beatTimer = -SecondsPerBeat * 10;//time for the beat timer to wait out the intro
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
        if (Paused || DiscoBossMusicStarted)
            return;
        delayTimer += Time.deltaTime;
        beatTimer += Time.deltaTime;
        if (beatTimer > SecondsPerBeat)
        {
            beatTimer -= SecondsPerBeat;
            delayTimer = -0.1f;
            if (beatCounter % BeatsPerMusicSplit == 0)
            {
                musicAudioSource.PlayOneShot(musicSplits[(beatCounter / BeatsPerMusicSplit) % musicSplits.Length]);
#if UNITY_EDITOR
                debugtext.text = "music split index playing: " + (beatCounter / BeatsPerMusicSplit % musicSplits.Length);
#endif
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
                    if (beatCounter % syncableObj.BeatsPerAction == 0)
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
        if (instance != null && syncableObj != null)
        {
            if ((flags & SyncableObjAddFlags.LevelOnly) == SyncableObjAddFlags.LevelOnly)
            {
                instance.syncableObjects.Add(syncableObj);
            }
            if((flags & SyncableObjAddFlags.BossOnly) == SyncableObjAddFlags.BossOnly)
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

}
