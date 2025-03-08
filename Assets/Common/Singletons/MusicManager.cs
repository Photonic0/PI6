using Assets.Common.Consts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    const int MusicIDBoss = 0;
    const int MusicIDTyphoon = 1;
    const int MusicIDSpike = 2;
    const int MusicIDTitle = -1;
    int currentMusicIDPlaying = -1;
    [SerializeField] AudioClip spikeMusicStart;
    [SerializeField] AudioClip spikeMusicLoop;
    [SerializeField] AudioClip typhoonMusicStart;
    [SerializeField] AudioClip typhoonMusicLoop;
    [SerializeField] AudioClip bossMusicLoop;
    [SerializeField] AudioSource startAudioSource;
    [SerializeField] AudioSource loopAudioSource;
    [SerializeField] AudioClip titleScreenMusicStart;
    [SerializeField] AudioClip titleScreenMusicLoop;
    static MusicManager instance;
    public static bool PlayingBossMusic => instance.currentMusicIDPlaying == MusicIDBoss;
    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            StartMainMenuMusic();
            SceneManager.sceneLoaded += PlayMainMenuMusic;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void PlayMainMenuMusic(Scene arg0, LoadSceneMode arg1)
    {
        if(arg0.buildIndex == SceneIndices.MainMenu)
        {
            StartMainMenuMusic();
        }
    }

    public static void StopMusic()
    {
        instance.startAudioSource.Stop();
        instance.loopAudioSource.Stop();
    }
    public static void StartTyphoonMusic()
    {
        instance.startAudioSource.clip = instance.typhoonMusicStart;
        instance.loopAudioSource.clip = instance.typhoonMusicLoop;
        double startTime = AudioSettings.dspTime + 0.1;
        double introDuration = (double)instance.typhoonMusicStart.samples / instance.typhoonMusicStart.frequency;
        instance.startAudioSource.PlayScheduled(startTime);
        instance.loopAudioSource.PlayScheduled(startTime + introDuration);
        instance.currentMusicIDPlaying = MusicIDTyphoon;
        LevelInfo.currentLevelMusicID = MusicIDTyphoon;
        LevelInfo.currentLevelMusicStart = instance.typhoonMusicStart;
        LevelInfo.currentLevelMusicLoop = instance.typhoonMusicLoop;
    }
    public static void StartSpikeMusic()
    {
        instance.startAudioSource.clip = instance.spikeMusicStart;
        instance.loopAudioSource.clip = instance.spikeMusicLoop;
        double startTime = AudioSettings.dspTime + 0.1;
        double introDuration = (double)instance.spikeMusicStart.samples / instance.spikeMusicStart.frequency;
        instance.startAudioSource.PlayScheduled(startTime);
        instance.loopAudioSource.PlayScheduled(startTime + introDuration);
        instance.currentMusicIDPlaying = MusicIDSpike;
        LevelInfo.currentLevelMusicID = MusicIDSpike;
        LevelInfo.currentLevelMusicStart = instance.spikeMusicStart;
        LevelInfo.currentLevelMusicLoop = instance.spikeMusicLoop;
    }
    public static void StartBossMusic()
    {
        instance.startAudioSource.Stop();
        instance.loopAudioSource.Stop();
        instance.loopAudioSource.clip = instance.bossMusicLoop;
        instance.loopAudioSource.Play();
        instance.currentMusicIDPlaying = MusicIDBoss;
    }
    public static void StartMainMenuMusic()
    {
        instance.startAudioSource.clip = instance.titleScreenMusicStart;
        instance.loopAudioSource.clip = instance.titleScreenMusicLoop;
        double startTime = AudioSettings.dspTime + 0.2;
        double introDuration = (double)instance.titleScreenMusicStart.samples / instance.titleScreenMusicStart.frequency;
        instance.startAudioSource.PlayScheduled(startTime);
        instance.loopAudioSource.PlayScheduled(startTime + introDuration);
        instance.currentMusicIDPlaying = MusicIDTitle;
        LevelInfo.currentLevelMusicID = MusicIDTitle;
        LevelInfo.currentLevelMusicStart = null;
        LevelInfo.currentLevelMusicLoop = null;
    }
    public static void RestartCurrentLevelMusic()
    {
        if(LevelInfo.currentLevelMusicID == -2 || LevelInfo.currentLevelMusicStart == null || LevelInfo.currentLevelMusicLoop == null)
        {
            return;
        }
        AudioClip start = LevelInfo.currentLevelMusicStart;
        AudioClip loop = LevelInfo.currentLevelMusicLoop;
        instance.startAudioSource.clip = start;
        instance.loopAudioSource.clip = loop;
        double startTime = AudioSettings.dspTime + 0.1;
        double introDuration = (double)start.samples / start.frequency;
        instance.startAudioSource.PlayScheduled(startTime);
        instance.loopAudioSource.PlayScheduled(startTime + introDuration);
        instance.currentMusicIDPlaying = LevelInfo.currentLevelMusicID;
    }
    public static void SetMusicVolume(float volumeMult)
    {
        instance.startAudioSource.volume = volumeMult;
        instance.loopAudioSource.volume = volumeMult;
    }
}