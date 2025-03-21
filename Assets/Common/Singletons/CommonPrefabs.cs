using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonPrefabs : MonoBehaviour
{
    static CommonPrefabs instance;
    public static GameObject RestoreDrop => instance.restoreDrop;
    [SerializeField] GameObject restoreDrop;
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
    public static GameObject FloatingText => instance.floatingText;
    [SerializeField] GameObject floatingText;
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
