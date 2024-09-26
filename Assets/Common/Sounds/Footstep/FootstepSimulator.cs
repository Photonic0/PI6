using UnityEngine;

public class FootstepSimulator
{
    readonly AudioSource footstepAudioSource;
    readonly AudioClip[] stepSounds;
    readonly float footstepRate;
    readonly float pitchMultiplier;
    float timer;
    public FootstepSimulator(AudioClip[] footstepSounds, float soundRate, AudioSource dedicatedAudioSource, float pitchMultiplier = 1)
    {
        timer = 0;
        footstepRate = soundRate;
        stepSounds = footstepSounds;
        footstepAudioSource = dedicatedAudioSource;
        this.pitchMultiplier = pitchMultiplier;

    }
    public void Update()//CALL THIS ON UPDATE OF BEHAVIOUR
    {
        if (timer > footstepRate)
        {
            timer %= footstepRate;
            CommonSounds.PlayRandom(stepSounds, footstepAudioSource, pitchMultiplier);
        }
        timer += Time.deltaTime;
    }
    public void CheckForPlaying()
    {
        if (timer > footstepRate)
        {
            timer %= footstepRate;
            CommonSounds.PlayRandom(stepSounds, footstepAudioSource, pitchMultiplier);
        }
    }
    public void IncreaseTimer()
    {
        timer += Time.deltaTime;
    }
    public void QueueSound() => timer = footstepRate + 0.0001f;
    public void ResetTimer() => timer = 0;
}
