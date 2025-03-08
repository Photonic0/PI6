using UnityEngine;

public class DiscoStageSingleton : MonoBehaviour
{
    static DiscoStageSingleton instance;
    public static DiscoStageSingleton Instance = instance;
    public static AudioClip BwowLite => instance.bwowLite;
    [SerializeField] AudioClip bwowLite;


    private void Awake()
    {
        instance = this;
    }
    private void OnDestroy()
    {
        instance = null;
    }
    public static void PlayBwowLite(AudioSource audioSource)
    {
        audioSource.pitch = 1f;
        audioSource.volume = 1f * Settings.sfxVolume;
        audioSource.PlayOneShot(instance.bwowLite);
    }
}
 