using Assets.Common.Consts;
using Assets.Helpers;
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
    public AudioClip[] hardwoodHit;
    [SerializeField] TileBase middleTile;
    [SerializeField] TileBase[] animatedTilesForVariation;
    private void Awake()
    {
        SceneManager.sceneUnloaded += UnloadSingleton;
        instance = this;
        BoundsInt tilemapBounds = solidTiles.cellBounds;

        int maxI = tilemapBounds.xMax - 1;
        int maxJ = tilemapBounds.yMax - 1; 
        int minI = tilemapBounds.xMin;
        int minJ = tilemapBounds.yMin;
        int amountOfVariationTiles = animatedTilesForVariation.Length;
        for (int i = minI; i < maxI; i++)
        {
            for (int j = minJ; j < maxJ; j++)
            {
                Vector3Int pos = new(i, j, 0);
                TileBase tile = solidTiles.GetTile(pos);
                if (tile == middleTile && Random2.XInY(amountOfVariationTiles - 1, amountOfVariationTiles))
                {                       
                    solidTiles.SetTile(pos, animatedTilesForVariation[Random.Range(0, amountOfVariationTiles)]);
                }
            }
        }
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
