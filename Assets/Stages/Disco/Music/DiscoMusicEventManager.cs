using Assets.Common.Consts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class DiscoMusicEventManager : MonoBehaviour
{
    public const float BPM = 144f;
    public const double SecondsPerBeat = 60.0 / BPM;
    public const int BeatsPerMusicSplit = 16;
    public bool Paused { get; private set; }
    public DiscoBossMusicHandler discoBossMusicHandler;
    bool DiscoBossMusicStarted => discoBossMusicHandler != null && discoBossMusicHandler.StartedMusic;
    public static DiscoMusicEventManager instance;
    private double beatTimer;
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

        beatTimer += Time.deltaTime;
        if (beatTimer > SecondsPerBeat)
        {
            beatTimer -= SecondsPerBeat;

            if (beatCounter % BeatsPerMusicSplit == 0)
            {
                musicAudioSource.PlayOneShot(musicSplits[(beatCounter / BeatsPerMusicSplit) % musicSplits.Length]);
#if UNITY_EDITOR
                debugtext.text = "music split index playing: " + (beatCounter / BeatsPerMusicSplit % musicSplits.Length);
#endif
            }
            beatCounter++;
            StartCoroutine(WaitABitToActuallyDoActionAfterPlayingSplit());
        }
    }
    IEnumerator WaitABitToActuallyDoActionAfterPlayingSplit()
    {
        yield return new WaitForSecondsRealtime(.05f);
        int beatCounter = this.beatCounter - 1;//because the beat counter increased before this code executed
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
#if UNITY_EDITOR
    private static void Lag(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            Debug.Log(iterations);
        }
    }
#endif

    public static void AddSyncableObject(IMusicSyncable syncableObj)
    {
        if (instance != null && syncableObj != null)
        {
            instance.syncableObjects.Add(syncableObj);
        }
    }
    public static void PauseMusic()
    {
        if (instance == null) return;
        instance.Paused = true;
        instance.musicAudioSource.Pause();
    }
    public static void UnPauseMusic()
    {
        if (instance == null) return;
        if (instance.DiscoBossMusicStarted) return;
        instance.Paused = false;
        instance.musicAudioSource.UnPause();
    }

}
