using Assets.Common.Consts;
using Assets.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [SerializeField] AudioSource cameraAudioSource;
    [SerializeField] Slider loadBar;
#if UNITY_EDITOR
    [SerializeField] GameObject explosiveStageButton;
    private void Start()
    {
        explosiveStageButton.SetActive(true);
    }
#endif
    //ADD 1UP WHICH IS AN UPWARDS POINTING ARROW
    //ADD CHECKPOINT INDICATOR THAT IS EITHER SOME HOLOGRAPHIC PAD IDK
    //ADD A BIG LIFE UP WHICH IS A BATTERY


    public void LoadSpikeStage()
    {
        StartCoroutine(LoadSpikeStageInner());
    }
    IEnumerator LoadSpikeStageInner()
    {
        loadBar.gameObject.SetActive(true);
        loadBar.value = 0f;
        CommonSounds.PlayButtonSound(cameraAudioSource);
        LevelInfo.PrepareStageChange();
        CommonSounds.LoadSpikeStageFootsteps();
        LevelInfo.SetLevelColor(FlipnoteColors.ColorID.Yellow);
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.SpikeStage);
        loadStatus.allowSceneActivation = false;
        while (loadStatus.progress < 0.9f)
        {
            loadBar.value = Helper.Remap(loadStatus.progress, 0, 0.9f, 0.15f, 1f);
            yield return null;
        }
        loadBar.value = 1f;
        yield return null;
        loadStatus.allowSceneActivation = true;
    }

    public void LoadDiscoStage() //called by the button
    {
        StartCoroutine(LoadDiscoStageInner());
    }

    IEnumerator LoadDiscoStageInner()
    {
        loadBar.gameObject.SetActive(true);
        loadBar.value = 0f;
        CommonSounds.PlayButtonSound(cameraAudioSource);
        LevelInfo.PrepareStageChange();
        CommonSounds.LoadDiscoStageFootsteps();
        LevelInfo.SetLevelColor(FlipnoteColors.ColorID.Magenta);
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.DiscoStage);
        loadStatus.allowSceneActivation = false;
        while (loadStatus.progress < 0.9f)
        {
            loadBar.value = Helper.Remap(loadStatus.progress, 0, 0.9f, 0.15f, 1f);
            yield return null;
        }
        loadBar.value = 1f;
        yield return null;
        loadStatus.allowSceneActivation = true;
    }

    public void LoadTyphoonStage()
    {
        StartCoroutine(LoadTyphoonStageInner());
    }

    IEnumerator LoadTyphoonStageInner()
    {
        loadBar.gameObject.SetActive(true);
        loadBar.value = 0f;
        CommonSounds.PlayButtonSound(cameraAudioSource);
        LevelInfo.PrepareStageChange();
        CommonSounds.LoadTyphoonStageFootsteps();
        LevelInfo.SetLevelColor(FlipnoteColors.ColorID.Blue);
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.TyphoonStage);
        loadStatus.allowSceneActivation = false;
        while (loadStatus.progress < 0.9f)
        {
            loadBar.value = Helper.Remap(loadStatus.progress, 0, 0.9f, 0.15f, 1f);
            yield return null;
        }
        loadBar.value = 1f;
        yield return null;
        loadStatus.allowSceneActivation = true;
    }


    public void LoadExplosiveStage()
    {
        StartCoroutine(LoadExplosiveStageInner());
    }

    IEnumerator LoadExplosiveStageInner()
    {
        loadBar.gameObject.SetActive(true);
        loadBar.value = 0f;
        CommonSounds.PlayButtonSound(cameraAudioSource);
        LevelInfo.PrepareStageChange();
        //CommonSounds.LoadTyphoonStageFootsteps(); (if needed)
        LevelInfo.SetLevelColor(FlipnoteColors.ColorID.DarkGreen);
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.ExplosiveStage);
        loadStatus.allowSceneActivation = false;
        while (loadStatus.progress < 0.9f)
        {
            loadBar.value = Helper.Remap(loadStatus.progress, 0, 0.9f, 0.15f, 1f);
            yield return null;
        }
        loadBar.value = 1f;
        yield return null;
        loadStatus.allowSceneActivation = true;
    }


}
