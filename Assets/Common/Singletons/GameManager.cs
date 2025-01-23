using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Common.Interfaces;
using Assets.Common.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool Paused { get; private set; }
#if UNITY_EDITOR
    [SerializeField] int debug_FPS = -1;
    [SerializeField] float debug_timeScale = 1;
    [SerializeField] bool debug_addTinyShake = false;
    [SerializeField] bool debug_addSmallShake = false;
    [SerializeField] bool debug_addMediumShake = false;
    [SerializeField] bool debug_addLargeShake = false;
#endif
    public static GameManager instance;
    public PlayerControl playerControl;
    public static PlayerControl PlayerControl => instance.playerControl;
    public PlayerRenderer playerRenderer;
    public static PlayerRenderer PlayerRenderer => instance.playerRenderer;
    public PlayerLife playerLife;
    public static PlayerLife PlayerLife => instance.playerLife;
    public static Vector3 PlayerPosition => instance.playerControl.Position;

    public static Camera CurrentCam => instance.currentCam;
    Camera currentCam;

    public static Transform CurrentCamTransform => instance.currentCamTransform;
    Transform currentCamTransform;
    [SerializeField] List<IUpdatableWhenPaused> thingsToUpdateWhenPaused;

    private void Awake()//assign player scripts refs in awake of player scripts so it goes fine when changing scenes
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            LevelInfo.latestCheckpointIndex = -1;
            PlayerLife.chances = PlayerLife.StartingChances;
            instance = this;
            thingsToUpdateWhenPaused = new();
            Paused = false;
            DontDestroyOnLoad(gameObject);
            currentCam = Camera.main;
        }
    }
    public static void AssignCameraVars(Camera camera)
    {
        if (camera != null)
        {
            instance.currentCam = camera;
            instance.currentCamTransform = camera.transform;
        }
    }
    private void Update()
    {
#if UNITY_EDITOR
        Application.targetFrameRate = debug_FPS;
        if (!Paused)
        {
            Time.timeScale = debug_timeScale;
        }
        if (debug_addTinyShake)
        {
            debug_addTinyShake = false;
            ScreenShakeManager.AddTinyShake();
        }
        if (debug_addSmallShake)
        {
            debug_addSmallShake = false;
            ScreenShakeManager.AddSmallShake();
        }
        if (debug_addMediumShake)
        {
            debug_addMediumShake = false;
            ScreenShakeManager.AddMediumShake();
        }
        if (debug_addLargeShake)
        {
            debug_addLargeShake = false;
            ScreenShakeManager.AddLargeShake();
        }
#endif

    }

    public static event UnityAction OnPause;
    public static event UnityAction OnUnPause;
    public static void PauseGame()
    {
        Paused = true;
        Time.timeScale = 0;
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        if (buildIndex == SceneIndices.DiscoStage)
        {
            DiscoMusicEventManager.PauseMusic();
        }
        instance.StartCoroutine(instance.UpdatePausedObjects());
        if (OnPause != null)
        {
            OnPause();
        }
    }
    IEnumerator UpdatePausedObjects()
    {
        float unscaledDT = 1f / 30f;//30 fps

        while (Paused)
        {
            for (int i = 0; i < thingsToUpdateWhenPaused.Count; i++)
            {
                if (thingsToUpdateWhenPaused[i].GameObject == null)
                {
                    thingsToUpdateWhenPaused.RemoveAt(i);
                    //reduce i and continue so that
                    //if there are 2 null or destroyed objs in a row
                    //it won't cause issues
                    i--;
                    continue;
                }
                thingsToUpdateWhenPaused[i].PausedUpdate(unscaledDT);
            }
            yield return new WaitForSecondsRealtime(unscaledDT);//30 fps updating
        }
    }
    internal static void UnpauseGame()
    {
        Paused = false;
        Time.timeScale = 1;
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        if (buildIndex == SceneIndices.DiscoStage)
        {
            DiscoMusicEventManager.UnPauseMusic();
        }
        if (OnUnPause != null)
        {
            OnUnPause();
        }
    }
    public static void CleanupDestroyedPausedUpdatedObjs()
    {
        for (int i = 0; i < instance.thingsToUpdateWhenPaused.Count; i++)
        {
            if (instance.thingsToUpdateWhenPaused[i] == null)
            {
                instance.thingsToUpdateWhenPaused.RemoveAt(i);
                i--;
            }
        }
    }
    public static void AddToPausedUpatedObjs(IUpdatableWhenPaused obj)
    {
        if (!instance.thingsToUpdateWhenPaused.Contains(obj))
        {
            instance.thingsToUpdateWhenPaused.Add(obj);
        }
    }
    public static void UpdatePermanentlyDestroyedObjFlagsAndDestroyObj(GameObject objToBeDestroyed, float destructionDelay = 1f)
    {
        for (int i = 0; i < LevelInfo.permanentlyDestroyableObjs.Length; i++)
        {
            //it should have a permanently destroyed flag IF
            //A. it is either about to be destroyed by this function (objToCheckAndDestroy == instance.permanentlyDestroyableObjs[i])
            //B. it has already been destroyed (instance.permanentlyDestroyableObjs[i] == null)
            if (objToBeDestroyed == LevelInfo.permanentlyDestroyableObjs[i] || LevelInfo.permanentlyDestroyableObjs[i] == null)
            {
                LevelInfo.permanentlyDestroyedObjsFlags[i] = true;
            }
        }

        Destroy(objToBeDestroyed, destructionDelay);
    }
}
