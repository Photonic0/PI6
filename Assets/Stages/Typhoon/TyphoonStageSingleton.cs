using Assets.Common.Consts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
public class TyphoonStageSingleton : MonoBehaviour
{
    public static TyphoonStageSingleton instance;
    public List<TyphoonEnemyCloud> cloudEnemies;
    public List<TyphoonEnemyLightningCloud> lightningCloudEnemies;
    public List<TyphoonEnemyTornado> tornadoEnemies;
    public AudioClip[] electricZaps;
    public AudioClip[] fanNoises;
    [SerializeField] bool playingFanNoise;
    [SerializeField] Transform[] fanNoiseSourcesTransforms;
    //there is one audio source game object in the array assigned in the inspector
    //done so that the first entry in the array can be used instead of a prefab reference
    [SerializeField] AudioSource[] fanNoiseAudioSources;
    //instantiate one audio source per bounds and clamp each one within one of the bounds
    [SerializeField] TyphoonHazardFan[] fans;
    (float minX, float maxX, float minY, float maxY)[] fanPositionBounds;
    public float fanNoiseTimer;
    public Tilemap solidTiles;

#if UNITY_EDITOR
    [SerializeField] TileBase middleTile;
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
    [SerializeField] bool debug_clickToFrameTiles;
#endif
    private void Awake()
    {
        SceneManager.sceneUnloaded += UnloadSingleton;
        instance = this;
        cloudEnemies = new(3);
        lightningCloudEnemies = new(5);
        tornadoEnemies = new(5);
        //InitializeFanBounds();
    }
    private void Update()
    {
        if (fanNoiseTimer < 0)
        {
            fanNoiseTimer = float.PositiveInfinity;
            playingFanNoise = false;
        }
        else
        {
            fanNoiseTimer -= Time.deltaTime;
        }
    }
    private void InitializeFanBounds()
    {
        List<(float minX, float maxX, float minY, float maxY)> fanPositionBoundsList = new(fans.Length);
        float padding = .1f;
        //initialize bounds list
        for (int i = 0; i < fanPositionBoundsList.Capacity; i++)
        {
            fanPositionBoundsList.Add(fans[i].GetWindBounds(padding));
        }
        //for every rectangle bounds,
        for (int i = 0; i < fanPositionBoundsList.Count; i++)
        {
            //check if it intersects another bounds
            for (int j = i + 1; j < fanPositionBoundsList.Count; j++)
            {
                if (j >= fanPositionBoundsList.Count)
                    break;

                (float minX, float maxX, float minY, float maxY) bounds1 = fanPositionBoundsList[i];
                (float minX, float maxX, float minY, float maxY) bounds2 = fanPositionBoundsList[j];
                //if bounds intersect, merge it with
                if (!(bounds1.maxX < bounds2.minX || bounds2.maxX < bounds1.minX || bounds1.maxY < bounds2.minY || bounds2.maxY < bounds1.minY))
                {
                    //remove the higher index first, then the lower one to avoid any issues caused by shifting indices
                    //j > i guaranteed
                    fanPositionBoundsList.RemoveAt(j);
                    fanPositionBoundsList.RemoveAt(i);
                    fanPositionBoundsList.Add(MergeBounds(bounds1, bounds2));
                    //j = i + 1;
                    i = 0;
                    break;
                }
            }
        }
        //shave off the padding to compensate for adding it earlier
        for (int i = 0; i < fanPositionBoundsList.Count; i++)
        {
            (float minX, float maxX, float minY, float maxY) bounds = fanPositionBoundsList[i];
            bounds.minX += padding;
            bounds.maxX -= padding;
            bounds.minY += padding;
            bounds.maxY -= padding;
            fanPositionBoundsList[i] = bounds;
        }
        fanPositionBounds = fanPositionBoundsList.ToArray();
        int amountOfAudioSources = fanPositionBounds.Length;
        Array.Resize(ref fanNoiseAudioSources, amountOfAudioSources);
        Array.Resize(ref fanNoiseSourcesTransforms, amountOfAudioSources);
        for (int i = 1; i < amountOfAudioSources; i++)
        {
            GameObject audioSrc = Instantiate(fanNoiseAudioSources[0].gameObject);
            fanNoiseAudioSources[i] = audioSrc.GetComponent<AudioSource>();
            fanNoiseSourcesTransforms[i] = audioSrc.transform;
        }
        UpdateFanNoiseAudioSourcesPositions();
    }

    (float minX, float maxX, float minY, float maxY) MergeBounds((float minX, float maxX, float minY, float maxY) bounds1, (float minX, float maxX, float minY, float maxY) bounds2)
    {
        return (
            Mathf.Min(bounds1.minX, bounds2.minX),
            Mathf.Max(bounds1.maxX, bounds2.maxX),
            Mathf.Min(bounds1.minY, bounds2.minY),
            Mathf.Max(bounds1.maxY, bounds2.maxY));
    }
    public static void AddToCloudEnemyList(TyphoonEnemyCloud enemyToAdd)
    {
        if (instance == null || instance.cloudEnemies == null || enemyToAdd == null)
            return;
        instance.cloudEnemies.Add(enemyToAdd);
    }
    public static void AddToLightningCloudEnemyList(TyphoonEnemyLightningCloud enemyToAdd)
    {
        if (instance == null || instance.lightningCloudEnemies == null || enemyToAdd == null)
            return;
        instance.lightningCloudEnemies.Add(enemyToAdd);
    }
    public static void AddToTornadoEnemyList(TyphoonEnemyTornado enemyToAdd)
    {
        if (instance == null || instance.tornadoEnemies == null || enemyToAdd == null)
            return;
        instance.tornadoEnemies.Add(enemyToAdd);
    }
    private void UnloadSingleton(Scene arg0)
    {
        if (arg0.buildIndex == SceneIndices.TyphoonStage)
        {
            if (!ReferenceEquals(instance, null))
            {
                instance.cloudEnemies = null;
                instance.lightningCloudEnemies = null;
                instance.fanNoiseAudioSources = null;
                instance.fanNoiseSourcesTransforms = null;
                instance.tornadoEnemies = null;
            }
            else if (ReferenceEquals(instance, this))
            {
                fanNoiseSourcesTransforms = null;
                fanNoiseAudioSources = null;
                lightningCloudEnemies = null;
                cloudEnemies = null;
                tornadoEnemies = null;
                instance = null;
            }
        }
        SceneManager.sceneUnloaded -= UnloadSingleton;
    }
    public static void RemoveCloudEnemyFromList(TyphoonEnemyCloud enemyToRemove)
    {
        if (ReferenceEquals(instance, null) || instance.cloudEnemies == null)
            return;
        instance.cloudEnemies.Remove(enemyToRemove);
    }
    public static void RemoveTornadoEnemyFromList(TyphoonEnemyTornado enemyToRemove)
    {
        if (ReferenceEquals(instance, null) || instance.tornadoEnemies == null)
            return;
        instance.tornadoEnemies.Remove(enemyToRemove);
    }
    public static void RemoveLightningCloudEnemyFromList(TyphoonEnemyLightningCloud enemyToRemove)
    {
        if (ReferenceEquals(instance, null) || instance.lightningCloudEnemies == null)
            return;
        instance.lightningCloudEnemies.Remove(enemyToRemove);
    }
    public static void StartFanNoise()
    {
        if (instance.playingFanNoise)
            return;
        instance.playingFanNoise = true;
        for (int i = 0; i < instance.fanNoiseAudioSources.Length; i++)
        {
            instance.fanNoiseAudioSources[i].clip = instance.fanNoises[UnityEngine.Random.Range(0, instance.fanNoises.Length)];
            instance.fanNoiseAudioSources[i].Play();
        }
        instance.fanNoiseTimer = Mathf.Max(1, instance.fanNoiseAudioSources[0].clip.length - 1);
    }
    void UpdateFanNoiseAudioSourcesPositions()
    {
        Vector3 playerPos = GameManager.PlayerPosition;
        for (int i = 0; i < instance.fanNoiseAudioSources.Length; i++)
        {
            instance.fanNoiseSourcesTransforms[i].position = ClampPosition(playerPos, instance.fanPositionBounds[i]);
        }
    }
    Vector3 ClampPosition(Vector3 position, (float minX, float maxX, float minY, float maxY) bounds) => new((float)Mathf.Clamp(position.x, bounds.minX, bounds.maxX), (float)Mathf.Clamp(position.y, bounds.minY, bounds.maxY), position.z);

#if UNITY_EDITOR
    private void Start()
    {
        Debug.ClearDeveloperConsole();
    }
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
                return;
            }
            if (left)
            {
                solidTiles.SetTile(pos, tileBottomRight);  // Top, left only
                return;
            }
            if (right)
            {
                solidTiles.SetTile(pos, tileBottomLeft);  // Top, right only
                return;
            }
            solidTiles.SetTile(pos, tilePillarBottomCap);  // Top only
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
            return;
        }
        if (left)
        {
            solidTiles.SetTile(pos, tileRowRightCap);  // Left only
            return;
        }
        if (right)
        {
            solidTiles.SetTile(pos, tileRowLeftCap);  // Right only
            return;
        }
    }
    static bool IndexClamped(bool[,] array, int i, int j)
    {
        //i = Mathf.Clamp(i, 0, array.GetLength(0) - 1);
        //j = Mathf.Clamp(j, 0, array.GetLength(1) - 1);
        if (i < 0)
        {
            return false;
        }
        if (j < 0)
        {
            return false;
        }
        if (i >= array.GetLength(0))
        {
            return false;
        }
        if (j >= array.GetLength(1))
        {
            return false;
        }
        return array[i, j];

    }
    private void GetTilePresenceAndBounds(BoundsInt tilemapBounds, out int maxI, out int maxJ, out int minI, out int minJ, out bool[,] tilePresence)
    {
        maxI = tilemapBounds.xMax;
        maxJ = tilemapBounds.yMax;
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

    private void OnDrawGizmos()
    {
        if (debug_clickToFrameTiles)
        {

            GetTilePresenceAndBounds(solidTiles.cellBounds, out int maxI, out int maxJ, out int minI, out int minJ, out bool[,] tilePresence);
            debug_clickToFrameTiles = false;
            TileFraming(maxI, maxJ, minI, minJ, tilePresence);
        }
    }
#endif

}
