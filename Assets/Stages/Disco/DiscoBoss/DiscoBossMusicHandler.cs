using Assets.Common.Consts;
using System;
using System.Collections;
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
    public const double SecondsPerFootstepCheck = 60.0 / (2.0 * BPM);
    public const int BeatsInMusic = 240;
    //const int

    public bool StartedMusic = false;
    public bool Paused { get; private set; }
    public double beatTimer;
    public double audioTimer;
    public double footstepCheckTimer;
    private float delayTimer;//need to delay the execution of the music synced action a bit
    public int beatCounter;
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioClip intro;
    [SerializeField] AudioClip music;
    [SerializeField] TilemapRenderer[] tileRenderers;
    [SerializeField] Material discoTileMaterialAsset;
    [SerializeField] DiscoBossAI boss;
    [SerializeField] IMusicSyncable[] syncableObjs;
    bool playMusicFileNextBeat;
    static readonly int discoTileMaterialFlipColFloatHash = Shader.PropertyToID("_FlipColFloat");
#if UNITY_EDITOR
    [SerializeField] int debug_startMusicAtBeat = -1;
    [SerializeField] bool debug_restartMusic;
#endif
    static DiscoBossMusicHandler instance;
    private void Awake()
    {
        instance = this;
    }
    private void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }
    private void Start()
    {
        DiscoMusicEventManager.instance.discoBossMusicHandler = this;
        musicAudioSource.clip = music;
    }
    public void StartMusic()
    {
#if UNITY_EDITOR
        if(debug_startMusicAtBeat > -1)
        {
            StartMusicNoIntro();
            return;
        }
#endif
        musicAudioSource.volume = Settings.musicVolume;
        DiscoMusicEventManager.PauseMusic();
        musicAudioSource.PlayOneShot(intro);
        playMusicFileNextBeat = false;
        StartedMusic = true;
        beatTimer = -7f * SecondsPerBeat;//intro duration
        delayTimer = float.MaxValue;
        beatCounter = -1;
        StartCoroutine(BossIntroBeatFrames());

    }
    void StartMusicNoIntro()
    {
        DiscoMusicEventManager.PauseMusic();
        playMusicFileNextBeat = false;
        StartedMusic = true;
        beatTimer = 0;
        delayTimer = float.MaxValue;
        beatCounter = -1;
        //beatCounter = debug_startMusicAtBeat <= 0 ? -1 : debug_startMusicAtBeat - 1;
    }
    void Update()
    {
        if (Paused || !StartedMusic)
            return;
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
#if UNITY_EDITOR
        if (debug_restartMusic)
        {
            musicAudioSource.Stop();
            StartedMusic = false;
            StartMusicNoIntro();
            debug_restartMusic = false;
            return;
        }
#endif
        if (playMusicFileNextBeat)
        {
            float playbackPos = Time.deltaTime;
#if UNITY_EDITOR
            if(debug_startMusicAtBeat > 0)
            {
                playbackPos += debug_startMusicAtBeat * (float)SecondsPerBeat;
                Debug.Log("started early at " + playbackPos + " sec");
            }
#endif

            musicAudioSource.time = playbackPos;
            musicAudioSource.Play();
            playMusicFileNextBeat = false;
        }
        delayTimer += deltaTime;
        beatTimer += deltaTime;
        audioTimer += deltaTime;//congtinue trying to ensure that music will be synced later
        if (beatTimer > SecondsPerBeat)
        {
            //float timeInAudio = 
            delayTimer = -0.05f;
            beatTimer -= SecondsPerBeat;
            beatCounter++;
            beatCounter %= BeatsInMusic;
#if UNITY_EDITOR

            if ((debug_startMusicAtBeat <= -1 && beatCounter == 0) || beatCounter == debug_startMusicAtBeat)
            {
                playMusicFileNextBeat = true;
            }else
#endif
            if(beatCounter == 0)
            {
                playMusicFileNextBeat = true;
            }
        }
        if (delayTimer > 0 && delayTimer < 9999999f)
        {
            DoBeatActions();
            delayTimer = float.MaxValue;

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
        if (StartedMusic && Paused)
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
    IEnumerator BossIntroBeatFrames()
    {
        yield return new WaitForSeconds(.5f);
        DoBeatActions();
        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds((float)SecondsPerBeat);
            DoBeatActions();
        }
    }
    public static void SetVolume(float volume)
    {
        if(instance != null)
        {
            instance.musicAudioSource.volume = volume;
        }
    }
    private void DoBeatActions()
    {
        for (int i = 0; i < syncableObjs.Length; i++)
        {
            syncableObjs[i].DoMusicSyncedAction();
        }
        boss.beatFrame = true;
        discoTileMaterialAsset.SetFloat(discoTileMaterialFlipColFloatHash, beatCounter % 2);
        for (int i = 0; i < tileRenderers.Length; i++)
        {
            tileRenderers[i].material.SetFloat(discoTileMaterialFlipColFloatHash, beatCounter % 2);
        }
    }
}
