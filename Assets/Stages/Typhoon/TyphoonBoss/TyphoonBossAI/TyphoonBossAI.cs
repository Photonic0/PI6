using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Common.Effects.Lightning;
using Assets.Helpers;
using Assets.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class TyphoonBossAI : Enemy
{
    static readonly int AnimIDIdle = Animator.StringToHash("Idle");
    static readonly int AnimIDHide = Animator.StringToHash("Hide");
    static readonly int AnimIDLightning = Animator.StringToHash("Lightning");
    static readonly int AnimIDLightningPre = Animator.StringToHash("LightningPre");

    public override int LifeMax => 150;
    public bool ShouldBeInPhase2 => life <= LifeMax / 2f;
    public bool InPhase2OrTransitionToIt => state >= StateIDTransitionToPhase2;

    const float ArenaWidth = 16;
    const float ArenaHeight = 8;

    const float Phase2ArenaWidth = 20;
    const float Phase2ArenaHeight = 12;

    const byte StateIDStart = 0;
    const byte StateIDIntro = 1;
    const byte StateIDLightningOrbs = 2;
    const byte StateIDLightning = 3;
    const byte StateIDRainLeftToRight = 4;
    const byte StateIDRainRightToLeft = 5;
    const byte StateIDLightningHazard = 6;
    const byte StateIDLightningShots = 7;
    const byte StateIDTransitionToPhase2 = 8;
    const byte StateIDPhase2Lightning = 9;
    const byte StateIDPhase2LightningShot = 10;
    const byte StateIDPhase2LightningShotsPlatformStaircase = 11;
    const byte StateIDPhase2RandomizeClouds = 12;
    //all phase 2 states should have an ID higher than the state that is the transition to phase 2
    //for ease of coding reasons.

    const float IntroDuration = 1;

    const float LightningOrbsStartup = 1;
    const sbyte LightningOrbsActionCount = 1;
    const float LightningOrbsActionRate = 3;
    const float LightningOrbsDelay = 1;
    const float LightningOrbAccelerationDuration = .6f;
    const float LightningOrbSpinSpeedRadiansPerSec = 2f;
    const float LightningOrbAccelerationCurveExponent = 2;

    const float LightningTelegraphDuration = .7f;
    const float LightningDuration = .2f;

    const float LightningStartup = .5f;
    const sbyte LightningActionCount = 5;
    const float LightningActionRate = LightningTelegraphDuration + LightningDuration;
    const float LightningDelay = .5f;
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

    const float LightningHazardStartup = 0.3f;
    const sbyte LightningHazardActionCount = 8;
    const float LightningHazardActionRate = 1f;
    const float LightningHazardDelay = .4f;
    const float LightningHazardFloatTrajectoryWidth = 9f;
    const float LightningHazardFloatTrajectoryHeight = 5f;
    const float LightningHazardFloatTrajectorySpeed = 3f;
    const int LightningHazardAmountOfHazardsPerRound = 7;
    const float LightningHazardTelegraphDuration = 1f;
    const float LightningHazardHitboxDuration = 0.05f;
    const float LightningHazardHalfMaxAngle = 0.7f;

    const float LightningShotsStartup = 2f;
    const sbyte LightningShotsActionCount = 2;
    const float LightningShotsActionRate = 2f;
    const float LightningShotsDelay = .5f;
    const float LightningShotsProjVelocity = 6;

    const float LightningPhase2Startup = 1f;
    const int LightningPhase2ActionCount = 4;
    const float LightningPhase2ActionRate = LightningPhase2TelegraphDuration + LightningPhase2Duration;
    const float LightningPhase2Delay = .5f;
    const float LightningPhase2TelegraphDuration = .7f;
    const float LightningPhase2Duration = .2f;

    const float LightningShotPhase2Startup = 0.3f;
    const int LightningShotPhase2ActionCount = 5;
    const float LightningShotPhase2ActionRate = 1.2f;
    const float LightningShotPhase2Delay = 0.4f;

    const float LightningShotsPlatformStaircasePhase2CloudMovementTelegraphDuration = 1f;
    const float LightningShotsPlatformPhase2StaircaseTimeToGetIntoPosition = 0.4f;
    const float LightningShotsPlatformStaircasePhase2Startup = LightningShotsPlatformStaircasePhase2CloudMovementTelegraphDuration + LightningShotsPlatformPhase2StaircaseTimeToGetIntoPosition;
    const float LightningShotsPlatformStaircasePhase2ActionRate = .4f;
    const int LightningShotsPlatformStaircasePhase2ActionCount = 1;
    const float LightningShotsPlatformStaircasePhase2Delay = 1.5f;

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
    [SerializeField] TyphoonBossLightningHazard[] lightningHazardPool;
    [SerializeField] TyphoonEnemyTornadoLightning[] lightningProjPool;
    [SerializeField] TyphoonBossCloudPlatform leftPlatform, middlePlatform, rightPlatform;
    [SerializeField] Tilemap phase2ArenaWalls;
    [SerializeField] TileBase rightEdgeTile, leftEdgeTile;
    [SerializeField] AudioClip[] thunderShoot;
    Vector2 lightningTargetPoint;
    Vector2 movementTargetPoint;//only some states use the movement target point
    Vector2 arenaCenter;

    const sbyte PlatformTargetIDBottom = 0;
    const sbyte PlatformTargetIDMiddle = 1;
    const sbyte PlatformTargetIDTop = 2;
    sbyte leftPlatformTargetID;
    sbyte middlePlatformTargetID;
    sbyte rightPlatformTargetID;
    bool StateJustStarted => stateTimer >= 0 && stateTimer < 1E-20f;
    bool FlipX => transform.position.x < GameManager.PlayerPosition.x;
    void Update()
    {
        if (life <= 0)
        {
            return;
        }
        sprite.flipX = FlipX;
        Phase2PreUpdates();
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
            case StateIDLightningHazard:
                State_LightningHazard();
                break;
            case StateIDLightningShots:
                State_LightningShots();
                break;
            case StateIDTransitionToPhase2:
                State_TransitionToPhase2();
                break;
            case StateIDPhase2Lightning:
                State_Phase2Lightning();
                break;
            case StateIDPhase2LightningShot:
                State_Phase2LightningShot();
                break;
            case StateIDPhase2LightningShotsPlatformStaircase:
                State_Phase2LightningShotsPlatformStaircase();
                break;
            case StateIDPhase2RandomizeClouds:
                State_RandomizeClouds();
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
            SwitchState(StateIDLightningOrbs, LightningOrbsActionCount, LightningOrbsStartup, LightningOrbsStartup);
        }
    }
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
            SwitchState(StateIDLightning, LightningActionCount, LightningActionRate, LightningStartup);
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
    private void State_Lightning()
    {
        if (stateTimer < LightningStartup)
        {
            Vector2 playerPos = GameManager.PlayerPosition;
            RaycastHit2D hit = Physics2D.Raycast(playerPos, Vector2.down, 20f, Layers.Tiles);
            movementTargetPoint.Set(playerPos.x, hit.point.y + LightningBossYDistFromGround);
            lightningTargetPoint.Set(playerPos.x, hit.point.y);
            transform.position = Helper.Decay(transform.position, movementTargetPoint, 5);
        }
        else if (stateTimer < LightningStartup + LightningActionRate * LightningActionCount)
        {
            float relativeTimer = (stateTimer - LightningStartup) % LightningActionRate;
            if (ShouldDoAction(LightningStartup, LightningActionRate))
            {
                Vector2 targetPos = GameManager.PlayerPosition;
                targetPos.x += LightningTelegraphDuration * GameManager.PlayerControl.rb.velocity.x;
                targetPos.x = Mathf.Clamp(targetPos.x - arenaCenter.x, -ArenaWidth / 2f + 1, ArenaHeight / 2f - 1) + arenaCenter.x;
                RaycastHit2D hit = Physics2D.Raycast(targetPos, Vector2.down, 20f, Layers.Tiles);
                movementTargetPoint.Set(targetPos.x, hit.point.y + LightningBossYDistFromGround);
                lightningTargetPoint.Set(targetPos.x, hit.point.y);
            }
            if (relativeTimer < LightningTelegraphDuration)
            {
                animator.CrossFade(AnimIDLightningPre, 0);
                Helper.TelegraphLightning(relativeTimer, transform.position + new Vector3(0, LightningSpawnYOffset), lightningTargetPoint, LightningTelegraphDuration, lightningTelegraphParticles);
            }
            else//if relativeTimer >= LightningTelegraphDuration
            {
                if (!lightningRenderer.LightningActive)
                {
                    CommonSounds.PlayRandom(thunderShoot, audioSource, 1f, 1f);
                    ScreenShakeManager.AddSmallShake();
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
            transform.position = Helper.Decay(transform.position, movementTargetPoint, 20);
        }
        else if (stateTimer < LightningOrbsStartup + LightningActionRate * LightningActionCount + LightningDelay)
        {
            animator.CrossFade(AnimIDIdle, 0);
            float progress = (stateTimer - LightningStartup - (LightningActionRate * LightningActionCount)) / LightningDelay;
            float dist = progress * RainBossYOffsetFromArenaCenterInitially;
            float offsetRotation = Mathf.LerpUnclamped(Mathf.PI * 3, 0, progress);
            transform.position = Helper.Decay(transform.position, arenaCenter + offsetRotation.PolarVector_Old(dist), 20);
        }
        else
        {
#if UNITY_EDITOR
            handlesColor = Color.green;
#endif
            SwitchState((byte)Random.Range(StateIDRainLeftToRight, StateIDRainRightToLeft + 1), RainActionCount, RainActionRate, RainStartup);
        }
    }
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
            SwitchState(StateIDLightningHazard, LightningHazardActionCount, LightningHazardActionRate, LightningHazardStartup);
        }
    }
    void State_LightningHazard()
    {
        if (stateTimer < LightningHazardStartup)
        {
            Vector2 floatStart = arenaCenter;
            floatStart.x += LightningHazardFloatTrajectoryWidth / 2f;
            transform.position = Helper.Decay(transform.position, floatStart, 20);
        }
        else if (stateTimer < LightningHazardStartup + LightningHazardActionCount * LightningHazardActionRate)
        {
            Vector2 targetPos = arenaCenter;
            float relativeTimer = stateTimer - LightningHazardStartup;
            targetPos.x += LightningHazardFloatTrajectoryWidth / 2f * Mathf.Cos(relativeTimer * LightningHazardFloatTrajectorySpeed);
            targetPos.y += LightningHazardFloatTrajectoryHeight / 2f * Mathf.Sin(relativeTimer * 2f * LightningHazardFloatTrajectorySpeed);
            transform.position = Helper.Decay(transform.position, targetPos, 20);
            if (ShouldDoAction(LightningHazardStartup, LightningHazardActionRate))
            {
#if UNITY_EDITOR
                debug_GizmoDisplayForLightningRound = new();
#endif
                for (int i = 0; i < LightningHazardAmountOfHazardsPerRound; i++)
                {
                    float angle = Random2.Float(-LightningHazardHalfMaxAngle, LightningHazardHalfMaxAngle) - Mathf.PI / 2f;
                    Vector2 hazardCenter = arenaCenter;
                    hazardCenter.x += Helper.Remap(i, 0, LightningHazardAmountOfHazardsPerRound - 1, -ArenaWidth / 2f + 1.5f, ArenaWidth / 2f - 1.5f);
                    hazardCenter.x += Random2.Float(-0.5f, 0.5f);
                    if (Helper.TryFindFreeIndex(lightningHazardPool, out TyphoonBossLightningHazard obj))
                    {
                        obj.gameObject.SetActive(true);
                        obj.RaycastPositionsAndAdjustPositionAndRotation(hazardCenter, angle, out Vector2 node1AttachedSurfaceNormal, out Vector2 node2AttachedSurfaceNormal);
                        obj.LimitAngle(node1AttachedSurfaceNormal, node2AttachedSurfaceNormal, LightningHazardHalfMaxAngle);
                        obj.StartTelegraphAndThenActivate(LightningHazardTelegraphDuration, LightningHazardHitboxDuration);
#if UNITY_EDITOR
                        debug_GizmoDisplayForLightningRound.Add((angle.PolarVector(), hazardCenter));
#endif   
                    }
                }
                if (LightningHazardActionCount - actionCounter > 1)
                {
                    CommonSounds.PlayRandom(thunderShoot, audioSource, 1f, 1f);
                    ScreenShakeManager.AddSmallSpiralShake(arenaCenter, ScreenShakeManager.ShakeGroupIDLightningRenderer);
                }
            }
        }
        else if (stateTimer < LightningHazardStartup + LightningHazardActionCount * LightningHazardActionRate + LightningHazardDelay)
        {
            Vector3 targetPos = arenaCenter;
            targetPos.y += ArenaHeight / 2f - 1f;
            transform.position = Helper.DecayVec3(transform.position, targetPos, 15);
        }
        TrySwitchState(StateIDLightningShots, LightningHazardActionCount, LightningHazardStartup, LightningHazardActionRate, LightningHazardDelay, LightningShotsActionCount, LightningShotsActionRate, LightningShotsStartup);
    }
    void State_LightningShots()
    {
        if (stateTimer < LightningShotsStartup)
        {
            rb.velocity = Vector2.zero;
            Vector3 targetPos = arenaCenter;
            float progress = Mathf.InverseLerp(0.3f, LightningShotsStartup, stateTimer);
            progress = Easings.SqrInOut(progress);
            targetPos.x -= (ArenaWidth / 2f - 1.5f) * Easings.SqrOut(progress);
            targetPos.y -= (ArenaHeight / 2f - 1.5f) * Easings.SqrIn(progress);
            transform.position = Helper.Decay(transform.position, targetPos, 30);
            if (StateJustStarted)
            {
                CommonSounds.PlayRandom(thunderShoot, audioSource, 1f, 1f);
                const int ProjAmount = 13;
                for (int i = 0; i < lightningProjPool.Length; i++)
                {
                    lightningProjPool[i].gameObject.SetActive(false);
                }
                for (int i = 0; i < ProjAmount; i++)
                {
                    Vector2 target = arenaCenter;
                    target.y -= ArenaHeight / 2f;
                    target.x += Helper.Remap(i, 0, ProjAmount - 1, -ArenaWidth / 2f, ArenaWidth / 2f);
                    Vector2 origin = arenaCenter;
                    origin.y += ArenaHeight / 2f;
                    Vector2 vel = (target - origin).normalized * LightningShotsProjVelocity;
                    if (i % 2 == 0)
                    {
                        vel *= 0.5f;
                    }
                    FireLightningProj(vel, origin);
                }
            }
        }
        else if (stateTimer < LightningShotsStartup + LightningShotsActionCount * LightningShotsActionRate)
        {
            int arenaSide = -(actionCounter % 2 * 2 - 1);
            Vector3 targetPos = arenaCenter;
            targetPos.x -= (ArenaWidth / 2f - 1.5f) * arenaSide;
            targetPos.y -= (ArenaHeight / 2f - 1.5f);
            Vector2 prevPos = arenaCenter;
            prevPos.x -= (ArenaWidth / 2f - 1.5f) * -arenaSide;
            prevPos.y -= (ArenaHeight / 2f - 1.5f);
            float actionProgress = Mathf.InverseLerp(LightningShotsStartup, LightningShotsStartup + LightningShotsActionRate, actionTimer);
            float easedAndRemappedProgress = Easings.SqrInOut(Mathf.InverseLerp(0, .5f, actionProgress));
            if (actionCounter == LightningShotsActionCount)
            {
                easedAndRemappedProgress = 1;
            }
            targetPos.x = Mathf.Lerp(prevPos.x, targetPos.x, easedAndRemappedProgress);
            targetPos.y = Mathf.Lerp(targetPos.y, arenaCenter.y + 2, Easing010(easedAndRemappedProgress));
            transform.position = Helper.Decay(transform.position, targetPos, 15);
            if (ShouldDoAction(LightningShotsStartup, LightningShotsActionRate))
            {
                CommonSounds.PlayRandom(thunderShoot, audioSource, 1f, 1f);
                const int ProjAmount = 5;
                for (int i = 0; i < ProjAmount; i++)
                {
                    Vector2 vel = Helper.Remap(i, 0, ProjAmount - 1, 0, Mathf.PI / 2).PolarVector(LightningShotsProjVelocity);
                    vel.x *= arenaSide;
                    FireLightningProj(vel);
                }
            }
        }
        else if (stateTimer < LightningShotsStartup + LightningShotsActionCount * LightningShotsActionRate + LightningShotsDelay)
        {
            Vector3 targetPos = arenaCenter;
            transform.position = Helper.Decay(transform.position, targetPos, 30);
        }
        TrySwitchState(StateIDLightningOrbs, LightningShotsActionCount, LightningShotsStartup, LightningShotsActionRate, LightningShotsDelay, LightningOrbsActionCount, LightningOrbsActionRate, LightningOrbsStartup);
    }
    void FireLightningProj(Vector2 vel, Vector2 from)
    {
        if (Helper.TryFindFreeIndex(lightningProjPool, out TyphoonEnemyTornadoLightning proj))
        {
            proj.gameObject.SetActive(true);
            proj.rb.velocity = vel;
            proj.transform.position = from;
            proj.rb.rotation = vel.Atan2Deg();
        }
    }
    TyphoonEnemyTornadoLightning FireLightningProj(Vector2 vel)
    {
        if (Helper.TryFindFreeIndex(lightningProjPool, out TyphoonEnemyTornadoLightning proj))
        {
            proj.gameObject.SetActive(true);
            proj.rb.velocity = vel;
            proj.transform.position = transform.position;
            proj.rb.rotation = vel.Atan2Deg();
            return proj;
        }
        return null;
    }
    void State_TransitionToPhase2()
    {

        Tilemap mainTilemap = TyphoonStageSingleton.instance.solidTiles;
        if (StateJustStarted)
        {
            const int ArenaCeilingThickness = 4;
            phase2ArenaWalls.gameObject.SetActive(true);
            int minI = (int)(arenaCenter.x - ArenaWidth / 2);
            int maxI = minI + (int)ArenaWidth;
            int minJ = (int)(arenaCenter.y + ArenaHeight / 2);
            int maxJ = minJ + ArenaCeilingThickness;
            for (int i = minI; i < maxI; i++)
            {
                for (int j = minJ; j < maxJ; j++)
                {
                    Vector3Int tilePos = new(i, j);
                    mainTilemap.SetTile(tilePos, null);
                }
            }
            for (int i = minJ; i < maxJ; i++)
            {
                Vector3Int tilePos = new(minI - 1, i);
                mainTilemap.SetTile(tilePos, leftEdgeTile);
                tilePos = new Vector3Int(maxI, i);
                mainTilemap.SetTile(tilePos, rightEdgeTile);

            }
        }
        GetCloudPlatformsPositionsForPhaseTransition(out Vector2 leftStart, out Vector2 middleStart, out Vector2 rightStart, out Vector2 leftEnd, out Vector2 middleEnd, out Vector2 rightEnd);
        float cloudPlatformRiseProgress = Easings.SqrOut(Mathf.InverseLerp(0f, 1f, stateTimer));
        leftPlatform.MoveTo(Vector2.LerpUnclamped(leftStart, leftEnd, cloudPlatformRiseProgress));
        middlePlatform.MoveTo(Vector2.LerpUnclamped(middleStart, middleEnd, cloudPlatformRiseProgress));
        rightPlatform.MoveTo(Vector2.LerpUnclamped(rightStart, rightEnd, cloudPlatformRiseProgress));
        float mainTilemapDescentProgress = Easings.SqrIn(Mathf.InverseLerp(0, 5f, stateTimer));
        Vector3 mainTilemapPos = mainTilemap.transform.position;
        mainTilemapPos.y = Mathf.Lerp(0, -4f, mainTilemapDescentProgress);
        mainTilemap.transform.position = mainTilemapPos;
        float zoomOutProgress = Easings.SqrInOut(Mathf.InverseLerp(0f, 1f, stateTimer));
        TyphoonCameraSystem.SetTyphoonBossPhase2ZoomOutProgress(zoomOutProgress);

        if (stateTimer < .35f)
        {
            transform.position = Helper.DecayVec3(transform.position, arenaCenter, 30);
        }
        else if (stateTimer < 2f)
        {
            float progress = Mathf.InverseLerp(0.35f, 1.5f, stateTimer);
            progress = Easings.InBack(progress);
            Vector2 targetPos = arenaCenter;
            targetPos.y += 10;
            targetPos = Vector2.LerpUnclamped(arenaCenter, targetPos, progress);
            transform.position = Helper.Decay(transform.position, targetPos, 30);
        }
        if (stateTimer > 6f)
        {
            TyphoonBackground.SetSpawnOffsetForBackgroundAndSpawnRateMultiplier(new Vector2(0, 20), 0.5f);
            SwitchState(StateIDPhase2RandomizeClouds, 1, 2f, 2f);//values don't really matter for this state.
        }
    }
    void State_Phase2Lightning()
    {
        if (StateJustStarted)
        {
            movementTargetPoint = arenaCenter;
            movementTargetPoint.y += 3f;//avoid colliding with the middle platform if it's in the high position
        }

        if (stateTimer < LightningPhase2Startup)
        {

        }
        else if (stateTimer < LightningPhase2Startup + LightningPhase2ActionRate * LightningPhase2ActionCount)
        {
            float relativeTimer = (stateTimer - LightningPhase2Startup) % LightningPhase2ActionRate;
            if (ShouldDoAction(LightningPhase2Startup, LightningPhase2ActionRate))
            {
                TyphoonBossCloudPlatform chosenPlatform = GetPlatformClosestToPlayer();
                Transform platformTransform = chosenPlatform.transform;
                movementTargetPoint = platformTransform.position;
                movementTargetPoint.y += TyphoonBossCloudPlatform.HalfColliderHeight + TyphoonBossCloudPlatform.ColliderYOffset;
                movementTargetPoint.y += LightningBossYDistFromGround;
                Vector2 targetPos = movementTargetPoint;
                lightningTargetPoint.Set(targetPos.x, targetPos.y - LightningBossYDistFromGround);
            }
            if (relativeTimer < LightningPhase2TelegraphDuration)
            {
                animator.CrossFade(AnimIDLightningPre, 0);
                Helper.TelegraphLightning(relativeTimer, transform.position + new Vector3(0, LightningSpawnYOffset), lightningTargetPoint, LightningPhase2TelegraphDuration, lightningTelegraphParticles);
            }
            else//if relativeTimer >= LightningPhase2TelegraphDuration
            {
                //change randomization to have 2 of each ID in a list, then randomize from choosing an element from the list, assigning it to a platform,
                //and then removing the "used" element from the list
                if (!lightningRenderer.LightningActive)
                {
                    CommonSounds.PlayRandom(thunderShoot, audioSource, 1f, 1f);
                    ScreenShakeManager.AddSmallShake();
                    animator.CrossFade(AnimIDLightning, 0);
                    EffectsHandler.SpawnSmallExplosion(arenaCenterTransform, FlipnoteColors.Yellow, lightningTargetPoint, LightningPhase2Duration);
                    lightningRenderer.ActivateAndSetAttributes(.1f, transform.position + new Vector3(0, LightningSpawnYOffset), lightningTargetPoint, LightningPhase2Duration);
                    Collider2D cloudCollider = Physics2D.OverlapCircle(lightningTargetPoint, 0.2f, Layers.Tiles);
                    if (cloudCollider != null && cloudCollider.TryGetComponent(out TyphoonBossCloudPlatform cloudPlatform))
                    {
                        cloudPlatform.BecomeHarmful(LightningPhase2Duration);
                    }
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
            transform.position = Helper.Decay(transform.position, movementTargetPoint, 20);
        }
        else if (stateTimer < LightningPhase2Startup + LightningPhase2ActionRate * LightningPhase2ActionCount + LightningPhase2Delay)
        {
            animator.CrossFade(AnimIDIdle, 0);
            movementTargetPoint = arenaCenter;
            movementTargetPoint.y += 3f;//avoid colliding with the middle platform if it's in the high position
        }
        else
        {
            SwitchState(StateIDPhase2LightningShot, LightningShotPhase2ActionCount, LightningShotPhase2ActionRate, LightningShotPhase2Startup);
        }
        transform.position = Helper.DecayVec3(transform.position, movementTargetPoint, 25f);
    }
    void State_Phase2LightningShot()
    {
        if (stateTimer < LightningShotPhase2Startup)
        {
            int arenaSideSign = (int)Mathf.Repeat(actionCounter, 2) * 2 - 1;
            movementTargetPoint = arenaCenter + new Vector2(arenaSideSign * (Phase2ArenaWidth / 2f - 2f), 0);
        }
        else if (stateTimer < LightningShotPhase2Startup + LightningShotPhase2ActionCount * LightningShotPhase2ActionRate)
        {
            int arenaSideSign = (int)Mathf.Repeat(actionCounter, 2) * 2 - 1;
            movementTargetPoint = arenaCenter + new Vector2(arenaSideSign * (Phase2ArenaWidth / 2f - 2f), 0);
            movementTargetPoint.y = GameManager.PlayerPosition.y;
            if (ShouldDoAction(LightningShotPhase2Startup, LightningShotPhase2ActionRate))
            {
                CommonSounds.PlayRandom(thunderShoot, audioSource, 1f, 1f);
                Vector2 vel = new(-arenaSideSign * LightningShotsProjVelocity, 0);
                Vector2 from = transform.position;
                const float NumShots = 3;
                const float ShotsHeight = 1f;
                for (int i = 0; i < NumShots; i++)
                {

                    Vector2 offset = new(0, Helper.Remap(i, 0, NumShots - 1, -ShotsHeight / 2f, ShotsHeight / 2f));
                    FireLightningProj(vel, from + offset);
                }
            }
        }
        else if (stateTimer < LightningShotPhase2Startup + LightningShotPhase2ActionCount * LightningShotPhase2ActionRate + LightningShotPhase2Delay)
        {

        }
        else
        {
            SwitchState(StateIDPhase2LightningShotsPlatformStaircase, LightningShotsPlatformStaircasePhase2ActionCount, LightningShotsPlatformStaircasePhase2ActionRate, LightningShotsPlatformStaircasePhase2Startup);
        }
        transform.position = Helper.DecayVec3(transform.position, movementTargetPoint, 25f);

    }
    void State_Phase2LightningShotsPlatformStaircase()
    {
        if (StateJustStarted)
        {
            sbyte previousLeftPlatformTargetID = leftPlatformTargetID;
            sbyte previousRightPlatformTargetID = rightPlatformTargetID;
            sbyte previousMiddlePlatformTargetID = middlePlatformTargetID;
            StaircasePlatformTargetIDs();
            leftPlatform.EnableAppropriateMovementIndicator(previousLeftPlatformTargetID, leftPlatformTargetID);
            middlePlatform.EnableAppropriateMovementIndicator(previousMiddlePlatformTargetID, middlePlatformTargetID);
            rightPlatform.EnableAppropriateMovementIndicator(previousRightPlatformTargetID, rightPlatformTargetID);
        }
        if (stateTimer > LightningShotsPlatformStaircasePhase2CloudMovementTelegraphDuration)
        {
            MoveCloudsTowardsTheirTargetPositions();
            leftPlatform.DisableMovementIndicators();
            middlePlatform.DisableMovementIndicators();
            rightPlatform.DisableMovementIndicators();
        }

        if (stateTimer < LightningShotsPlatformStaircasePhase2CloudMovementTelegraphDuration + LightningShotsPlatformPhase2StaircaseTimeToGetIntoPosition)
        {
            movementTargetPoint = arenaCenter;
            //leftPlatformTargetID will only ever be 0 or 2 here, so this transforms it into a range of -1 to 1
            movementTargetPoint.x += (leftPlatformTargetID - 1) * (Phase2ArenaWidth / 2f - 1f);
            movementTargetPoint.y += Phase2ArenaHeight / 2f;
        }
        else if (stateTimer < LightningShotsPlatformStaircasePhase2Startup + LightningShotsPlatformStaircasePhase2ActionRate * LightningShotsPlatformStaircasePhase2ActionCount)
        {
            //leftPlatformTargetID will only ever be 0 or 2 here, so this transforms it into a range of -1 to 1
            int arenaSideSign = (leftPlatformTargetID - 1);
            movementTargetPoint = arenaCenter;
            movementTargetPoint.x += arenaSideSign * (Phase2ArenaWidth / 2f - 1f);
            movementTargetPoint.y += Phase2ArenaHeight / 2f;
            if (ShouldDoAction(LightningShotsPlatformStaircasePhase2Startup, LightningShotsPlatformStaircasePhase2ActionRate))
            {
                CommonSounds.PlayRandom(thunderShoot, audioSource, 1f, 1f);
                const float ProjAmount = 10;
                Vector2 center = transform.position;
                Vector2 lowerPart = arenaCenter + new Vector2((Phase2ArenaWidth * 0.5f - 5) * arenaSideSign, -Phase2ArenaHeight / 2f + 6f) - center;
                Vector2 upperPart = arenaCenter + new Vector2(Phase2ArenaWidth * -arenaSideSign * 0.5f, Phase2ArenaHeight / 2f - 1f) - center;
                lowerPart = lowerPart.normalized * LightningShotsProjVelocity;
                upperPart = upperPart.normalized * LightningShotsProjVelocity;
                for (int i = 0; i < ProjAmount; i++)
                {
                    float t = i / (ProjAmount - 1);
                    Vector2 vel = Vector2.Lerp(lowerPart, upperPart, Easings.CubeOut(t));
                    vel *= Mathf.Lerp(.3f, 1f, t);
                    TyphoonEnemyTornadoLightning proj = FireLightningProj(vel);
                    if (proj != null)
                    {
                        proj.SetLifetime(10f);
                    }
                }
            }
        }
        else if (stateTimer < LightningShotsPlatformStaircasePhase2Startup + LightningShotsPlatformStaircasePhase2ActionRate * LightningShotsPlatformStaircasePhase2ActionCount + LightningShotsPlatformStaircasePhase2Delay)
        {
            movementTargetPoint = arenaCenter;
            movementTargetPoint.y += 3;
        }
        else
        {
            SwitchState(StateIDPhase2RandomizeClouds, 2, 2f, 2f);//values don't matter for randomize clouds state. Only the state ID matters
        }
        transform.position = Helper.Decay(transform.position, movementTargetPoint, 20f);
    }
    void State_RandomizeClouds()
    {
        if (StateJustStarted)
        {
            sbyte previousLeftPlatformTargetID = leftPlatformTargetID;
            sbyte previousMiddlePlatformTargetID = middlePlatformTargetID;
            sbyte previousRightPlatformTargetID = rightPlatformTargetID;
        RandomizationStart:
            GetRandomPlatformHeightID(out leftPlatformTargetID, out middlePlatformTargetID, out rightPlatformTargetID);
            int amountOfDifference = 0;
            if (previousLeftPlatformTargetID != leftPlatformTargetID)
            {
                amountOfDifference++;
            }
            if (previousMiddlePlatformTargetID != middlePlatformTargetID)
            {
                amountOfDifference++;
            }
            if (previousRightPlatformTargetID != rightPlatformTargetID)
            {
                amountOfDifference++;
            }
            if (amountOfDifference < 1)
            {
                goto RandomizationStart;
            }
            leftPlatform.EnableAppropriateMovementIndicator(previousLeftPlatformTargetID, leftPlatformTargetID);
            middlePlatform.EnableAppropriateMovementIndicator(previousMiddlePlatformTargetID, middlePlatformTargetID);
            rightPlatform.EnableAppropriateMovementIndicator(previousRightPlatformTargetID, rightPlatformTargetID);
        }
        if (stateTimer > 1f)
        {
            MoveCloudsTowardsTheirTargetPositions();
        }
        if (stateTimer > 2f)
        {
            GetCloudPlatformsPositionsForRandomization(out Vector2[] leftPlatformPositions, out Vector2[] middlePlatformPositions, out Vector2[] rightPlatformPositions);
            leftPlatform.MoveTo(leftPlatformPositions[leftPlatformTargetID]);
            middlePlatform.MoveTo(middlePlatformPositions[middlePlatformTargetID]);
            rightPlatform.MoveTo(rightPlatformPositions[rightPlatformTargetID]);
            //SwitchState(StateIDPhase2RandomizeClouds, 3, 3, 3);//test line

            SwitchState(StateIDPhase2Lightning, LightningPhase2ActionCount, LightningPhase2ActionRate, LightningPhase2Startup);
            leftPlatform.DisableMovementIndicators();
            middlePlatform.DisableMovementIndicators();
            rightPlatform.DisableMovementIndicators();
        }
    }
    private void MoveCloudsTowardsTheirTargetPositions()
    {
        //don't move them too fast or else player can fall through them
        const float CloudPlatformMoveSpeed = 5f;
        GetCloudPlatformsPositionsForRandomization(out Vector2[] leftPlatformPositions, out Vector2[] middlePlatformPositions, out Vector2[] rightPlatformPositions);
        leftPlatform.MoveTo(Helper.Decay(leftPlatform.transform.position, leftPlatformPositions[leftPlatformTargetID], CloudPlatformMoveSpeed));
        middlePlatform.MoveTo(Helper.Decay(middlePlatform.transform.position, middlePlatformPositions[middlePlatformTargetID], CloudPlatformMoveSpeed));
        rightPlatform.MoveTo(Helper.Decay(rightPlatform.transform.position, rightPlatformPositions[rightPlatformTargetID], CloudPlatformMoveSpeed));
    }
    void StaircasePlatformTargetIDs()
    {
        if (leftPlatformTargetID == 0 && middlePlatformTargetID == 1 && rightPlatformTargetID == 2)
        {
            rightPlatformTargetID = 0;
            leftPlatformTargetID = 2;
            return;
        }
        if (leftPlatformTargetID == 2 && middlePlatformTargetID == 1 && rightPlatformTargetID == 0)
        {
            rightPlatformTargetID = 2;
            leftPlatformTargetID = 0;
            return;
        }
        sbyte rand = (sbyte)(Random.Range(0, 2) * 2);
        //if rand is 2, then it goes 0, 1, 2
        //if rand is 0, then it goes 2, 1, 0
        leftPlatformTargetID = (sbyte)(2 - rand);
        middlePlatformTargetID = 1;
        rightPlatformTargetID = rand;
    }
    public void ChangeToIntro()
    {
        GetComponent<Collider2D>().enabled = true;
        state = StateIDIntro;
        state = StateIDIntro;
        actionTimer = stateTimer = 0;
    }
    void Phase2PreUpdates()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.O))
        {
            life = LifeMax / 2 - 1;
            UIManager.UpdateBossLifeBar(LifePercent);
        }
#endif
        if (!InPhase2OrTransitionToIt)
        {
            return;
        }
        const float MaxScrollSpeedForPhase2 = 12f;
        float scrollSpeed = MaxScrollSpeedForPhase2;
        float scrollSpeedMultForBossItself = 1f;
        if (state == StateIDTransitionToPhase2)
        {
            scrollSpeed = Helper.Remap(stateTimer, 4f, 5.5f, 0, MaxScrollSpeedForPhase2);
            scrollSpeedMultForBossItself = Mathf.InverseLerp(5f, 6f, stateTimer);
        }
        scrollSpeed *= Time.deltaTime;
        Vector3 pos;        //DO NOT CHANGE FROM VECTOR3!!!
        pos = GameManager.PlayerControl.transform.position;
        pos.y += scrollSpeed;
        GameManager.PlayerControl.transform.position = pos;
        if (scrollSpeedMultForBossItself < 1f)
        {
            pos = transform.position;
        }
        else
        {
            pos = transform.parent.position;
        }
        pos.y += scrollSpeed * scrollSpeedMultForBossItself;
        if (scrollSpeedMultForBossItself < 1f)
        {
            transform.position = pos;
        }
        else
        {
            transform.parent.position = pos;
        }
        pos = arenaCenterTransform.position;
        pos.y += scrollSpeed;
        arenaCenterTransform.position = pos;
        arenaCenter.Set(pos.x, pos.y);
        movementTargetPoint.y += scrollSpeed;
        pos = GameManager.CurrentCamTransform.position;
        pos.y += scrollSpeed;
        GameManager.CurrentCamTransform.position = pos;
        //using PlayerPosition wasn't working properly (was just freezing the player), so use PlayerControl.transform.position here.

        pos = phase2ArenaWalls.transform.position;
        if (pos.y < arenaCenter.y)
        {
            pos.y = Mathf.Ceil(arenaCenter.y);
            phase2ArenaWalls.transform.position = pos;
        }
        Transform playerProjPoolTransform = GameManager.PlayerProjsPool.transform;
        pos = playerProjPoolTransform.position;
        pos.y += scrollSpeed;
        playerProjPoolTransform.position = pos;

        lightningTargetPoint.y += scrollSpeed;
        lightningRenderer.Move(scrollSpeed);
        //fix telegraph particle positions
        //don't need to worry about cloud platform movement here because player also gets moved by the same amount
    }
    void SwitchState(byte nextStateID, sbyte nextStateActionCount, float nextStateActionRate, float nextStateStartup)
    {
        //it's in this method so that it doesn't cut at any state, and instead starts phase 2 at the next state switch
        if (state < StateIDTransitionToPhase2 && ShouldBeInPhase2 && !InPhase2OrTransitionToIt)
        {
            nextStateID = StateIDTransitionToPhase2;
            nextStateActionCount = 2;//2 actions, the cloud platforms rise, and the tilemap adjust
        }
        state = nextStateID;
        stateTimer = -Time.deltaTime;
        actionTimer = nextStateActionRate - Time.deltaTime;
        actionCounter = nextStateActionCount;
    }
    void TrySwitchState(byte nextStateID, int currentActionCount, float currentStartup, float currentActionRate, float currentExtraDelay, sbyte nextStateActionCount, float nextStateActionRate, float nextStateStartup)
    {
        if (stateTimer > currentActionCount * currentActionRate + currentStartup + currentExtraDelay)
        {
            SwitchState(nextStateID, nextStateActionCount, nextStateActionRate, nextStateStartup);
        }
    }
    bool ShouldDoAction(float startup, float actionRate)
    {
        if (actionCounter < 0)
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
            // Use the generalized quadratic function: f(x) = a * x^p
            return a * Mathf.Pow(x, p);
        }
        else
        {
            // Use the linear function: g(x) = m * x + n
            return linearSlope * x + n;
        }
    }
    public override bool PreKill()
    {
        lightningRenderer.Stop();
        PlayerWeaponManager.UnlockTyphoon();
        BossHelper.BossDeath(gameObject, this, null, FlipnoteColors.Blue, arenaCenter);
        return false;
    }
    static float Easing010(float progress)
    {
        progress *= 2;
        progress--;
        progress *= progress;
        progress = 1 - progress;
        return Mathf.Max(0, progress);
    }
    void GetCloudPlatformsPositionsForPhaseTransition(out Vector2 leftStart, out Vector2 middleStart, out Vector2 rightStart, out Vector2 leftEnd, out Vector2 middleEnd, out Vector2 rightEnd)
    {
        Vector2 center = arenaCenter;
        center.y -= 9;
        leftStart = center;
        middleStart = center;
        rightStart = center;
        leftStart.x -= 6;
        rightStart.x += 6;
        center.y += 5;
        leftEnd = center;
        middleEnd = center;
        rightEnd = center;
        leftEnd.x -= 6;
        rightEnd.x += 6;
    }
    void GetCloudPlatformsPositionsForRandomization(out Vector2 bottomLeft, out Vector2 bottomCenter, out Vector2 bottomRight, out Vector2 middleLeft, out Vector2 middleCenter, out Vector2 middleRight, out Vector2 topLeft, out Vector2 topCenter, out Vector2 topRight)
    {
        const float BottomY = -5;
        const float MiddleY = -2;
        const float TopY = 1;
        const float LeftX = -6;
        const float CenterX = 0;
        const float RightX = 6;
        Vector2 center = arenaCenter;
        bottomLeft = new Vector2(center.x + LeftX, center.y + BottomY);
        bottomCenter = new Vector2(center.x + CenterX, center.y + BottomY);
        bottomRight = new Vector2(center.x + RightX, center.y + BottomY);
        middleLeft = new Vector2(center.x + LeftX, center.y + MiddleY);
        middleCenter = new Vector2(center.x + CenterX, center.y + MiddleY);
        middleRight = new Vector2(center.x + RightX, center.y + MiddleY);
        topLeft = new Vector2(center.x + LeftX, center.y + TopY);
        topCenter = new Vector2(center.x + CenterX, center.y + TopY);
        topRight = new Vector2(center.x + RightX, center.y + TopY);
    }
    void GetCloudPlatformsPositionsForRandomization(out Vector2[] bottomLeftMiddleLeftTopLeft, out Vector2[] bottomCenterMiddleCenterTopCenter, out Vector2[] bottomRightMiddleRightTopRight)
    {
        const float BottomY = -5;
        const float MiddleY = -2;
        const float TopY = 1;
        const float LeftX = -6;
        const float CenterX = 0;
        const float RightX = 6;

        Vector2 center = arenaCenter;
        bottomLeftMiddleLeftTopLeft = new Vector2[3];
        bottomCenterMiddleCenterTopCenter = new Vector2[3];
        bottomRightMiddleRightTopRight = new Vector2[3];

        bottomLeftMiddleLeftTopLeft[0] = new Vector2(center.x + LeftX, center.y + BottomY);
        bottomLeftMiddleLeftTopLeft[1] = new Vector2(center.x + LeftX, center.y + MiddleY);
        bottomLeftMiddleLeftTopLeft[2] = new Vector2(center.x + LeftX, center.y + TopY);
        bottomCenterMiddleCenterTopCenter[0] = new Vector2(center.x + CenterX, center.y + BottomY);
        bottomCenterMiddleCenterTopCenter[1] = new Vector2(center.x + CenterX, center.y + MiddleY);
        bottomCenterMiddleCenterTopCenter[2] = new Vector2(center.x + CenterX, center.y + TopY);
        bottomRightMiddleRightTopRight[0] = new Vector2(center.x + RightX, center.y + BottomY);
        bottomRightMiddleRightTopRight[1] = new Vector2(center.x + RightX, center.y + MiddleY);
        bottomRightMiddleRightTopRight[2] = new Vector2(center.x + RightX, center.y + TopY);
    }
    TyphoonBossCloudPlatform GetPlatformClosestToPlayer()
    {
        TyphoonBossCloudPlatform result = rightPlatform;
        Vector3 playerPos = GameManager.PlayerPosition;
        if ((middlePlatform.transform.position - playerPos).sqrMagnitude < (result.transform.position - playerPos).sqrMagnitude)
        {
            result = middlePlatform;
        }
        if ((leftPlatform.transform.position - playerPos).sqrMagnitude < (result.transform.position - playerPos).sqrMagnitude)
        {
            result = leftPlatform;
        }
        return result;
    }
    void GetRandomPlatformHeightID(out sbyte first, out sbyte second, out sbyte third)
    {
        List<sbyte> list = new() { 0, 0, 1, 1, 2, 2 };
        int randomIndex = Random.Range(0, list.Count);
        first = list[randomIndex];
        list.RemoveAt(randomIndex);
        randomIndex = Random.Range(0, list.Count);
        second = list[randomIndex];
        list.RemoveAt(randomIndex);
        third = list[Random.Range(0, list.Count)];

        if (Mathf.Abs(first - second) > 1 || Mathf.Abs(third - second) > 1)
        {
            second = PlatformTargetIDMiddle;
            return;
        }
    }
#if UNITY_EDITOR
    Color handlesColor;
    List<(Vector2 direction, Vector2 position)> debug_GizmoDisplayForLightningRound;
    private void OnDrawGizmos()
    {
        PreviewLightningHazardPositionAndRotations();
        Gizmos.color = Color.yellow;
        if (debug_GizmoDisplayForLightningRound != null && debug_GizmoDisplayForLightningRound.Count > 0)
        {
            for (int i = 0; i < debug_GizmoDisplayForLightningRound.Count; i++)
            {
                (Vector2 direction, Vector2 position) = debug_GizmoDisplayForLightningRound[i];
                Gizmos2.DrawArrow(position, direction);
                Gizmos2.DrawArrow(position, -direction);
            }
        }
        Handles.color = handlesColor;
    }
    private void PreviewLightningHazardPositionAndRotations()
    {
        float minAngle = -LightningHazardHalfMaxAngle - Mathf.PI / 2f;
        float maxAngle = LightningHazardHalfMaxAngle - Mathf.PI / 2f;
        Vector2 minAngleDirection = minAngle.PolarVector(.5f);
        Vector2 maxAngleDirection = maxAngle.PolarVector(.5f);
        Vector2 hazardCenter = arenaCenterTransform.position;
        for (int i = 0; i < LightningHazardAmountOfHazardsPerRound; i++)
        {
            hazardCenter.x += Helper.Remap(i, 0, LightningHazardAmountOfHazardsPerRound - 1, -ArenaWidth / 2f + 1.5f, ArenaWidth / 2f - 1.5f);
            hazardCenter.x += -.5f; //Random2.Float(-5f, .5f);
            Vector2 lineEnd = hazardCenter;
            lineEnd.x++;
            Gizmos2.DrawArrow(lineEnd, maxAngleDirection);
            Gizmos2.DrawArrow(hazardCenter, minAngleDirection);
            minAngleDirection.y *= -1;
            maxAngleDirection.y *= -1;
            Gizmos2.DrawArrow(lineEnd, maxAngleDirection);
            Gizmos2.DrawArrow(hazardCenter, minAngleDirection);
            Gizmos.DrawLine(hazardCenter, lineEnd);
            hazardCenter = arenaCenterTransform.position;
        }
        hazardCenter.x += Helper.Remap(0, 0, LightningHazardAmountOfHazardsPerRound - 1, -ArenaWidth / 2f + 1.5f, ArenaWidth / 2f - 1.5f);
        hazardCenter.x += -0.5f; //Random2.Float(-5f, .5f);
    }
#endif
}
