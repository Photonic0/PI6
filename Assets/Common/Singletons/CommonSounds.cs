using Assets.Helpers;
using UnityEngine;

public class CommonSounds : MonoBehaviour
{
    static CommonSounds instance;
    [SerializeField] AudioClip buttonClick;
    [SerializeField] AudioClip deathBig;
    [SerializeField] AudioClip gateOpen;
    public static AudioClip[] WoodenFootsteps => instance.footSteps;
    [SerializeField] AudioClip[] footSteps;
    [SerializeField] AudioClip commonShot;
    [SerializeField] AudioClip throwSfx;
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
    public static void PlayRandom(AudioClip[] clips, AudioSource dedicatedSource, float pitchMultiplier = 1)
    {
        dedicatedSource.volume = .5f;
        AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];
        dedicatedSource.pitch = Random2.Float(.85f, 1.15f) * pitchMultiplier;
        dedicatedSource.PlayOneShot(clipToPlay);
    }
    public static void PlayFootstep(AudioSource dedicatedSource)
    {
        dedicatedSource.volume = .5f;
        AudioClip clipToPlay = instance.footSteps[Random.Range(0, instance.footSteps.Length)];
        dedicatedSource.pitch = Random2.Float(.85f, 1.15f);
        dedicatedSource.PlayOneShot(clipToPlay);
    }
    public static void Play(AudioClip audio, AudioSource source, float volume = 1, float pitch = 1)
    {
        source.volume = volume;
        source.pitch = pitch;
        source.PlayOneShot(audio);
    }
    public static void PlayDeathBig(AudioSource source)
    {
        source.volume = 1;
        source.PlayOneShot(instance.deathBig);
    }
    //might add random variation
    public static void PlayButtonSound(AudioSource source)
    {
        source.volume = 1;
        source.PlayOneShot(instance.buttonClick);
    }
    public static void PlayCommonShotSound(AudioSource source)
    {
        source.PlayOneShot(instance.commonShot);
    }
    public static void PlayThrowSound(AudioSource source)
    {
        source.PlayOneShot(instance.throwSfx);
    }
    public static void PlayGateOpen(AudioSource source)
    {
        source.pitch = 1.4f;
        source.PlayOneShot(instance.gateOpen);
    }
}
