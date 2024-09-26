using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonPrefabs : MonoBehaviour
{
    static CommonPrefabs instance;
    public static GameObject BasicShot => instance.basicShot;
    [SerializeField] GameObject basicShot;
    public static GameObject TyphoonShot => instance.typhoonShot;
    [SerializeField] GameObject typhoonShot;
    public static GameObject DeathParticles => instance.deathParticles;
    [SerializeField] GameObject deathParticles;
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
}
