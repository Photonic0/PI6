using Assets.Common.Consts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class SpikeStageSingleton : MonoBehaviour
{
    public static SpikeStageSingleton instance;
    public GameObject spikeWaveSpike;
    public AudioClip[] spikeBreak;
    public AudioClip spikeBreakNew;
    public AudioClip spikeShockwave;
    public Tilemap solidTiles;
    private void Awake()
    {
        SceneManager.sceneUnloaded += UnloadSingleton;
        instance = this;
    }

    private void UnloadSingleton(Scene arg0)
    {
        if (arg0.buildIndex == SceneIndices.SpikeStage)
        {
            if (instance != null)//for some reason instance is being null here already? strange...
            {
                instance.spikeWaveSpike = null;//do it just in case
            }
            instance = null;
            SceneManager.sceneUnloaded -= UnloadSingleton;
        }
    }
    
}
