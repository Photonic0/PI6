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
#if UNITY_EDITOR
    //                        oxo       xoo         ooo         ooo             ooo         ooo             ooo         oox
    //                        ooo       ooo         xoo         ooo             ooo         ooo             oox         ooo
    //                        ooo       ooo         ooo         xoo             oxo         oox             ooo         ooo
    [SerializeField] TileBase tileTop, tileTopLeft, tileLeft, tileBottomLeft, tileBottom, tileBottomRight, tileRight, tileTopRight;
    //                          o                   x                   o
    //                          x                   o                   o
    //                          o                   o                   x
    [SerializeField] TileBase tilePillarRepeat, tilePillarUpperCap, tilePillarBottomCap;
    [SerializeField] TileBase tileRowRepeat, tileRowLeftCap, tileRowRightCap;
    [SerializeField] TileBase tileSingle;
#endif
    [SerializeField] Tile backTile;
    [SerializeField] TileBase backTileBottomLeft, backTileBottom, backTileBottomRight, backTilePillarBottomCap, backTileRowLeftCap, backTileRowRepeat, backTileRowRightCap;
    [SerializeField] Tilemap backLayer;


    private void Awake()
    {
        SceneManager.sceneUnloaded += UnloadSingleton;
        instance = this;
        int amountOfVariationTiles = animatedTilesForVariation.Length;

        BoundsInt tilemapBounds = solidTiles.cellBounds;
        GetTilePresenceAndBounds(tilemapBounds, out int maxI, out int maxJ, out int minI, out int minJ, out bool[,] tilePresence);
        for (int i = minI; i < maxI; i++)
        {
            for (int j = minJ; j < maxJ; j++)
            {
                Vector3Int pos = new(i, j, 0);
                TileBase tile = solidTiles.GetTile(pos);
                if (tile == middleTile && Random2.XInY(amountOfVariationTiles - 1, amountOfVariationTiles)/* && IndexClamped(tilePresence, i, j - 1)*/)
                {
                    solidTiles.SetTile(pos, animatedTilesForVariation[Random.Range(0, amountOfVariationTiles)]);
                }
            }
        }
        SwingingSpikeBall[] balls = FindObjectsOfType<SwingingSpikeBall>();
        if (balls != null)
        {
            for (int i = 0; i < balls.Length; i++)
            {
                Vector3 anchorPos = balls[i].transform.parent.position;
                anchorPos.y += 1;
                Vector3Int pos = new((int)anchorPos.x, (int)anchorPos.y);
                solidTiles.SetTile(pos, middleTile);
                backLayer.SetTile(pos, backTile);
            }
        }
    }


    private void GetTilePresenceAndBounds(BoundsInt tilemapBounds, out int maxI, out int maxJ, out int minI, out int minJ, out bool[,] tilePresence)
    {
        maxI = tilemapBounds.xMax - 1;
        maxJ = tilemapBounds.yMax - 1;
        minI = tilemapBounds.xMin;
        minJ = tilemapBounds.yMin;
        tilePresence = new bool[maxI - minI, maxJ - minJ];
        for (int i = minI; i < maxI; i++)
        {
            for (int j = minJ; j < maxJ; j++)
            {
                Vector3Int pos = new(i, j, 0);
                tilePresence[i - minI, j - minJ] = solidTiles.HasTile(pos);
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
    static bool IndexClamped(bool[,] array, int i, int j)
    {
        if (i < 0)
        {
            i = 0;
        }
        if (j < 0)
        {
            j = 0;
        }
        if (i >= array.GetLength(0))
        {
            i = array.GetLength(0) - 1;
        }
        if (j >= array.GetLength(1))
        {
            j = array.GetLength(1) - 1;
        }
        return array[i, j];
    }
#if UNITY_EDITOR
    private void TileFraming(int maxI, int maxJ, int minI, int minJ, bool[,] tilePresence)
    {
        for (int i = minI; i < maxI; i++)
        {
            for (int j = minJ; j < maxJ; j++)
            {
                int centeredI = i - minI;
                int centeredJ = j - minJ;
                bool top = IndexClamped(tilePresence, centeredI, centeredJ + 1);
                bool right = IndexClamped(tilePresence, centeredI + 1, centeredJ);
                bool bottom = IndexClamped(tilePresence, centeredI, centeredJ - 1);
                bool left = IndexClamped(tilePresence, centeredI - 1, centeredJ);
                bool current = tilePresence[centeredI, centeredJ];
                if (!current)
                    continue;
                Vector3Int pos = new(i, j, 0);
                TileFraming_Inner(top, right, bottom, left, pos);
            }
        }
    }
    private void TileFraming_Inner(bool top, bool right, bool bottom, bool left, Vector3Int pos)
    {
        if (solidTiles.HasTile(pos))
        {
            backLayer.SetTile(pos, backTile);
        }
        else
        {
            backLayer.SetTile(pos, null);
        }
        if (top && bottom)
        {
            if (left && right)
            {
                solidTiles.SetTile(pos, middleTile);  // All sides connected
                return;
            }
            if (left)
            {
                solidTiles.SetTile(pos, tileRight);  // Top, bottom, left only
                return;
            }
            if (right)
            {
                solidTiles.SetTile(pos, tileLeft);  // Top, bottom, right only
                return;
            }
            solidTiles.SetTile(pos, tilePillarRepeat);  // Top and bottom only
            return;
        }

        if (top)
        {
            if (left && right)
            {
                solidTiles.SetTile(pos, tileBottom);  // Top, left, right
                backLayer.SetTile(pos, backTileBottom);
                return;
            }
            if (left)
            {
                solidTiles.SetTile(pos, tileBottomRight);  // Top, left only
                backLayer.SetTile(pos, backTileBottomRight);
                return;
            }
            if (right)
            {
                solidTiles.SetTile(pos, tileBottomLeft);  // Top, right only
                backLayer.SetTile(pos, backTileBottomLeft);
                return;
            }
            solidTiles.SetTile(pos, tilePillarBottomCap);  // Top only
            backLayer.SetTile(pos, backTilePillarBottomCap);

            return;
        }

        if (bottom)
        {
            if (left && right)
            {
                solidTiles.SetTile(pos, tileTop);  // Bottom, left, right
                return;
            }
            if (left)
            {
                solidTiles.SetTile(pos, tileTopRight);  // Bottom, left only
                return;
            }
            if (right)
            {
                solidTiles.SetTile(pos, tileTopLeft);  // Bottom, right only
                return;
            }
            solidTiles.SetTile(pos, tilePillarUpperCap);
            return;  // Bottom only
        }

        if (left && right)
        {
            solidTiles.SetTile(pos, tileRowRepeat);  // Left and right only
            backLayer.SetTile(pos, backTileRowRepeat);
            return;
        }
        if (left)
        {
            solidTiles.SetTile(pos, tileRowRightCap);  // Left only
            backLayer.SetTile(pos, backTileRowRightCap);
            return;
        }
        if (right)
        {
            solidTiles.SetTile(pos, tileRowLeftCap);  // Right only
            backLayer.SetTile(pos, backTileRowLeftCap);
            return;
        }
    }
    private void TileFramingAndBallTweaks(out int maxI, out int maxJ, out int minI, out int minJ)
    {
        BoundsInt tilemapBounds = solidTiles.cellBounds;
        GetTilePresenceAndBounds(tilemapBounds, out maxI, out maxJ, out minI, out minJ, out bool[,] tilePresence);
        TileFraming(maxI, maxJ, minI, minJ, tilePresence);
        SwingingSpikeBall[] balls = FindObjectsOfType<SwingingSpikeBall>();
        if (balls != null)
        {
            for (int i = 0; i < balls.Length; i++)
            {
                Vector3 anchorPos = balls[i].transform.parent.position;
                anchorPos.y += 1;
                Vector3Int pos = new((int)anchorPos.x, (int)anchorPos.y);
                solidTiles.SetTile(pos, middleTile);
                backLayer.SetTile(pos, backTile);
            }
        }
    }
  [SerializeField] bool debug_AutoFrame = false;
    private void OnDrawGizmos()
    {
        if (debug_AutoFrame)
        {
            debug_AutoFrame = false;
            TileFramingAndBallTweaks(out _, out _, out _, out _);
        }
    }
#endif

}
