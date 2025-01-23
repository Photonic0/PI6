using Assets.Common.Consts;
using Assets.Common.Systems;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
//doing this because gamemanager, uimanager etc are DontDestroyOnLoad
//and these objects are scene-specific.
//don't use this for UI elements. use script on canvas prefab instead.
public class MultiSceneSingletonPopulator : MonoBehaviour
{
    [SerializeField] Checkpoint[] checkpoints;
    [SerializeField] GameObject[] permanentlyDestroyableObjs;
    private void Awake()
    {
#if UNITY_EDITOR
        if (GameManager.instance == null)
        {
            SceneManager.LoadScene(SceneIndices.MainMenu);
            Debug.ClearDeveloperConsole();
        }
#endif
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] != null)
            {
                checkpoints[i].index = i;
            }
        }
        LevelInfo.checkpoints = checkpoints;
        LevelInfo.permanentlyDestroyableObjs = permanentlyDestroyableObjs;
        if (LevelInfo.permanentlyDestroyedObjsFlags == null)
        {
            LevelInfo.permanentlyDestroyedObjsFlags = new bool[permanentlyDestroyableObjs.Length];
        }
        else
        {
            for (int i = 0; i < LevelInfo.permanentlyDestroyedObjsFlags.Length; i++)
            {
                if (LevelInfo.permanentlyDestroyedObjsFlags[i])
                {
                    Destroy(permanentlyDestroyableObjs[i]);
                }
            }
        }
        StartCoroutine(RespawnOnEndOfFrame());
        Destroy(gameObject, 1f);
    }
   
    IEnumerator RespawnOnEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        //needed to do this here because wasn't working in player life script for some reason????
        if (LevelInfo.latestCheckpointIndex != -1)
        {
            LevelInfo.LatestCheckpoint.RespawnAt();
        }
    }
}
