using Assets.Common.Consts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
/// <summary>
/// this NEEDS to be *before* the script of the bossAI
/// </summary>
public class DiscoBossMusicHandler : MonoBehaviour
{
    public const float BPM = 138;
    public const double SecondsPerBeat = 60.0 / BPM;
    public const int BeatsInMusic = 240;
    //const int

    public bool StartedMusic = false;
    public bool Paused { get; private set; }
    public double beatTimer;
    private float delayTimer;//need to delay the execution of the music synced action a bit
    public int beatCounter;
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioClip intro;
    [SerializeField] AudioClip music;
    [SerializeField] TilemapRenderer[] tileRenderers;
    [SerializeField] Material discoTileMaterialAsset;
    [SerializeField] TextMeshProUGUI debugtext;
    [SerializeField] DiscoBossAI boss;
    [SerializeField] IMusicSyncable[] syncableObjs;
    bool playMusicFileNextBeat;
    static readonly int discoTileMaterialFlipColFloatHash = Shader.PropertyToID("_FlipColFloat");

    [SerializeField] int debug_startMusicAtBeat = -1;
    [SerializeField] bool debug_restartMusic;
    private void Start()
    {
        DiscoMusicEventManager.instance.discoBossMusicHandler = this;
        musicAudioSource.clip = music;
    }
    public void StartMusic()
    {
        DiscoMusicEventManager.PauseMusic();
        musicAudioSource.PlayOneShot(intro);
        playMusicFileNextBeat = false;
        StartedMusic = true;
        beatTimer = -7f * SecondsPerBeat;//intro duration
        delayTimer = float.MaxValue;
        beatCounter = -1;
    }
    void StartMusicNoIntro()
    {
        DiscoMusicEventManager.PauseMusic();
        playMusicFileNextBeat = false;
        StartedMusic = true;
        beatTimer = 0;
        delayTimer = float.MaxValue;
        beatCounter = debug_startMusicAtBeat <= 0 ? -1 : debug_startMusicAtBeat - 1;
    }
    void Update()
    {
        if (Paused || !StartedMusic)
            return;
        if (debug_restartMusic)
        {
            musicAudioSource.Stop();
            StartedMusic = false;
            StartMusicNoIntro();
            debug_restartMusic = false;
            return;
        }
        if (playMusicFileNextBeat)
        {
            float playbackPos = Time.deltaTime;
            if(debug_startMusicAtBeat > 0)
            {
                playbackPos += debug_startMusicAtBeat * (float)SecondsPerBeat;
                Debug.Log("started early at " + playbackPos + " sec");
            }
            musicAudioSource.time = playbackPos;
            musicAudioSource.Play();
            playMusicFileNextBeat = false;
        }
        delayTimer += Time.deltaTime;
        beatTimer += Time.deltaTime;
        if (beatTimer > SecondsPerBeat)
        {
            delayTimer = -0.05f;
            beatTimer -= SecondsPerBeat;
            beatCounter++;
            beatCounter %= BeatsInMusic;
            if ((debug_startMusicAtBeat <= -1 && beatCounter == 0) || beatCounter == debug_startMusicAtBeat)
            {
                playMusicFileNextBeat = true;
            }
        }
        if (delayTimer > 0 && delayTimer < 9999999f)
        {
            for (int i = 0; i < syncableObjs.Length; i++)
            {
                syncableObjs[i].DoMusicSyncedAction();
            }
#if UNITY_EDITOR
            debugtext.color = beatCounter % 2 == 0 ? Color.magenta : FlipnoteColors.Yellow;
            debugtext.text = "current beat: " + beatCounter;
#endif
            delayTimer = float.MaxValue;
            boss.beatFrame = true;
            discoTileMaterialAsset.SetFloat(discoTileMaterialFlipColFloatHash, beatCounter % 2);
            for (int i = 0; i < tileRenderers.Length; i++)
            {
                tileRenderers[i].material.SetFloat(discoTileMaterialFlipColFloatHash, beatCounter % 2);
            }
        }
    }
    public void Pause()
    {
        if (StartedMusic)
        {
            Paused = true;
            musicAudioSource.Pause();
        }
    }
    public void UnPause()
    {
        if ((StartedMusic) && Paused)
        {
            Paused = false;
            musicAudioSource.UnPause();
        }
    }
    public void AddToSyncables(IMusicSyncable obj)
    {
        if (syncableObjs == null)
            syncableObjs = new IMusicSyncable[1] { obj };
        if (syncableObjs[^1] == null)
        {
            syncableObjs[^1] = obj;
        }
        else
        {
            Array.Resize(ref syncableObjs, syncableObjs.Length + 1);
            syncableObjs[^1] = obj;
        }
    }
}
