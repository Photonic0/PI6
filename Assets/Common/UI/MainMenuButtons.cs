using Assets.Common.Consts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    [SerializeField] AudioSource cameraAudioSource;
    public void LoadSpikeStage()
    {
        GameManager.CleanupCheckpoints();
        CommonSounds.LoadSpikeStageFootsteps();
        CommonSounds.PlayButtonSound(cameraAudioSource);
        SceneManager.LoadScene(SceneIndices.SpikeStage);
    }
    public void LoadDiscoStage()
    {
        GameManager.CleanupCheckpoints();
        CommonSounds.LoadDiscoStageFootsteps();
        CommonSounds.PlayButtonSound(cameraAudioSource);
        SceneManager.LoadScene(SceneIndices.DiscoStage);
    }
    public void LoadTyphoonStage()
    {
        GameManager.CleanupCheckpoints();
        CommonSounds.LoadTyphoonStageFootsteps();
        CommonSounds.PlayButtonSound(cameraAudioSource);
        SceneManager.LoadScene(SceneIndices.TyphoonStage);
    }
}
