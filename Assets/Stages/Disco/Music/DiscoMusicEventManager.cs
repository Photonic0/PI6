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
    public const float SecondsPerBeat = 60f / BPM;
    public const int BeatsPerMusicSplit = 16;

    public static DiscoMusicEventManager instance;
    private double beatTimer;
    public int beatCounter;
    private List<IMusicSyncable> syncableObjects;
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioClip[] musicSplits;
    [SerializeField] TilemapRenderer[] tileRenderers;
    [SerializeField] Material discoTileMaterialAsset;
    [SerializeField] TextMeshProUGUI debugtext;
    static readonly int discoTileMaterialFlipColFloatHash = Shader.PropertyToID("_FlipColFloat");

    void Awake()
    {
        syncableObjects = new List<IMusicSyncable>();
        if(instance != null && instance != this)
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

    private void UnloadSingleton(Scene arg0)
    {
        if(arg0.buildIndex == SceneIndices.DiscoStage)
        {
            instance = null;
            SceneManager.sceneUnloaded -= UnloadSingleton;
        }
    }

    void Update()
    {
        beatTimer += Time.deltaTime;
        if(beatTimer > SecondsPerBeat)
        {
            beatTimer -= SecondsPerBeat;

            if (beatCounter % BeatsPerMusicSplit == 0)
            {
                musicAudioSource.PlayOneShot(musicSplits[(beatCounter / BeatsPerMusicSplit) % musicSplits.Length]);
                debugtext.text = "music split index playing: " + (beatCounter / BeatsPerMusicSplit % musicSplits.Length);
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
    private static void Lag(int iterations)
    {
        for(int i = 0;i < iterations;i++) 
        {
            Debug.Log(iterations);
        }
    }
    public static void AddSyncableObject(IMusicSyncable syncableObj)
    {
        if (instance != null && syncableObj != null)
        {
            instance.syncableObjects.Add(syncableObj);
        }
    }
}
