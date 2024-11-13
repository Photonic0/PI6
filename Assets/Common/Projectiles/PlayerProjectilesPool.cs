using Assets.Helpers;
using UnityEngine;

public class PlayerProjectilesPool : MonoBehaviour
{
    static PlayerProjectilesPool instance;
    [SerializeField] DiscoShot[] discoShots;
    [SerializeField] SpikeShot[] spikeShots;
    [SerializeField] TyphoonShot[] typhoonShots;
    [SerializeField] BasicShot[] basicShots;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public static bool GetDiscoShot(out DiscoShot proj)
    {
        if(Helper.TryFindFreeIndex(instance.discoShots, out int index))
        {
            proj = instance.discoShots[index];
            return true;
        }
        proj = null;
        return false;
    }
    public static bool GetBasicShot(out BasicShot proj)
    {
        if (Helper.TryFindFreeIndex(instance.basicShots, out int index))
        {
            proj = instance.basicShots[index];
            return true;
        }
        proj = null;
        return false;
    }
    public static bool GetSpikeShot(out SpikeShot proj)
    {
        if (Helper.TryFindFreeIndex(instance.spikeShots, out int index))
        {
            proj = instance.spikeShots[index];
            return true;
        }
        proj = null;
        return false;
    }
    public static bool GetTyphoonShot(out TyphoonShot proj)
    {
        if (Helper.TryFindFreeIndex(instance.typhoonShots, out int index))
        {
            proj = instance.typhoonShots[index];
            return true;
        }
        proj = null;
        return false;
    }
}
