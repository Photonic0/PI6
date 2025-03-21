using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BossHelper
{
    public static void BossDeath(GameObject obj, MonoBehaviour behaviour, GameObject[][] objsToDisable, Color deathParticleColor, Vector2 arenaCenter)
    {
        if (objsToDisable != null && objsToDisable.Length > 0)
        {
            for (int j = 0; j < objsToDisable.Length; j++)
            {
                if (objsToDisable[j] != null && objsToDisable[j].Length > 0)
                {
                    for (int i = 0; i < objsToDisable[j].Length; i++)
                    {
                        objsToDisable[j][i].gameObject.SetActive(false);
                    }
                }
            }
        }
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        rb.simulated = false;
        rb.isKinematic = true;
        obj.GetComponent<BoxCollider2D>().enabled = false;
        MusicManager.StopMusic();
        behaviour.StartCoroutine(BossFinish(obj, deathParticleColor, arenaCenter));
    }
    static IEnumerator BossFinish(GameObject obj, Color deathParticleColor, Vector2 arenaCenter)
    {
        if (!obj.TryGetComponent<AudioSource>(out AudioSource audioSource))
        {
            if (audioSource == null)
            {
                audioSource = obj.transform.root.GetComponentInChildren<AudioSource>();
            }
        }
        DeathParticle deathParticles = DeathParticle.Spawn(obj.transform.position, deathParticleColor, audioSource);
        deathParticles.popupText = "New weapon unlocked! \n Press E while in a level to select a weapon";
        //PlayerControl.DisableInputs();
        PlayerWeaponManager.CloseMenu();
        ScreenShakeManager.AddTinyShake();
        yield return new WaitForSeconds(DeathParticle.SpinEffectDuration);
        ScreenShakeManager.AddLargeShake();
        if (obj.TryGetComponent(out SpriteRenderer sprite))
        {
            sprite.enabled = false;
        }
        EffectsHandler.SpawnMediumExplosion(deathParticleColor, obj.transform.position);
        yield return new WaitForSeconds(2f - DeathParticle.SpinEffectDuration);
        if (deathParticles != null)
        {
            deathParticles.BeAbsorbedByPlayer();
        }
        yield return new WaitForSeconds(5f);
        LevelInfo.PrepareStageChange();
        SceneManager.LoadScene(SceneIndices.MainMenu);
    }

}
