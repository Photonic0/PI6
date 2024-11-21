using Assets.Common.Consts;
using Assets.Common.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
//doing this because gamemanager, uimanager etc are DontDestroyOnLoad
//and these objects are scene-specific.
//don't use this for UI elements. use script on canvas prefab instead.
public class MultiSceneSingletonPopulator : MonoBehaviour
{
   
    [SerializeField] Checkpoint[] checkpoints;

    private void Awake()
    {
#if UNITY_EDITOR
        if (GameManager.instance == null)
        {
            SceneManager.LoadScene(SceneIndices.MainMenu);
            return;
        }
#endif

        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].index = i;
        }
        GameManager.checkpoints = checkpoints;

        //needed to do this here because wasn't working in player life script for some reason????
        if (GameManager.latestCheckpointIndex != -1)
        {
            GameManager.LatestCheckpoint.RespawnAt();
        }
    }
}
