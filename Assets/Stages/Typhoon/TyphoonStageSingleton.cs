using Assets.Common.Consts;
using Assets.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TyphoonStageSingleton : MonoBehaviour
{
    public static TyphoonStageSingleton instance;
    public List<TyphoonEnemyCloud> cloudEnemies;
    public List<TyphoonEnemyLightningCloud> lightningCloudEnemies;
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
    private void Awake()
    {
        SceneManager.sceneUnloaded += UnloadSingleton;
        instance = this;
        cloudEnemies = new(3);
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
    public static void AddToCloudEnemyArray(TyphoonEnemyCloud enemyToAdd)
    {
        if (instance == null || instance.cloudEnemies == null || enemyToAdd == null)
            return;
        instance.cloudEnemies.Add(enemyToAdd);
    }
    public static void AddToLightningCloudEnemyArray(TyphoonEnemyLightningCloud enemyToAdd)
    {
        if (instance == null || instance.lightningCloudEnemies == null || enemyToAdd == null)
            return;
        instance.lightningCloudEnemies.Add(enemyToAdd);
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
            }
            fanNoiseSourcesTransforms = null;
            fanNoiseAudioSources = null;
            lightningCloudEnemies = null;
            cloudEnemies = null;
            instance = null;
        }
        SceneManager.sceneUnloaded -= UnloadSingleton;
    }
    public static void RemoveCloudEnemyFromList(TyphoonEnemyCloud enemyToRemove)
    {
        if (instance == null || instance.cloudEnemies == null || enemyToRemove == null)
            return;
        instance.cloudEnemies.Remove(enemyToRemove);
    }
    public static void RemoveLightningCloudEnemyFromList(TyphoonEnemyLightningCloud enemyToRemove)
    {
        if (instance == null || instance.lightningCloudEnemies == null || enemyToRemove == null)
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
    void UpdateFanNoiseAudioSourcePosition()
    {
        int iterations = fanPositionBounds.Length;
        Vector2 playerPos = GameManager.PlayerPosition;
        float playerX = playerPos.x;
        float playerY = playerPos.y;
        float[] closestXs = new float[iterations];
        float[] closestYs = new float[iterations];
        for (int i = 0; i < iterations; i++)
        {
            (float minX, float maxX, float minY, float maxY) = fanPositionBounds[i];
            closestXs[i] = Mathf.Clamp(playerX, minX, maxX);
            closestYs[i] = Mathf.Clamp(playerY, minY, maxY);
        }
        float closestX = 99999f;
        float closestY = 99999f;
        float distCurrent = 9999999f;
        for (int i = 0; i < iterations; i++)
        {
            float xToCheck = closestXs[i];
            float yToCheck = closestYs[i];
            float deltaX = playerX - xToCheck;
            float deltaY = playerY - yToCheck;
            float dist = deltaX * deltaX + deltaY * deltaY;
            if (dist < distCurrent)
            {
                closestX = xToCheck;
                closestY = yToCheck;
                distCurrent = dist;
            }
        }
        //fanNoiseSourcePos.position = new Vector3(closestX, closestY, fanNoiseSourcePos.position.z);
    }
#if UNITY_EDITOR
    [SerializeField] bool debug_initializeFanBounds;
    private void OnDrawGizmos()
    {
        if (fanPositionBounds == null || debug_initializeFanBounds)
        {
            debug_initializeFanBounds = false;
            //InitializeFanBounds();
        }
        if (fanPositionBounds != null)
        {
            for (int i = 0; i < fanPositionBounds.Length; i++)
            {
                (float minX, float maxX, float minY, float maxY) = fanPositionBounds[i];
                Gizmos2.DrawRectangle(minX, maxX, minY, maxY, Color.green);
            }
        }
        //if(fanNoiseAudioSources != null)
        //{
        //    Gizmos.DrawCube(fanNoiseSourcePos.position, new Vector3(.2f, .2f, .2f));
        //}
    }
#endif
    //OLD FAN BOUNDS CALCULATION
    /*
     *   fanPositionBounds = new (float minX, float maxX, float minY, float maxY)[fans.Length];
        float padding = .5f;
        for (int i = 0; i < fanPositionBounds.Length; i++)
        {
            TyphoonHazardFan fan = fans[i];
            fanPositionBounds[i] = fan.GetWindBounds(padding);
        }
        List<(float minX, float maxX, float minY, float maxY)> fanPositionBoundsList = new(fans.Length);
        List<(int i, int j)> collisionsToNotCheck = new(fans.Length);
        for (int i = 0; i < fanPositionBounds.Length; i++)
        {
            for (int j = i; j < fanPositionBounds.Length; j++)
            {
            startOfJLoop:
                if (j == i)
                {
                    continue;
                }
                (float minX, float maxX, float minY, float maxY) bounds1 = fanPositionBounds[i];
                (float minX, float maxX, float minY, float maxY) bounds2 = fanPositionBounds[j];
                if (collisionsToNotCheck.Count > 1)
                {
                    for (int k = 0; k < collisionsToNotCheck.Count; k++)
                    {
                        if (collisionsToNotCheck[k] == (i, j))
                        {
                            i++;
                            goto startOfJLoop;
                        }
                    }
                }
                if (!(bounds1.maxX < bounds2.minX || bounds2.maxX < bounds1.minX || bounds1.maxY < bounds2.minY || bounds2.maxY < bounds1.minY))
                {
                    collisionsToNotCheck.Add((i, j));
                    (float minX, float maxX, float minY, float maxY) mergedBounds = new
                        (Mathf.Min(bounds1.minX, bounds2.minX), 
                        Mathf.Max(bounds1.maxX, bounds2.maxX),
                        Mathf.Min(bounds1.minY, bounds2.minY),
                        Mathf.Max(bounds1.maxY, bounds2.maxY));
                    fanPositionBoundsList.Add(bounds1);
                }
            }
        }
        fanPositionBounds = fanPositionBoundsList.ToArray();
     */

}
