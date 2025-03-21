using Assets.Helpers;
using UnityEngine;

public class DeathParticle : MonoBehaviour
{
    [SerializeField] Transform[] particles;
    [SerializeField] SpriteRenderer[] particleRenderers;//this is to set the colors
    Vector3[] velocities;
    float timer;
    public const float SpinEffectDuration = .2f;
    const float TotalEffectDuration = 7;
    const float ParticleSpreadSpeed = 3f;
    const float AbsorbedByPlayerAnimationDuration = 2f;
    float ParticleAbsorbSpeed => 20f;// Helper.Remap(timer, 0, 4, ParticleSpreadSpeed, 20f);
    Vector2 center;
    //so effect goes like this
    //first 2 circles spin around the character's center, each one spins 360 degrees, and both are on opposite sides
    //after that, spawn a ring of 8 particles and a slower spreading ring of 4 particles
    bool beingAbsorbedByPlayer;
    public string popupText = string.Empty;
    public bool spawnedPopupText;
    void Update()
    {
        if (beingAbsorbedByPlayer)
        {
            Animation_AbsorbedByPlayer();
        }
        else
        {
            Animation_Default();
        }
    }

    private void Animation_Default()
    {
        if (timer < SpinEffectDuration)
        {
            for (int i = 0; i < 2; i++)//have 2 particles unique for the first spin to avoid animation desync issues
            {
                Transform particle = particles[i];
                particle.gameObject.SetActive(true);
                particle.position = center + (i * Mathf.PI + Helper.Remap(timer, 0, SpinEffectDuration, 0, Mathf.PI * 2, false)).PolarVector_Old(.5f);
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
                particle.position = center + (i * Mathf.PI * .5f).PolarVector_Old(relativeTimer * ParticleSpreadSpeed);
                particleRenderers[i].color = colorWithUpdatedOpacity;
            }
            for (int i = 6; i < 14; i++)
            {
                Transform particle = particles[i];
                particle.gameObject.SetActive(true);
                particle.position = center + (i * Mathf.PI * .25f).PolarVector_Old(relativeTimer * 2 * ParticleSpreadSpeed);
                particleRenderers[i].color = colorWithUpdatedOpacity;
            }
            //for (int i = 14; i < 30; i++)
            //{
            //    Transform particle = particles[i];
            //    particle.gameObject.SetActive(true);
            //    particle.position = center + (i * Mathf.PI * .125f + Mathf.PI * 0.0625f + (0.25f / relativeTimer * (i % 2 * 2 - 1))).PolarVector_Old(relativeTimer * 5 * particleSpreadSpeed);
            //    particleRenderers[i].color = colorWithUpdatedOpacity;
            //}
        }
        else
        {
            Destroy(gameObject);
        }
        timer += Time.deltaTime;
    }
    private void Animation_AbsorbedByPlayer()
    {
        //if (timer > AbsorbedByPlayerAnimationDuration + 1f)
        //{
        //    Destroy(gameObject);
        //    return;
        //}
        timer += Time.deltaTime;
        Vector3 playerPos = GameManager.PlayerPosition;
        const float DecaySpeed = 4f;
        bool anyActive = false;
        for (int i = 2; i < 14; i++)
        {
            Transform current = particles[i];
            if (!current.gameObject.activeInHierarchy)
            {
                continue;
            }
            anyActive = true;
            Vector3 currentPos = current.position;
            Vector3 targetDir = (playerPos - currentPos).normalized * ParticleAbsorbSpeed;
            velocities[i] = Helper.DecayVec3(velocities[i], targetDir, DecaySpeed);
            for (int j = 2; j < 14; j++)
            {
                Transform check = particles[j];
                if (!check.gameObject.activeInHierarchy)
                {
                    continue;
                }
                Vector3 deltaPos = (check.position - current.position);//need to use this position because it's what it'll be updated

                float dist = deltaPos.magnitude;
                const float RadiusCurrent = 0.35f;
                const float RadiusOther = 0.35f;

                if (dist < RadiusCurrent + RadiusOther)
                {
                    //velocities[i] -= deltaPos.normalized;
                    current.position -= deltaPos.normalized * (RadiusCurrent + RadiusOther - dist);
                }
            }
            current.position += velocities[i] * Time.deltaTime;
            if ((current.position - playerPos).magnitude < 0.3f)
            {
                current.gameObject.SetActive(false);
            }
        }
        if(!anyActive && !spawnedPopupText)
        {
            spawnedPopupText = true;
            FloatingText.SpawnText(GameManager.PlayerPosition + new Vector3(0, 3), popupText);
        }
    }
    public void BeAbsorbedByPlayer()
    {
        timer = 0;
        particles[0].gameObject.SetActive(false);
        particles[1].gameObject.SetActive(false);
        beingAbsorbedByPlayer = true;
        for (int i = 0; i < particles.Length; i++)
        {
            Color c = particleRenderers[i].color;
            c.a = 1;
            particleRenderers[i].color = c;
        }
    }
    public static DeathParticle SpawnWithoutSound(Vector2 center, Color color)
    {
        GameObject obj = Instantiate(CommonPrefabs.DeathParticles, center, Quaternion.identity);
        DeathParticle particle = obj.GetComponent<DeathParticle>();
        particle.center = center;
        particle.velocities = new Vector3[particle.particleRenderers.Length];
        for (int i = 0; i < particle.particleRenderers.Length; i++)
        {
            particle.particleRenderers[i].color = color;
            if (i < 6)
            {
                particle.velocities[i] = (i * Mathf.PI * .5f).PolarVector_Old(ParticleSpreadSpeed);
            }
            else
            {
                particle.velocities[i] = (i * Mathf.PI * .25f).PolarVector_Old(2 * ParticleSpreadSpeed);
            }
            //don't need to adjust position since they will be children of the prefab
            //the game objects will also be disabled in the prefab
        }
        return particle;
    }
    public static DeathParticle Spawn(Vector2 center, Color color, AudioSource audioSource)
    {
        DeathParticle particle = SpawnWithoutSound(center, color);
        if (audioSource != null)
        {
            CommonSounds.PlayDeathBig(audioSource);
        }
        return particle;
    }
}
