using Assets.Helpers;
using UnityEngine;

public class DeathParticle : MonoBehaviour
{
    [SerializeField] Transform[] particles;
    [SerializeField] SpriteRenderer[] particleRenderers;//this is to set the colors
    float timer;
    public const float SpinEffectDuration = .2f;
    const float TotalEffectDuration = 7;
    const float particleSpreadSpeed = 1.875f;
    Vector2 center;
    //so effect goes like this
    //first 2 circles spin around the character's center, each one spins 360 degrees, and both are on opposite sides
    //after that, spawn a ring of 8 particles and a slower spreading ring of 4 particles

    void Update()
    {
        if (timer < SpinEffectDuration)
        {
            for (int i = 0; i < 2; i++)//have 2 particles unique for the first spin to avoid animation desync issues
            {
                Transform particle = particles[i];
                particle.gameObject.SetActive(true);
                particle.position = center + (i * Mathf.PI + Helper.Remap(timer, 0, SpinEffectDuration, 0, Mathf.PI * 2, false)).PolarVector(.5f);
            }
        }
        else if (timer < TotalEffectDuration)
        {
            particles[0].gameObject.SetActive(false);
            particles[1].gameObject.SetActive(false);
            float relativeTimer = timer - SpinEffectDuration;
            float opacity = Mathf.InverseLerp(TotalEffectDuration, TotalEffectDuration - .2f, timer);
            Color colorWithUpdatedOpacity = particleRenderers[2].color;
            colorWithUpdatedOpacity.a = opacity;
            for (int i = 2; i < 6; i++)
            {
                Transform particle = particles[i];
                particle.gameObject.SetActive(true);
                particle.position = center + (i * Mathf.PI * .5f).PolarVector(relativeTimer * particleSpreadSpeed);
                particleRenderers[i].color = colorWithUpdatedOpacity;
            }
            for (int i = 6; i < 14; i++)
            {
                Transform particle = particles[i];
                particle.gameObject.SetActive(true);
                particle.position = center + (i * Mathf.PI * .25f).PolarVector(relativeTimer * 3 * particleSpreadSpeed);
                particleRenderers[i].color = colorWithUpdatedOpacity;
            }
            for (int i = 14; i < 30; i++)
            {
                Transform particle = particles[i];
                particle.gameObject.SetActive(true);
                particle.position = center + (i * Mathf.PI * .125f + Mathf.PI * 0.0625f + (0.25f / relativeTimer * (i % 2 * 2 - 1))).PolarVector(relativeTimer * 5 * particleSpreadSpeed);
                particleRenderers[i].color = colorWithUpdatedOpacity;
            }
        }
        else
        {
            Destroy(gameObject);
        }
        timer += Time.deltaTime;
    }
    public static void SpawnWithoutSound(Vector2 center, Color color)
    {
        GameObject obj = Instantiate(CommonPrefabs.DeathParticles, center, Quaternion.identity);
        DeathParticle particle = obj.GetComponent<DeathParticle>();
        particle.center = center;
        for (int i = 0; i < particle.particleRenderers.Length; i++)
        {
            particle.particleRenderers[i].color = color;
            //don't need to adjust position since they will be children of the prefab
            //the game objects will also be disabled in the prefab
        }
    }
    public static void Spawn(Vector2 center, Color color, AudioSource audioSource)
    {
        SpawnWithoutSound(center, color);
        CommonSounds.PlayDeathBig(audioSource);
    }
}
