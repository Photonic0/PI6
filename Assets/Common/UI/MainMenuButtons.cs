using Assets.Common.Consts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    [SerializeField] AudioSource cameraAudioSource;
    public void LoadSpikeStage()
    {
        GameManager.CleanupCheckpoints();
        CommonSounds.PlayButtonSound(cameraAudioSource);
        SceneManager.LoadScene(SceneIndices.SpikeStage);
    }
    public void LoadDiscoStage()
    {
        GameManager.CleanupCheckpoints();
        CommonSounds.PlayButtonSound(cameraAudioSource);
        SceneManager.LoadScene(SceneIndices.DiscoStage);
    }
    public void LoadTyphoonStage()
    {

        GameManager.CleanupCheckpoints();
        CommonSounds.PlayButtonSound(cameraAudioSource);
        SceneManager.LoadScene(SceneIndices.TyphoonStage);
    }
}
