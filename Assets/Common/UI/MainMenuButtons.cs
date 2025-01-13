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

    public void LoadSpikeStage()
    {
        StartCoroutine(LoadSpikeStageInner());
    }
    IEnumerator LoadSpikeStageInner()
    {
        CommonSounds.PlayButtonSound(cameraAudioSource);
        GameManager.CleanupCheckpoints();
        CommonSounds.LoadSpikeStageFootsteps();
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.SpikeStage);
        loadStatus.allowSceneActivation = false;

        while (loadStatus.progress < 0.9f) // Scene is loaded at 90% but not activated
        {
            //loadBar.fillAmount = loadStatus.progress;
            yield return new WaitForFixedUpdate();
        }

        // Scene is loaded, now activate it
       // loadBar.fillAmount = 1f;
        loadStatus.allowSceneActivation = true;
        yield return null;
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
        loadBar.value += 0.0125f;
        yield return null;
        GameManager.CleanupCheckpoints();
        loadBar.value += 0.0125f;
        yield return null;
        CommonSounds.LoadDiscoStageFootsteps();
        loadBar.value += 0.0125f;
        yield return null;
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
        yield return null;
    }

    public void LoadTyphoonStage()
    {
        StartCoroutine(LoadTyphoonStageInner());
    }

    IEnumerator LoadTyphoonStageInner()
    {
        CommonSounds.PlayButtonSound(cameraAudioSource);
        GameManager.CleanupCheckpoints();
        CommonSounds.LoadTyphoonStageFootsteps();
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.TyphoonStage);
        loadStatus.allowSceneActivation = false;

        while (loadStatus.progress < 0.9f) // Scene is loaded at 90% but not activated
        {
           // loadBar.fillAmount = loadStatus.progress;
            yield return new WaitForFixedUpdate();
        }

        //loadBar.fillAmount = 1f;
        loadStatus.allowSceneActivation = true;
        yield return null;
    }

    public void LoadExplosiveStage()
    {
        StartCoroutine(LoadExplosiveStageInner());
    }

    IEnumerator LoadExplosiveStageInner()
    {
        CommonSounds.PlayButtonSound(cameraAudioSource);
        GameManager.CleanupCheckpoints();
        //CommonSounds.LoadTyphoonStageFootsteps();
        AsyncOperation loadStatus = SceneManager.LoadSceneAsync(SceneIndices.ExplosiveStage);
        loadStatus.allowSceneActivation = false;

        while (loadStatus.progress < 0.9f) // Scene is loaded at 90% but not activated
        {
           // loadBar.fillAmount = loadStatus.progress;
            yield return new WaitForFixedUpdate();
        }

        // Scene is loaded, now activate it
       // loadBar.fillAmount = 1f;
        loadStatus.allowSceneActivation = true;
        yield return null;
    }

}
