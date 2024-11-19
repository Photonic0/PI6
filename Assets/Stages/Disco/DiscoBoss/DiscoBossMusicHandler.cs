using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DiscoBossMusicHandler : MonoBehaviour
{
    public const float BPM = 38;
    public const double SecondsPerBeat = 60.0 / BPM;
    public const int BeatsInMusic = 240;

    public bool StartedMusic = false;
    public bool Paused { get; private set; }
    public double beatTimer;
    public int beatCounter;
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioClip intro;
    [SerializeField] AudioClip music;
    [SerializeField] TilemapRenderer[] tileRenderers;
    [SerializeField] Material discoTileMaterialAsset;
    [SerializeField] TextMeshProUGUI debugtext;
    bool playMusicFileNextBeat;
    static readonly int discoTileMaterialFlipColFloatHash = Shader.PropertyToID("_FlipColFloat");
    private void Start()
    {
        DiscoMusicEventManager.instance.discoBossMusicHandler = this;
    }
    public void StartMusic()
    {
        musicAudioSource.PlayOneShot(intro);
        playMusicFileNextBeat = true;
        StartedMusic = true;
        DiscoMusicEventManager.PauseMusic();
    }
    void Update()
    {
        if (Paused || !StartedMusic)
            return;
        if(playMusicFileNextBeat)
        {
            musicAudioSource.PlayOneShot(music);
            playMusicFileNextBeat = false;
        }
        beatTimer += Time.deltaTime;
        if (beatTimer > SecondsPerBeat)
        {
            beatTimer -= SecondsPerBeat;
            beatCounter++;
            beatCounter %= BeatsInMusic;
            if(beatCounter == 0)
            {
                playMusicFileNextBeat = true;
            }
            StartCoroutine(WaitABitToActuallyDoActionAfterPlayingSplit());

        }
    }
    IEnumerator WaitABitToActuallyDoActionAfterPlayingSplit()
    {
        yield return new WaitForSecondsRealtime(.05f);
        int beatCounter = this.beatCounter - 1;//because the beat counter increased before this code executed
        discoTileMaterialAsset.SetFloat(discoTileMaterialFlipColFloatHash, beatCounter % 2);
        for (int i = 0; i < tileRenderers.Length; i++)
        {
            tileRenderers[i].material.SetFloat(discoTileMaterialFlipColFloatHash, beatCounter % 2);
        }
    }
}
