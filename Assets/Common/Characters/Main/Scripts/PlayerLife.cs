﻿using Assets.Common.Consts;
using Assets.Common.Interfaces;
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
        public const int StartingChances = 3;
        public static int chances;
        public const int LifeMax = 27;
        public int life = LifeMax;
        public const float ImmuneTimeMax = 1;
        public float immuneTime;
        public const float DeathRestartDuration = 3;
        public float deathRestartTimer;
        public bool Dead => life <= 0;
        private void Update()
        {
            immuneTime -= Time.deltaTime;
            deathRestartTimer -= Time.deltaTime;
            if (Dead && deathRestartTimer < 0)
            {
                if (chances < 0)
                {
                    SceneManager.LoadScene(SceneIndices.MainMenu);
                    GameManager.CleanupCheckpoints();

                    chances = StartingChances;
                    GameManager.latestCheckpointIndex = -1;
                }
                else
                {
                    int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;

                    //shouldn't awake of scene scripts be called here??
                    //apparently not
                    SceneManager.LoadScene(activeSceneIndex);//right here the gamemanager player references will be swapped.

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
            UIManager.UpdatePlayerLifeBar(life);
        }
        public void HealMax()
        {
            life = LifeMax;
            UIManager.UpdatePlayerLifeBar(life);
        }
        public void Damage(int damage)
        {
            if (immuneTime > 0)
                return;
            this.life -= damage;
            CheckDead();
            UIManager.UpdatePlayerLifeBar(life);
            immuneTime = ImmuneTimeMax;
        }
        public void CheckDead()
        {
            if (Dead)
            {
                deathRestartTimer = DeathRestartDuration;
                DeathParticle.Spawn(transform.position, GameManager.PlayerRenderer.Color, audioSource);
                GameManager.PlayerControl.DisableCollision();
                chances--;
            }
        }
    }
}