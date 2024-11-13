using Assets.Common.Consts;
using Assets.Common.Interfaces;
using Assets.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoShot : Projectile
{
    public override int Damage => 1;//multi hit 
    public GameObject[] sparkles;
    public float[] sparkleTimers;
    float timer = 0;
    float hitTimer;
    short hitCounter;
    const float hitRate = 3f / 50f;
    const short maxHits = 10;
    const float SparkleFrequency = 0.1f;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Rigidbody2D rb;
    bool exploded = false;
    [SerializeField] LayerMask enemyLayer;
    private void Start()
    {
        ResetValues();  
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if (exploded)
        {
            for (int i = 0; i < sparkleTimers.Length; i++)
            {
                float sparkleTimer = sparkleTimers[i];
                sparkleTimer += Time.deltaTime;
                if(sparkleTimer > SparkleFrequency)
                {
                    GameObject sparkle = sparkles[i];
                    Vector3 sparkleOffset = Random2.CenteredRect(1, 3);
                    sparkle.transform.position = transform.position + sparkleOffset;
                    sparkleTimer -= SparkleFrequency;
                }
                sparkleTimers[i] = sparkleTimer;
            }    
        }
        else
        {
            Vector2 vel = rb.velocity;
            vel.x = Helper.Decay(vel.x, 0, 30);
            vel.y = Helper.Decay(vel.y, 0, 30);
            if(timer > 1)
            {
                hitTimer = hitRate;
                exploded = true;
                for (int i = 0; i < sparkleTimers.Length; i++)
                {
                    sparkleTimers[i] = Random2.Float(SparkleFrequency);
                    Vector3 sparkleOffset = Random2.CenteredRect(1, 3);
                    GameObject sparkle = sparkles[i];
                    sparkle.transform.position = transform.position + sparkleOffset;
                    sparkle.SetActive(true);
                }
                sprite.enabled = false;
            }
        }
    }
    private void FixedUpdate()
    {
        if (exploded)
        {
            if(hitCounter > maxHits)
            {
                gameObject.SetActive(false);
                ResetValues();
                return;
            }
            hitTimer += Time.fixedDeltaTime;
            if(hitTimer > hitRate)
            {
                hitTimer -= hitRate;
                hitCounter++;
                Collider2D[] hitObjs = Physics2D.OverlapBoxAll(transform.position, new Vector3(1,3), 0, enemyLayer);
                for (int i = 0; i < hitObjs.Length; i++)
                {
                    GameObject obj = hitObjs[i].gameObject;
                    if(!obj.CompareTag(Tags.Player) && obj.TryGetComponent(out IDamageable damageable))
                    {
                        damageable.Damage(Damage);
                    }
                }
            }
        }
    }
    public void ResetValues()
    {
        sparkleTimers = new float[sparkles.Length];
        for (int i = 0; i < sparkleTimers.Length; i++)
        {
            sparkleTimers[i] = SparkleFrequency + 0.00001f;
            sparkles[i].SetActive(false);
        }
        timer = 0;
        hitTimer = 0;
        hitCounter = 0;
        exploded = false;
    }
}
