using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Common.Interfaces;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Common.Characters.Main.Scripts
{
    public class PlayerLife : MonoBehaviour, IDamageable
    {
        private void Awake()//can do in awake because gamemanager instance will already be loaded from previous scene
        {
            GameManager.instance.playerLife = this;
        }
        [SerializeField] AudioSource audioSource;
        public const int StartingChances = 6;
        public static int chances;
        public const int LifeMax = 31;
        public int life = LifeMax;
        public const float ImmuneTimeMax = 1;
        public float immuneTime;
        public const float DeathRestartDuration = 3;
        public float deathRestartTimer;
        [SerializeField] AudioClip hitSound;
        public bool Dead => life <= 0;
#if UNITY_EDITOR
        [SerializeField] bool debug_dontTakeDamage;
#endif
        private void Update()
        {

#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.H))
            {
                HealMax();
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                debug_dontTakeDamage = !debug_dontTakeDamage;
            }
#endif

            immuneTime -= Time.deltaTime;
            deathRestartTimer -= Time.deltaTime;
            if (Dead && deathRestartTimer < 0)
            {
                if (chances < 0)
                {
                    GameManager.startedGame = false;
                    LevelInfo.PrepareStageChange();
                    SceneManager.LoadScene(SceneIndices.MainMenu);
                    chances = StartingChances;
                }
                else
                {
                    int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;

                    //shouldn't awake of scene scripts be called here??
                    //apparently not
                    SceneManager.LoadScene(activeSceneIndex);//right here the gamemanager player references will be swapped.
                    if (MusicManager.PlayingBossMusic)
                    {
                        MusicManager.RestartCurrentLevelMusic();
                    }
                    //because thing didn't work, now the checkpoint respawn is called on
                    //multiscene singleton populator            
                }
            }
        }

        public void Heal(int life)
        {
            this.life += life;
            //later add sfx and particle
            if (this.life > LifeMax)
            {
                this.life = LifeMax;
            }
            UIManager.UpdatePlayerLifeBar(this.life);
        }
        public void HealMax()
        {
            life = LifeMax;
            UIManager.UpdatePlayerLifeBar(life);
        }
        public void Damage(int damage)
        {
#if UNITY_EDITOR
            if (debug_dontTakeDamage)
            {
                return;
            }
#endif
            if (immuneTime > 0)
                return;
            this.life -= damage;
            CheckDead();
            UIManager.UpdatePlayerLifeBar(life);
            immuneTime = ImmuneTimeMax;
            CommonSounds.Play(hitSound, audioSource, .8f, .8f);
            PlayerControl control = GameManager.PlayerControl;
            Vector2 vel = control.rb.velocity;
            if (Mathf.Abs(vel.x) > 0.001f)
            {
                vel.x = PlayerControl.KBPushbackVelocity * GameManager.PlayerRenderer.SpriteDirection;
            }
            vel.x = -vel.x;
            GameManager.PlayerControl.rb.velocity = vel;
        }
        IEnumerator ShakeWithDelay()
        {
            yield return new WaitForSeconds(DeathParticle.SpinEffectDuration);
            ScreenShakeManager.AddLargeShake();
        }
        public void CheckDead()
        {
            ScreenShakeManager.AddTinyShake();
            if (Dead)
            {
                deathRestartTimer = DeathRestartDuration;
                DeathParticle.Spawn(transform.position, GameManager.PlayerRenderer.Color, audioSource);
                GameManager.PlayerControl.DisableCollision();
                chances--;
                StartCoroutine(ShakeWithDelay());
                if (chances < 0)
                {
                    PlayerWeaponManager.ResetUnlocks();
                }
            }
        }
    }
}
