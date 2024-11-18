using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Common.Effects.Lightning;
using Assets.Helpers;
using Assets.Systems;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// still need to do animations
/// </summary>
public class TyphoonBossAI : Enemy
{
    static readonly int AnimIDIdle = Animator.StringToHash("Idle");

    public override int LifeMax => 25;

    const float ArenaWidth = 16;
    const float ArenaHeight = 8;

    const short StateIDStart = 0;
    const short StateIDIntro = 1;
    const short StateIDLightningOrbs = 2;
    const short StateIDLightning = 3;
    const short StateIDRainLeftToRight = 4;
    const short StateIDRainRightToLeft = 5;

    const float IntroDuration = 1;

    const float LightningOrbsStartup = 1;
    const short LightningOrbsActionCount = 1;
    const float LightningOrbsActionRate = 4;
    const float LightningOrbsDelay = 1;
    const float LightningOrbAccelerationDuration = .6f;
    const float LightningOrbSpinSpeedRadiansPerSec = 1.5f;
    const float LightningOrbAccelerationCurveExponent = 2;

    const float LightningStartup = 1;
    const short LightningActionCount = 3;
    const float LightningActionRate = 2;
    const float LightningDelay = 1;
    const float LightningYOffset = -0.5f;

    const float RainStartup = 0.7f;
    const short RainActionCount = 20;
    const float RainActionRate = .1f;
    const float RainDelay = 0.5f;

    public new Transform transform;
    [SerializeField] Transform arenaCenterTransform;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] float stateTimer;
    [SerializeField] float actionTimer;
    [SerializeField] int actionCounter;
    [SerializeField] short state;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] TyphoonBossLightningOrb[] lightningOrbs;
    [SerializeField] TyphoonEnemyCloudProjectile[] rainProjPool;
    [SerializeField] SimpleLightningRenderer lightningRenderer;
    [SerializeField] ParticleSystem lightningTelegraphParticles;
    Vector2 lightningTargetPoint;
    Vector2 movementTargetPoint;//only some states use the movement target point
    Vector2 arenaCenter;
    bool StateJustStarted => stateTimer >= 0 && stateTimer < 1E-20f;
    bool ActionJustStarted => actionTimer >= 0 && actionTimer < 1E-20f;
    bool IsOnLeftSideOfArena => transform.position.x < arenaCenter.x;
    bool IsOnRightSideOfArena => transform.position.x > arenaCenter.x;
    bool IsOnMiddleOfArena => transform.position.x == arenaCenter.x;
    float CurrentArenaSideSign => Mathf.Sign(transform.position.x - arenaCenter.x);
    float DirectionSign => sprite.flipX ? -1 : 1;
    bool FlipX => transform.position.x < GameManager.PlayerPosition.x;
    Vector2 CurrentArenaSidePoint => IsOnLeftSideOfArena ? (arenaCenter - new Vector2(ArenaWidth / 2 - 2, ArenaHeight / 2 - 1)) : (arenaCenter + new Vector2(ArenaWidth / 2 - 2, -ArenaHeight / 2 + 1));
    void Update()
    {
        if (life <= 0)
        {
            return;
        }
        sprite.flipX = FlipX;
        switch (state)
        {
            case StateIDStart:
                State_Start();
                break;
            case StateIDIntro:
                State_Intro();
                break;
            case StateIDLightningOrbs:
                State_LightningOrbs();
                break;
            case StateIDLightning:
                State_Lightning();
                break;
            case StateIDRainLeftToRight:
            case StateIDRainRightToLeft:
                State_Rain();
                break;
        }
        stateTimer += Time.deltaTime;
        actionTimer += Time.deltaTime;
    }
    private void State_Start()
    {
        if (arenaCenter == default)
        {
            arenaCenter = arenaCenterTransform.position;
        }
        // animator.CrossFade(AnimIDIdle, 0);
    }
    private void State_Intro()
    {
        if (StateJustStarted)
        {
            UIManager.ActivateBossLifeBar(FlipnoteColors.Yellow);
        }
        UIManager.UpdateBossLifeBar(Mathf.Clamp01(stateTimer / IntroDuration));
        if (stateTimer > IntroDuration)
        {
            SwitchState(StateIDLightningOrbs, LightningOrbsActionCount);
        }
    }

    //sprite mais agaichado (pode só botar um pouco mais pra baixo o sprite do personagem encima da nuvem)
    private void State_LightningOrbs()
    {
        if (StateJustStarted)
        {
            for (int i = 0; i < lightningOrbs.Length; i++)
            {
                TyphoonBossLightningOrb orb = lightningOrbs[i];
                orb.timeLeftUntilColliderActivates = LightningOrbsStartup + LightningOrbAccelerationDuration;
                orb.collider.enabled = false;
                orb.sprite.color = new Color(1, 1, 1, 0);
                orb.gameObject.SetActive(true);
            }
        }
        if (stateTimer < LightningOrbsStartup)
        {
            //go to arena center
            transform.position = Helper.Decay(transform.position, arenaCenter, 10);

            int orbCount = lightningOrbs.Length;
            float a = Mathf.InverseLerp(0, .5f, stateTimer - LightningOrbsStartup);
            Color orbColor = new(1, 1, 1, a);
            for (int i = 0; i < orbCount; i++)
            {
                //don't cover entire arena, just enough to force player to the corner
                TyphoonBossLightningOrb orb = lightningOrbs[i];
                orb.sprite.color = orbColor;
            }
            PositionOrbs(0);
        }
        else if (stateTimer < LightningOrbsStartup + LightningOrbsActionCount * LightningOrbsActionRate)
        {
            //acelerate spin
            //very readable code yes
            PositionOrbs(SampleCompositeFunction(stateTimer - LightningOrbsStartup, LightningOrbAccelerationDuration, LightningOrbSpinSpeedRadiansPerSec, LightningOrbAccelerationCurveExponent));

        }
        else if (stateTimer < LightningOrbsStartup + LightningOrbsActionCount * LightningOrbsActionRate + LightningOrbsDelay)
        {
            int orbCount = lightningOrbs.Length;
            float relativeTimer = stateTimer - LightningOrbsStartup - LightningOrbsActionCount * LightningOrbsActionRate;
#if UNITY_EDITOR
            this.relativeTimer = relativeTimer;
#endif
            float a = Mathf.InverseLerp(.5f, 0, relativeTimer);
            Color orbColor = new(1, 1, 1, a);
            for (int i = 0; i < orbCount; i++)
            {
                //don't cover entire arena, just enough to force player to the corner
                TyphoonBossLightningOrb orb = lightningOrbs[i];
                orb.sprite.color = orbColor;
                orb.collider.enabled = false;
            }
            PositionOrbs(SampleCompositeFunction(stateTimer - LightningOrbsStartup, LightningOrbAccelerationDuration, LightningOrbSpinSpeedRadiansPerSec, LightningOrbAccelerationCurveExponent));
        }
        else
        {
            for (int i = 0; i < lightningOrbs.Length; i++)
            {
                lightningOrbs[i].gameObject.SetActive(false);
            }
            SwitchState(StateIDLightning, LightningActionCount);
        }
    }
    private void PositionOrbs(float spinAmount)
    {
        int orbCount = lightningOrbs.Length;
        Vector2 center = transform.position;
        for (int i = 0; i < orbCount; i++)
        {
            //don't cover entire arena, just enough to force player to the corner
            TyphoonBossLightningOrb orb = lightningOrbs[i];
            int perArmOrbIndex = i / 2;
            int orbSide = i % 2 * 2 - 1;
            float orbDist = perArmOrbIndex;//3 is dist
            float offset = orbSide * Mathf.PI * .5f - perArmOrbIndex * 2f / orbCount;
            orb.transform.SetPositionAndRotation(center + (spinAmount + offset).PolarVector(orbDist), Quaternion.Euler(0, 0, stateTimer * 200));
        }
    }

    //sprite com mão para cima e outra apoiada, e depois as duas mãos apoiadas
    private void State_Lightning()
    {
        if (stateTimer < LightningOrbsStartup)
        {
            Vector2 playerPos = GameManager.PlayerPosition;
            movementTargetPoint.Set(playerPos.x, arenaCenter.y + 2);
            RaycastHit2D hit = Physics2D.Raycast(playerPos, Vector2.down, 20f, Layers.Tiles);
            lightningTargetPoint.Set(playerPos.x, hit.point.y);
            transform.position = Helper.Decay(transform.position, movementTargetPoint, 5);
        }
        else if (stateTimer < LightningOrbsStartup + LightningActionRate * LightningActionCount)
        {
            float relativeTimer = stateTimer - LightningOrbsStartup;
            float actionProgress = Mathf.InverseLerp(0, LightningActionRate, relativeTimer % LightningActionRate);
            if (ShouldDoAction(LightningStartup, LightningActionRate))
            {
                Vector2 playerPos = GameManager.PlayerPosition;
                movementTargetPoint.Set(playerPos.x, arenaCenter.y + 2);
                RaycastHit2D hit = Physics2D.Raycast(playerPos, Vector2.down, 20f, Layers.Tiles);
                lightningTargetPoint.Set(playerPos.x, hit.point.y);
            }
            if (actionProgress < .5f)
            {
                Helper.TelegraphLightning(actionProgress, transform.position + new Vector3(0, LightningYOffset), lightningTargetPoint, 0.5f, lightningTelegraphParticles);
            }
            else//if action progress >= 0.5
            {
                if (!lightningRenderer.LightningActive)
                {
                    EffectsHandler.SpawnSmallExplosion(FlipnoteColors.ColorID.Yellow, lightningTargetPoint, LightningActionRate / 2f);
                    lightningRenderer.ActivateAndSetAttributes(.1f, transform.position + new Vector3(0, LightningYOffset), lightningTargetPoint, LightningActionRate / 2f);
                }
                Vector2 lightningCenter = lightningRenderer.CenterPoint;
                Vector2 deltaPos = lightningTargetPoint - (Vector2)transform.position;
                float angle = Mathf.Atan2(deltaPos.y, deltaPos.x);
                Vector2 size = new Vector2(.5f, deltaPos.magnitude);
                Collider2D playerCollider = Physics2D.OverlapBox(lightningCenter, size, angle, Layers.Player);
                if(playerCollider != null)
                {
                    playerCollider.GetComponent<PlayerLife>().Damage(4);
                }
            }
            transform.position = Helper.Decay(transform.position, movementTargetPoint, 5);
        }
        else if (stateTimer < LightningOrbsStartup + LightningActionRate * LightningActionCount + LightningDelay)
        {
            transform.position = Helper.Decay(transform.position, arenaCenter + stateTimer.PolarVector(2), 10);
        }
        else
        {
            SwitchState((short)Random.Range(StateIDRainLeftToRight, StateIDRainRightToLeft + 1), RainActionCount);
        }
    }

    //sprite de ataque de chuva com as duas mãos apoiadas na nuvem
    private void State_Rain()
    {
        //go above center of arena, then curve down to one of the edges, and then slide left while raining
        if(stateTimer < RainStartup)
        {
            Vector2 origin = arenaCenter + new Vector2(0, 2);
            float xOffset = state == StateIDRainLeftToRight ? -ArenaWidth / 2 + 1 : ArenaWidth / 2 - 1;
            Vector2 targetPoint = new Vector2(arenaCenter.x + xOffset, arenaCenter.y + 3);
            float progress = Mathf.InverseLerp(0, RainStartup - .2f, stateTimer);
            progress = Easings.SqrInOut(progress);
            targetPoint.x = Mathf.Lerp(origin.x, targetPoint.x, Easings.SqrOut(progress));
            targetPoint.y = Mathf.Lerp(origin.y, targetPoint.y, Easings.SqrIn(progress));
            transform.position = Helper.Decay(transform.position, targetPoint, 20);
        }
        else if (stateTimer < RainStartup + RainActionCount * RainActionRate)
        {
            float startX = state == StateIDRainLeftToRight ? -ArenaWidth / 2 + 1 : ArenaWidth / 2 - 1;
            float endX = state == StateIDRainLeftToRight ? ArenaWidth / 2 - 2 : -ArenaWidth / 2 + 2;

            float progress = Mathf.InverseLerp(0, RainActionRate * RainActionCount, stateTimer - RainStartup);
            progress = Easings.SqrInOut(progress);
            Vector2 targetPoint = new Vector2(arenaCenter.x + Mathf.Lerp(startX, endX, progress), arenaCenter.y + 3);
            transform.position = Helper.Decay(transform.position, targetPoint, 20);
            if (ShouldDoAction(RainStartup, RainActionRate))
            {
                if(Helper.TryFindFreeIndex(rainProjPool, out int index))
                {
                    TyphoonEnemyCloudProjectile rainProj = rainProjPool[index];
                    rainProj.gameObject.SetActive(true);
                    rainProj.transform.position = transform.position + new Vector3(Random2.Float(-0.5f, 0.5f), 0);
                    rainProj.rb.velocity = new Vector2(0, -5);
                    rainProj.timer = 0;
                }
            }
        }
        else if (stateTimer < RainStartup + RainActionCount * RainActionRate + RainDelay)
        {
            Vector2 origin = arenaCenter + new Vector2(0, 2);
            float xOffset = state == StateIDRainLeftToRight ? ArenaWidth / 2 - 2 : -ArenaWidth / 2 + 2;
            Vector2 targetPoint = new Vector2(arenaCenter.x + xOffset, arenaCenter.y + 3);
            //progress is inverted, instead of 0 to 1 so it goes from 1 to 0
            float progress = 1 - Mathf.InverseLerp(0, RainStartup - .2f, stateTimer - RainDelay - RainActionCount * RainActionRate);
            progress = Easings.SqrInOut(progress);
            targetPoint.x = Mathf.Lerp(origin.x, targetPoint.x, Easings.SqrOut(progress));
            targetPoint.y = Mathf.Lerp(origin.y, targetPoint.y, Easings.SqrIn(progress));
            transform.position = Helper.Decay(transform.position, targetPoint, 20);
        }
        else
        {
            SwitchState(StateIDLightningOrbs, LightningOrbsActionCount);
        }
    }
    public void ChangeToIntro()
    {
        state = StateIDIntro;
        state = StateIDIntro;
        actionTimer = stateTimer = 0;
    }
    void SwitchState(short nextStateID, int nextStateActionCount)
    {
        state = nextStateID;
        stateTimer = -Time.deltaTime;
        actionTimer = -Time.deltaTime;
        actionCounter = nextStateActionCount;
    }
    void TrySwitchState(short nextStateID, int currentActionCount, float currentStartup, float currentActionRate, float currentExtraDelay, int nextStateActionCount)
    {
        if (stateTimer > currentActionCount * currentActionRate + currentStartup + currentExtraDelay)
        {
            SwitchState(nextStateID, nextStateActionCount);
        }
    }
    bool ShouldDoAction(float startup, float actionRate)
    {
        if (actionCounter <= 0)
        {
            return false;
        }
        float relativeTimer = actionTimer - startup;
        if (relativeTimer > actionRate)
        {
            relativeTimer %= actionRate;
            actionTimer = relativeTimer + startup;
            actionCounter--;
            return true;
        }
        return false;
    }
    public override void OnHit(int damageTaken)
    {
        UIManager.UpdateBossLifeBar(LifePercent);
    }

    //thanks chatgpt
    static float SampleCompositeFunction(float x, float x0, float linearSlope, float quadraticExponent)
    {
        // Define the quadratic exponent: f(x) = a * x^p
        float p = quadraticExponent;

        // Calculate the quadratic coefficient 'a' to match at x0 (for continuity)
        // Given that g(x) = m * x + n must equal f(x) = a * x^p at x0
        // a * x0^p = m * x0 + n
        // n is determined by continuity and slope at x0
        float a = linearSlope / (p * Mathf.Pow(x0, p - 1));  // Ensuring smoothness at x0
        float n = a * Mathf.Pow(x0, p) - linearSlope * x0;    // Continuity condition at x0

        // Sample the function based on the value of x
        if (x <= x0)
        {
            //Gizmos.color = Color.red;
            // Use the generalized quadratic function: f(x) = a * x^p
            return a * Mathf.Pow(x, p);
        }
        else
        {
            //Gizmos.color = Color.green;
            // Use the linear function: g(x) = m * x + n
            return linearSlope * x + n;
        }
    }

    public override bool PreKill()
    {
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        sprite.enabled = false;
        DeathParticle.Spawn(transform.position, FlipnoteColors.Blue, audioSource);
        EffectsHandler.SpawnBigExplosion(FlipnoteColors.ColorID.Blue, transform.position);
        StartCoroutine(ReturnToMainMenuAfter3SecAndUnlockUpgrade());
        return false;
    }
    IEnumerator ReturnToMainMenuAfter3SecAndUnlockUpgrade()
    {
        yield return new WaitForSecondsRealtime(3f);
        PlayerWeaponManager.UnlockTyphoon();
        GameManager.CleanupCheckpoints();
        SceneManager.LoadScene(SceneIndices.MainMenu);
    }
#if UNITY_EDITOR
    float relativeTimer;
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up, relativeTimer.ToString());
    }
#endif
}
