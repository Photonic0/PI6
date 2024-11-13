using Assets.Common.Consts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TyphoonStageSingleton : MonoBehaviour
{
    public static TyphoonStageSingleton instance;
    public List<TyphoonEnemyCloud> cloudEnemies;
    private void Awake()
    {
        SceneManager.sceneUnloaded += UnloadSingleton;
        instance = this;
        cloudEnemies = new(3);
    }
    public static void AddToCloudEnemyArray(TyphoonEnemyCloud enemyToAdd)
    {
        if (instance == null || instance.cloudEnemies == null || enemyToAdd == null)
            return;
        instance.cloudEnemies.Add(enemyToAdd);
    }
    private void UnloadSingleton(Scene arg0)
    {
        if (arg0.buildIndex == SceneIndices.TyphoonStage)
        {
            if (instance != null)//for some reason instance is being null here already? strange...
            {
                instance.cloudEnemies = null;
            }
            cloudEnemies = null;
            instance = null;
            SceneManager.sceneUnloaded -= UnloadSingleton;
        }
    }
    public static void RemoveCloudEnemyFromList(TyphoonEnemyCloud enemyToRemove)
    {
        if (instance == null || instance.cloudEnemies == null || enemyToRemove == null)
            return;
        instance.cloudEnemies.Remove(enemyToRemove);
    } 
    //pergunta pro prof sobres os inimigos tornado
}
