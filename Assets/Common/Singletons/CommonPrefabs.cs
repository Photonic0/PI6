using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonPrefabs : MonoBehaviour
{
    static CommonPrefabs instance;
    public static GameObject RestoreDrop => instance.restoreDrop;
    [SerializeField] GameObject restoreDrop;
    public static GameObject BasicShot => instance.basicShot;
    [SerializeField] GameObject basicShot;
    public static GameObject TyphoonShot => instance.typhoonShot;
    [SerializeField] GameObject typhoonShot;
    public static GameObject DiscoShot => instance.discoShot;
    [SerializeField] GameObject discoShot;
    public static GameObject SpikeShot => instance.spikeShot;
    [SerializeField] GameObject spikeShot;
    public static GameObject DeathParticles => instance.deathParticles;
    [SerializeField] GameObject deathParticles;
    public static GameObject ExplosionBig => instance.explosionBig;
    [SerializeField] GameObject explosionBig;
    public static GameObject ExplosionMedium => instance.explosionMedium;
    [SerializeField] GameObject explosionMedium;
    public static GameObject ExplosionSmall => instance.explosionSmall;
    [SerializeField] GameObject explosionSmall;
    public static GameObject SimpleLightningLineRenderer => instance.simpleLightningLineRenderer;
    [SerializeField] GameObject simpleLightningLineRenderer;
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
