
using Assets.Common.Systems;
using UnityEngine;
using UnityEngine.UI;
//doing this because gamemanager, uimanager etc are DontDestroyOnLoad
//and these objects are scene-specific.
public class MultiSceneSingletonPopulator : MonoBehaviour
{
    [SerializeField] Image lifeBarOutline;
    [SerializeField] Image lifeBarFill;
    [SerializeField] Image lifeBarBack;
    [SerializeField] Image weaponBarOutline;
    [SerializeField] Image weaponBarFill;
    [SerializeField] Image weaponBarBack;
    [SerializeField] Image bossLifeBarOutline;
    [SerializeField] Image bossLifeBarFill;
    [SerializeField] Image bossLifeBarBack;
    [SerializeField] Checkpoint[] checkpoints;
    private void Awake()
    {
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

        UIManager.Instance.lifeBarOutline = lifeBarOutline;
        UIManager.Instance.lifeBarFill = lifeBarFill;
        UIManager.Instance.weaponBarOutline = weaponBarOutline;
        UIManager.Instance.weaponBarFill = weaponBarFill;
        UIManager.Instance.bossLifeBarOutline = bossLifeBarOutline;
        UIManager.Instance.bossLifeBarFill = bossLifeBarFill;
        UIManager.Instance.bossLifeBarBack = bossLifeBarBack;
        UIManager.Instance.lifeBarBack = lifeBarBack;
        UIManager.Instance.weaponBarBack = weaponBarBack;
    }
}
