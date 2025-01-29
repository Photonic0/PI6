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
    static readonly int AnimIDHide = Animator.StringToHash("Hide");
    static readonly int AnimIDLightning = Animator.StringToHash("Lightning");
    static readonly int AnimIDLightningPre = Animator.StringToHash("LightningPre");

    public override int LifeMax => 50;

    const float ArenaWidth = 16;
    const float ArenaHeight = 8;

    const byte StateIDStart = 0;
    const byte StateIDIntro = 1;
    const byte StateIDLightningOrbs = 2;
    const byte StateIDLightning = 3;
    const byte StateIDRainLeftToRight = 4;
    const byte StateIDRainRightToLeft = 5;

    const float IntroDuration = 1;

    const float LightningOrbsStartup = 1;
    const sbyte LightningOrbsActionCount = 1;
    const float LightningOrbsActionRate = 4;
    const float LightningOrbsDelay = 1;
    const float LightningOrbAccelerationDuration = .6f;
    const float LightningOrbSpinSpeedRadiansPerSec = 1.85f;
    const float LightningOrbAccelerationCurveExponent = 2;

    const float LightningTelegraphDuration = .8f;
    const float LightningDuration = .25f;

    const float LightningStartup = .5f;
    const sbyte LightningActionCount = 3;
    const float LightningActionRate = LightningTelegraphDuration + LightningDuration;
    const float LightningDelay = 1;
    const float LightningSpawnYOffset = -1f;
    const float LightningBossYDistFromGround = 4;


    const float RainStartup = 0.1f;
    const sbyte RainActionCount = 30;
    const float RainActionRate = 0.05f;
    const float RainDelay = 0.5f;
    const float RainProjSpawnPositionYoffset = -0.8f;
    const float RainProjSpawnHalfXSpread = .7f;
    const float RainProjYVelocity = -6f;
    const float RainBossYOffsetFromArenaCenterInitially = 3;

    public new Transform transform;
    [SerializeField] Transform arenaCenterTransform;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] float stateTimer;
    [SerializeField] float actionTimer;
    [SerializeField] sbyte actionCounter;
    [SerializeField] byte state;
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
    bool FlipX => transform.position.x < GameManager.PlayerPosition.x;
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
            animator.CrossFade(AnimIDHide, 0);
            arenaCenter = arenaCenterTransform.position;
        }
    }
    private void State_Intro()
    {
        if (StateJustStarted)
        {
            UIManager.ActivateBossLifeBar(FlipnoteColors.Blue);
        }
        float progress = stateTimer / IntroDuration;
        if (progress <= .5f)
        {
            animator.CrossFade(AnimIDHide, 0);
        }
        else if (progress <= 0.75f)
        {
            animator.CrossFade(AnimIDLightning, 0);
        }
        else
        {
            animator.CrossFade(AnimIDIdle, 0);
        }
        UIManager.UpdateBossLifeBar(Mathf.Clamp01(progress));
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
            animator.CrossFade(AnimIDHide, 0);
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
            this.debug_numberDisplay = relativeTimer;
#endif
            float a = Mathf.InverseLerp(.5f, 0, relativeTimer);
            Color orbColor = new(1, 1, 1, a);
            for (int i = 0; i < orbCount; i++)
            {
                TyphoonBossLightningOrb orb = lightningOrbs[i];
                orb.sprite.color = orbColor;
                orb.collider.enabled = false;
            }
            animator.CrossFade(AnimIDIdle, 0);
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
            float orbDist = perArmOrbIndex * 0.95f + 1f;
            float offset = orbSide * Mathf.PI * 0.5f - perArmOrbIndex * 2f / orbCount;
            orb.transform.SetPositionAndRotation(center + (spinAmount + offset).PolarVector_Old(orbDist), Quaternion.Euler(0, 0, stateTimer * 200));
        }
    }

    //sprite com mão para cima e outra apoiada, e depois as duas mãos apoiadas
    private void State_Lightning()
    {
        if (stateTimer < LightningOrbsStartup)
        {
            Vector2 playerPos = GameManager.PlayerPosition;
            RaycastHit2D hit = Physics2D.Raycast(playerPos, Vector2.down, 20f, Layers.Tiles);
            movementTargetPoint.Set(playerPos.x, hit.point.y + LightningBossYDistFromGround);
            lightningTargetPoint.Set(playerPos.x, hit.point.y);
            transform.position = Helper.Decay(transform.position, movementTargetPoint, 5);
        }
        else if (stateTimer < LightningOrbsStartup + LightningActionRate * LightningActionCount)
        {
            float relativeTimer = (stateTimer - LightningStartup) % LightningActionRate;
            if (ShouldDoAction(LightningStartup, LightningActionRate))
            {
                Vector2 targetPos = GameManager.PlayerPosition;
                targetPos.x += LightningTelegraphDuration * GameManager.PlayerControl.rb.velocity.x;
                targetPos.x = Mathf.Clamp(targetPos.x - arenaCenter.x, -ArenaHeight + 1, ArenaHeight - 1) + arenaCenter.x;
                RaycastHit2D hit = Physics2D.Raycast(targetPos, Vector2.down, 20f, Layers.Tiles);
                movementTargetPoint.Set(targetPos.x, hit.point.y + LightningBossYDistFromGround);
                lightningTargetPoint.Set(targetPos.x, hit.point.y);
            }
            if (relativeTimer < LightningTelegraphDuration)
            {
                if (actionCounter > 0)
                {
                    animator.CrossFade(AnimIDLightningPre, 0);
                    Helper.TelegraphLightning(relativeTimer, transform.position + new Vector3(0, LightningSpawnYOffset), lightningTargetPoint, LightningTelegraphDuration, lightningTelegraphParticles);
                }
            }
            else//if relativeTimer > LightningTelegraphDuration
            {
                if (!lightningRenderer.LightningActive)
                {
                    animator.CrossFade(AnimIDLightning, 0);
                    EffectsHandler.SpawnSmallExplosion(FlipnoteColors.ColorID.Yellow, lightningTargetPoint, LightningDuration);
                    lightningRenderer.ActivateAndSetAttributes(.1f, transform.position + new Vector3(0, LightningSpawnYOffset), lightningTargetPoint, LightningDuration);
                }
                Vector2 lightningCenter = lightningRenderer.CenterPoint;
                Vector2 deltaPos = lightningTargetPoint - (Vector2)transform.position;
                float angle = Mathf.Atan2(deltaPos.y, deltaPos.x);
                Vector2 size = new(.5f, deltaPos.magnitude);
                Collider2D playerCollider = Physics2D.OverlapBox(lightningCenter, size, angle, Layers.PlayerHurtbox);
                if (playerCollider != null)
                {
                    GameManager.PlayerLife.Damage(4);
                }
            }
            transform.position = Helper.Decay(transform.position, movementTargetPoint, 10);
        }
        else if (stateTimer < LightningOrbsStartup + LightningActionRate * LightningActionCount + LightningDelay)
        {
            animator.CrossFade(AnimIDIdle, 0);
            float progress = (stateTimer - LightningOrbsStartup - (LightningActionRate * LightningActionCount)) / LightningDelay;
            float dist = progress * RainBossYOffsetFromArenaCenterInitially;
            float offsetRotation = Mathf.LerpUnclamped(Mathf.PI * 3, 0, progress);
            transform.position = Helper.Decay(transform.position, arenaCenter + offsetRotation.PolarVector_Old(dist), 10);
#if UNITY_EDITOR
            debug_numberDisplay = progress;
            handlesColor = Color.red;
#endif

        }
        else
        {
#if UNITY_EDITOR
            handlesColor = Color.green;
#endif
            SwitchState((byte)Random.Range(StateIDRainLeftToRight, StateIDRainRightToLeft + 1), RainActionCount);
        }
    }

    //sprite de ataque de chuva com as duas mãos apoiadas na nuvem
    private void State_Rain()
    {
        if (StateJustStarted)
        {
            animator.CrossFade(AnimIDIdle, 0);
        }
        //go above center of arena, then curve down to one of the edges, and then slide left while raining
        if (stateTimer < RainStartup)
        {
            Vector2 origin = arenaCenter - new Vector2(0, 2);
            float xOffset = state == StateIDRainLeftToRight ? -ArenaWidth / 2 + 1 : ArenaWidth / 2 - 1;
            Vector2 targetPoint = new(arenaCenter.x + xOffset, arenaCenter.y + RainBossYOffsetFromArenaCenterInitially);
            float progress = Mathf.InverseLerp(0, RainStartup / 2f, stateTimer);
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
            Vector2 targetPoint = new(arenaCenter.x + Mathf.Lerp(startX, endX, progress), arenaCenter.y + RainBossYOffsetFromArenaCenterInitially);
            transform.position = Helper.Decay(transform.position, targetPoint, 20);
            if (ShouldDoAction(RainStartup, RainActionRate))
            {
                animator.CrossFade(AnimIDLightning, 0);
                if (Helper.TryFindFreeIndex(rainProjPool, out int index))
                {
                    TyphoonEnemyCloudProjectile rainProj = rainProjPool[index];
                    rainProj.gameObject.SetActive(true);
                    rainProj.transform.position = transform.position + new Vector3(Random2.Float(-RainProjSpawnHalfXSpread, RainProjSpawnHalfXSpread), RainProjSpawnPositionYoffset);
                    rainProj.rb.velocity = new Vector2(0, RainProjYVelocity);
                    rainProj.timer = 0;
                }
            }
        }
        else if (stateTimer < RainStartup + RainActionCount * RainActionRate + RainDelay)
        {
            Vector2 origin = arenaCenter - new Vector2(0, 2);
            float xOffset = state == StateIDRainLeftToRight ? ArenaWidth / 2 - 2 : -ArenaWidth / 2 + 2;
            Vector2 targetPoint = new(arenaCenter.x + xOffset, arenaCenter.y + RainBossYOffsetFromArenaCenterInitially);
            //progress is inverted, instead of 0 to 1 so it goes from 1 to 0
            float progress = 1 - Mathf.InverseLerp(0, RainStartup / 2f, stateTimer - RainDelay - RainActionCount * RainActionRate);
            progress = Easings.SqrInOut(progress);
            targetPoint.x = Mathf.Lerp(origin.x, targetPoint.x, Easings.SqrOut(progress));
            targetPoint.y = Mathf.Lerp(origin.y, targetPoint.y, Easings.SqrIn(progress));
            transform.position = Helper.Decay(transform.position, targetPoint, 20);
            animator.CrossFade(AnimIDIdle, 0);
        }
        else
        {
            SwitchState(StateIDLightningOrbs, LightningOrbsActionCount);
        }
    }
    public void ChangeToIntro()
    {
        GetComponent<Collider2D>().enabled = true;
        state = StateIDIntro;
        state = StateIDIntro;
        actionTimer = stateTimer = 0;
    }
    void SwitchState(byte nextStateID, sbyte nextStateActionCount)
    {
        state = nextStateID;
        stateTimer = -Time.deltaTime;
        actionTimer = -Time.deltaTime;
        actionCounter = nextStateActionCount;
    }
    void TrySwitchState(byte nextStateID, int currentActionCount, float currentStartup, float currentActionRate, float currentExtraDelay, sbyte nextStateActionCount)
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
        lightningRenderer.Stop();
        for (int i = 0; i < lightningOrbs.Length; i++)
        {
            lightningOrbs[i].gameObject.SetActive(false);
        }
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        DeathParticle.Spawn(transform.position, FlipnoteColors.Blue, audioSource);
        StartCoroutine(ReturnToMainMenuAfter3SecAndUnlockUpgrade());
        return false;
    }
    IEnumerator ReturnToMainMenuAfter3SecAndUnlockUpgrade()
    {
        ScreenShakeManager.AddTinyShake();
        yield return new WaitForSecondsRealtime(DeathParticle.SpinEffectDuration);
        ScreenShakeManager.AddLargeShake();
        sprite.enabled = false;
        EffectsHandler.SpawnMediumExplosion(FlipnoteColors.ColorID.Blue, transform.position);
        yield return new WaitForSecondsRealtime(3f - DeathParticle.SpinEffectDuration); PlayerWeaponManager.UnlockTyphoon();
        LevelInfo.PrepareStageChange();
        SceneManager.LoadScene(SceneIndices.MainMenu);
    }
#if UNITY_EDITOR
    Color handlesColor;
    float debug_numberDisplay;
    private void OnDrawGizmos()
    {
        Handles.color = handlesColor;
        Handles.Label(arenaCenter + Vector2.up, debug_numberDisplay.ToString());
    }
#endif
}
