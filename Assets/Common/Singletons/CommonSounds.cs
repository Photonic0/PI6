using Assets.Helpers;
using UnityEngine;

public class CommonSounds : MonoBehaviour
{
    static CommonSounds instance;
    [SerializeField] AudioClip uiConfirm;
    [SerializeField] AudioClip uiChange;
    [SerializeField] AudioClip deathBig;
    [SerializeField] AudioClip gateOpen;
    public static AudioClip[] Footstep => instance.footSteps;
    [SerializeField] AudioClip[] footSteps;
    [SerializeField] AudioClip[] spikeStageSteps;
    [SerializeField] AudioClip[] discoStageSteps;
    [SerializeField] AudioClip[] typhoonStageSteps;
    [SerializeField] AudioClip commonShot;
    [SerializeField] AudioClip throwSfx;
    [SerializeField] AudioClip bwow;
    [SerializeField] AudioClip snareReverb;
    [SerializeField] AudioClip electronicSnare;
    [SerializeField] AudioClip electronicKick;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public static void PlayRandom(AudioClip[] clips, AudioSource dedicatedSource, float pitchMultiplier = 1, float volumeMultiplier = 0.5f)
    {
        dedicatedSource.volume = volumeMultiplier * Settings.sfxVolume;
        AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];
        dedicatedSource.pitch = Random2.Float(.85f, 1.15f) * pitchMultiplier;
        dedicatedSource.PlayOneShot(clipToPlay);
    }

    public static void PlayFootstep(AudioSource dedicatedSource)
    {
        dedicatedSource.volume = .5f * Settings.sfxVolume;
        AudioClip clipToPlay = instance.footSteps[Random.Range(0, instance.footSteps.Length)];
        dedicatedSource.pitch = Random2.Float(.85f, 1.15f);
        dedicatedSource.PlayOneShot(clipToPlay);
    }
    public static void Play(AudioClip audio, AudioSource source, float volume = 1, float pitch = 1)
    {
        source.volume = volume * Settings.sfxVolume;
        source.pitch = pitch;
        source.PlayOneShot(audio);
    }
    public static void PlayDeathBig(AudioSource source)
    {
        source.volume = Settings.sfxVolume;
        source.PlayOneShot(instance.deathBig);
    }
    public static void PlayUIConfirm()
    {
        UIManager.AudioSource.volume = Settings.sfxVolume;
        UIManager.AudioSource.PlayOneShot(instance.uiConfirm);
    }
    public static void PlayUIChange()
    {
        UIManager.AudioSource.volume = Settings.sfxVolume;
        UIManager.AudioSource.PlayOneShot(instance.uiChange);
    }
    public static void PlayCommonShotSound(AudioSource source)
    {
        source.volume = Settings.sfxVolume;
        source.PlayOneShot(instance.commonShot);
    }
    public static void PlayThrowSound(AudioSource source)
    {
        source.volume = Settings.sfxVolume;
        source.PlayOneShot(instance.throwSfx);
    }
    public static void PlayGateOpen(AudioSource source)
    {
        source.pitch = 1.4f;
        source.volume = Settings.sfxVolume;
        source.PlayOneShot(instance.gateOpen);
    }
    public static void LoadSpikeStageFootsteps()
    {
        instance.footSteps = instance.spikeStageSteps;
    }
    public static void LoadDiscoStageFootsteps()
    {
        instance.footSteps = instance.discoStageSteps;
    }
    public static void LoadTyphoonStageFootsteps()
    {
        if (instance.typhoonStageSteps != null && instance.typhoonStageSteps.Length > 0)
        {
            instance.footSteps = instance.typhoonStageSteps;
        }
        else
        {
            instance.footSteps = instance.spikeStageSteps;
        }
    }
    public static void PlaySnareReverb(AudioSource source, float pitchMultiplier = 1f, float volumeMultiplier = 1f)
    {
        source.pitch = pitchMultiplier;
        source.volume = volumeMultiplier * Settings.sfxVolume;
        source.PlayOneShot(instance.snareReverb);
    }
    public static void PlaySnare(AudioSource source, float pitchMultiplier = 1f, float volumeMultiplier = 1f)
    {
        source.pitch = pitchMultiplier;
        source.volume = volumeMultiplier * Settings.sfxVolume;
        source.PlayOneShot(instance.electronicSnare);
    }
    public static void PlayBwow(AudioSource source, float pitchMultiplier = 1f, float volumeMultiplier = 1f)
    {
        source.pitch = pitchMultiplier;
        source.volume = volumeMultiplier * Settings.sfxVolume;
        source.PlayOneShot(instance.bwow);
    }
    public static void PlayKick(AudioSource source, float pitchMultiplier = 1f, float volumeMultiplier = 1f)
    {
        source.pitch = pitchMultiplier;
        source.volume = volumeMultiplier * Settings.sfxVolume;
        source.PlayOneShot(instance.electronicKick);
    }
}
