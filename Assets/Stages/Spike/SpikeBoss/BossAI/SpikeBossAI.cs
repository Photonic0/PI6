using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Helpers;
using Assets.Systems;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpikeBossAI : Enemy
{
    static readonly int AnimIDIdle = Animator.StringToHash("Idle");
    static readonly int AnimIDThrow1 = Animator.StringToHash("Throw1");
    static readonly int AnimIDThrow2 = Animator.StringToHash("Throw2");
    static readonly int AnimIDThrow3 = Animator.StringToHash("Throw3");
    static readonly int AnimIDMidairLeft = Animator.StringToHash("MidairLeft");
    static readonly int AnimIDMidairRight = Animator.StringToHash("MidairRight");
    static readonly int AnimIDMidairUp = Animator.StringToHash("MidairUp");
    static readonly int AnimIDMidairDown = Animator.StringToHash("MidairDown");
    static readonly int AnimIDSlamCharge = Animator.StringToHash("SlamCharge");
    static readonly int AnimIDSlamFall = Animator.StringToHash("SlamFall");
    static readonly int AnimIDSlamGrounded = Animator.StringToHash("SlamGrounded");
    static readonly int AnimIDSlamGroundedToIdle = Animator.StringToHash("SlamGroundedToIdle");
    static readonly int AnimIDWalk = Animator.StringToHash("Walk");
    //add swinging spike ball attack
    //
    [SerializeField] new Transform transform;
    [SerializeField] Transform arenaCenterTransform;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] float stateTimer;
    [SerializeField] float actionTimer;
    [SerializeField] int actionCounter;
    [SerializeField] short state;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource footstepAudioSource;
    [SerializeField] SpikeBossSpikeBall[] spikeBalls;//make it do like a triple throw, with 2 additional spike balls thrown with rotated angles
    [SerializeField] FallingSpike[] fallingSpikePool;
    [SerializeField] SpikeBossSwingingSpikeBall swingingSpikeBall;
    SpikeBossSpikeBall currentSpikeBall;
    bool[] whichSpikesFallOnlastCeilingSpikeWave;
    Vector2 arenaCenter;

#if UNITY_EDITOR
    Vector2 debug_jumpVelocity;
#endif

    const float WalkSpeed = 7;
    const float ArenaWidth = 16;
    const float ArenaHeight = 8;

    const short StateIDStart = 0;
    const short StateIDIntro = 1;
    const short StateIDSpikeBallThrow = 2;
    const short StateIDSpikeShockwave = 3;
    const short StateIDJumpLeftToRight = 4;
    const short StateIDJumpRightToLeft = 5;
    const short StateIDCeilingSpikes = 6;
    const short StateIDSwingingSpikeBall = 7;

    const float IntroDuration = 1;

    const float SpikeBallThrowJumpHeight = 5;
    const float SpikeBallThrowStartup = 0.3f;
    const float SpikeBallThrowRate = 0.9f;
    const int SpikeBallThrowActionCount = 3;
    const float SpikeBallThrowDelay = .2f;
    const float SpikeBallThrowVelocity = 17;
    const float SpikeBallThrowDistNeededForSideSwitch = 4.5f;
    const float SpikeBallThrowArcRadians = 0.35f;


    const int SpikeShockwaveActionCount = 1;//The action is the shockwave creation
    const float SpikeShockwaveGoToAirStallPointDuration = .4f;
    const float SpikeShockwaveAirStallHeight = 5.5f;
    const float SpikeShockwaveAirStallDuration = .35f;
    const float SpikeShockwaveDownMovementDuration = .1f;
    const float SpikeShockwaveGroundStayDuration = .25f;

    const float JumpToOtherSideDuration = .7f;
    const float JumpToOtherSideJumpHeight = 6;

    const float CeilingSpikesStartup = 0;
    const int CeilingSpikesActionCount = 7;
    const float CeilingSpikesActionRate = 0.5f;
    const float CeilingSpikesDelay = 1.5f;
    const int CeilingSpikesMaxSpikesInRow = 4;

    const float SwingingSpikeBallStartup = 0;
    const int SwingingSpikeBallActionCount = 8;
    const float SwingingSpikeBallActionRate = .4f;
    const float SwingingSpikeBallDelay = 2f;
    const float SwingingSpikeBallSwingDuration = 2f;

    public override int LifeMax => 70;
    bool StateJustStarted => stateTimer >= 0 && stateTimer < 1E-20f;
    bool IsOnLeftSideOfArena => transform.position.x < arenaCenter.x;
    bool IsOnRightSideOfArena => transform.position.x > arenaCenter.x;
    bool IsOnMiddleOfArena => transform.position.x == arenaCenter.x;
    float CurrentArenaSideSign => Mathf.Sign(transform.position.x - arenaCenter.x);
    float DirectionSign => sprite.flipX ? -1 : 1;
    bool FlipX => transform.position.x < GameManager.PlayerPosition.x;
    Vector2 CurrentArenaSidePoint => IsOnLeftSideOfArena ? (arenaCenter - new Vector2(ArenaWidth / 2 - 2, ArenaHeight / 2 - 1)) : (arenaCenter + new Vector2(ArenaWidth / 2 - 2, -ArenaHeight / 2 + 1));

    public override void Start()
    {
        DiscardCurrentSpikeBall();
        base.Start();
    }
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
            case StateIDSpikeBallThrow:
                State_SpikeBallThrow();
                break;
            case StateIDSpikeShockwave:
                State_SpikeShockwave();
                break;
            case StateIDJumpLeftToRight:
            case StateIDJumpRightToLeft:
                State_JumpToOtherSide();
                break;
            case StateIDCeilingSpikes:
                State_CeilingSpikes();
                break;
            case StateIDSwingingSpikeBall:
                State_SwingingSpikeBall();
                break;
        }
        stateTimer += Time.deltaTime;
        actionTimer += Time.deltaTime;
    }
    private void State_Intro()
    {
        animator.CrossFade(AnimIDIdle, 0);
        if (StateJustStarted)
        {
            UIManager.ActivateBossLifeBar(FlipnoteColors.Yellow);
        }
        UIManager.UpdateBossLifeBar(Mathf.Clamp01(stateTimer / IntroDuration));
        if (stateTimer > IntroDuration)
        {
            SwitchState(StateIDSpikeBallThrow, SpikeBallThrowActionCount);
        }
        //Action[] testAction = new Action[] { State_Start, State_SpikeThrow };
        //testAction[1]();
    }
    private void State_Start()
    {
        animator.CrossFade(AnimIDIdle, 0);
        if (arenaCenter == default)
        {
            arenaCenter = arenaCenterTransform.position;
        }
    }
    private void State_SpikeBallThrow()
    {
        //float stateProgress = stateTimer / (SpikeBallThrowStartup + SpikeBallThrowRate * SpikeBallThrowCount);

        float jumpPoint = ParabolaFrom0to1(Mathf.InverseLerp(0, SpikeBallThrowRate, (stateTimer - SpikeBallThrowStartup * 2f) % SpikeBallThrowRate));
        if (actionCounter <= 0 && stateTimer > SpikeBallThrowStartup * 2 + SpikeBallThrowRate * SpikeBallThrowActionCount)
        {
            jumpPoint = 0;
        }
        Vector2 position = CurrentArenaSidePoint + new Vector2(0, jumpPoint * SpikeBallThrowJumpHeight);
        transform.position = position;
        if (actionTimer > SpikeBallThrowStartup)
        {
            float preThrowProgress = (actionTimer - SpikeBallThrowStartup) / SpikeBallThrowRate;
            if (preThrowProgress < 1 / 3f)
            {
                if (actionCounter < SpikeBallThrowActionCount)//if has already thrown at least 1 spike ball
                {
                    animator.CrossFade(AnimIDThrow3, 0);
                }
            }
            else if (preThrowProgress < 2 / 3f)
            {
                if (currentSpikeBall == null && actionCounter > 0)
                {
                    PrepareSpikeBall();
                }
                if (currentSpikeBall != null)
                {
                    Vector3 throwPos = transform.position + new Vector3(0, 1);
                    currentSpikeBall.transform.position = throwPos;
                }
                animator.CrossFade(AnimIDThrow1, 0);
            }
            else
            {
                if (currentSpikeBall == null && actionCounter > 0)
                {
                    PrepareSpikeBall();
                }
                if (currentSpikeBall != null)
                {
                    Vector3 throwPos = transform.position + new Vector3(DirectionSign, 1);
                    currentSpikeBall.transform.position = throwPos;
                }
                animator.CrossFade(AnimIDThrow2, 0);
            }
        }
        if (ShouldDoAction(SpikeBallThrowStartup, SpikeBallThrowRate))
        {
            Vector2 primaryBallThrowVelocity = Vector2.zero;
            Vector3 throwPos = transform.position + new Vector3(0, 1);
            if (currentSpikeBall != null)
            {
                currentSpikeBall.transform.position = throwPos;
                primaryBallThrowVelocity = LimitDirection(throwPos, GameManager.PlayerPosition) * SpikeBallThrowVelocity;
                currentSpikeBall.rb.velocity = primaryBallThrowVelocity;
                currentSpikeBall.EnablePhysics();
                currentSpikeBall.dontSpawnShockwave = false;
                currentSpikeBall = null;
                audioSource.transform.position = throwPos;
                CommonSounds.PlayThrowSound(audioSource);
            }
            for (int i = -1; i < 2; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                if (Helper.TryFindFreeIndex(spikeBalls, out int index))
                {
                    SpikeBossSpikeBall ball = spikeBalls[index];
                    float angleDiff = Helper.Remap(i, -1, 1, -SpikeBallThrowArcRadians * .5f, SpikeBallThrowArcRadians * .5f);
                    ball.EnablePhysics();
                    ball.gameObject.SetActive(true);
                    ball.rb.velocity = primaryBallThrowVelocity.RotatedBy(angleDiff);
                    ball.dontSpawnShockwave = true;
                    ball.transform.position = throwPos;
                }
            }
        }
        if (Mathf.Abs(transform.position.x - GameManager.PlayerPosition.x) < SpikeBallThrowDistNeededForSideSwitch)
        {
            short nextState = IsOnLeftSideOfArena ? StateIDJumpLeftToRight : StateIDJumpRightToLeft;
            TrySwitchState(nextState, SpikeBallThrowActionCount, SpikeBallThrowStartup, SpikeBallThrowRate, SpikeBallThrowDelay + SpikeBallThrowRate * .5f, (int)CurrentArenaSideSign);
        }
        else
        {
            TrySwitchState(StateIDSpikeShockwave, SpikeBallThrowActionCount, SpikeBallThrowStartup, SpikeBallThrowRate, SpikeBallThrowDelay + SpikeBallThrowRate * .5f, SpikeShockwaveActionCount);
        }
    }
    private void State_SpikeShockwave()
    {
        if (stateTimer < SpikeShockwaveGoToAirStallPointDuration)
        {
            Vector2 airStallPos = arenaCenter + new Vector2(0, -ArenaHeight / 2 + 1 + SpikeShockwaveAirStallHeight);
            float progress = Mathf.InverseLerp(0, SpikeShockwaveGoToAirStallPointDuration, stateTimer);
            float progressNoEasing = progress;
            progress = Easings.SqrOut(progress);

            GetArenaPoints(out Vector2 leftStandingPoint, out Vector2 rightStandingPoint);
            Vector2 from = IsOnRightSideOfArena ? rightStandingPoint : leftStandingPoint;
            Vector2 position = from;
            position.x = Mathf.Lerp(from.x, airStallPos.x, progress);
            position.y = Mathf.Lerp(from.y, airStallPos.y, Easings.SqrOut(progress));
            transform.position = position;
            float derivativeY = 1 - progressNoEasing;
            derivativeY *= derivativeY * derivativeY;
            derivativeY *= 4;
            float dirXSign = IsOnRightSideOfArena ? 1 : -1;
            MidairAnim(new Vector2(2 * dirXSign - 2 * dirXSign * progressNoEasing, derivativeY).normalized);
        }
        else if (stateTimer < SpikeShockwaveAirStallDuration + SpikeShockwaveGoToAirStallPointDuration)
        {
            Vector2 airStallPos = arenaCenter + new Vector2(0, -ArenaHeight / 2 + 1 + SpikeShockwaveAirStallHeight);

            animator.CrossFade(AnimIDSlamCharge, 0);
            transform.position = airStallPos;
        }
        else if (stateTimer < SpikeShockwaveDownMovementDuration + SpikeShockwaveAirStallDuration + SpikeShockwaveGoToAirStallPointDuration)
        {
            Vector2 airStallPos = arenaCenter + new Vector2(0, -ArenaHeight / 2 + 1 + SpikeShockwaveAirStallHeight);

            animator.CrossFade(AnimIDSlamFall, 0);
            transform.position = Vector2.Lerp(airStallPos, arenaCenter - new Vector2(0, ArenaHeight / 2 - 1),// -1 so that the boss doesn't clip into the floor.
                                                                                                             // it's -1 and not +1 because it will be subtracting less from the Y position
                Easings.SqrIn((stateTimer - SpikeShockwaveGoToAirStallPointDuration - SpikeShockwaveAirStallDuration) / SpikeShockwaveDownMovementDuration));
        }
        else if (stateTimer < SpikeShockwaveGroundStayDuration + SpikeShockwaveDownMovementDuration + SpikeShockwaveAirStallDuration + SpikeShockwaveGoToAirStallPointDuration)
        {
            if (actionCounter > 0)
            {
                ScreenShakeManager.AddMediumShake();
                Vector2 arenaFloorCenter = arenaCenter - new Vector2(0, ArenaHeight / 2 + .5f);
                SpikeWaveSpike.StartSpikeWave(arenaFloorCenter, 1f, 8, .3f, .15f, audioSource);
                arenaFloorCenter.y += 1.5f;
                transform.position = arenaFloorCenter;
                actionCounter = 0;
            }
            float relativeTimer = stateTimer - SpikeShockwaveDownMovementDuration - SpikeShockwaveAirStallDuration - SpikeShockwaveGoToAirStallPointDuration;
            float progress = relativeTimer / SpikeShockwaveGroundStayDuration;
            if (progress < .3334f)
            {
                animator.CrossFade(AnimIDSlamGrounded, 0);
            }
            else if (progress < .6667f)
            {
                animator.CrossFade(AnimIDSlamGroundedToIdle, 0);
            }
            else
            {
                animator.CrossFade(AnimIDIdle, 0);
            }
        }
        else
        {
            GetArenaPoints(out Vector2 leftStandingPoint, out Vector2 rightStandingPoint);
            Vector2 walkTarget = IsOnMiddleOfArena
                ? Random2.Bool ? leftStandingPoint : rightStandingPoint
                : IsOnLeftSideOfArena ? leftStandingPoint : rightStandingPoint;
            if (Mathf.Abs(transform.position.x - walkTarget.x) <= 1E-7f)
            {
                SwitchState(StateIDCeilingSpikes, CeilingSpikesActionCount);
                animator.CrossFade(AnimIDIdle, 0);
            }
            else
            {
                animator.CrossFade(AnimIDWalk, 0);
                sprite.flipX = Mathf.Sign(transform.position.x - walkTarget.x) < 0;
                transform.position = Vector3.MoveTowards(transform.position, walkTarget, Time.deltaTime * WalkSpeed);
            }

        }
    }
    private void State_JumpToOtherSide()
    {
        GetArenaPoints(out Vector2 leftStandingPoint, out Vector2 rightStandingPoint);
        float stateProgress = stateTimer / JumpToOtherSideDuration;
        int directionSign = 1;
        if (state == StateIDJumpRightToLeft)
        {
            directionSign = -1;
            stateProgress = 1 - stateProgress;
        }
        float derivative = ParabolaFrom0To1Derivative(stateProgress);
        Vector2 jumpVel = new Vector2(directionSign, derivative * directionSign) / JumpToOtherSideDuration;
#if UNITY_EDITOR
        debug_jumpVelocity = jumpVel;
#endif

        //vector and method already factors in left to right vs right to left
        MidairAnim(jumpVel.normalized);

        float jumpYOffset = ParabolaFrom0to1(stateProgress) * JumpToOtherSideJumpHeight;
        float jumpX = Mathf.Lerp(leftStandingPoint.x, rightStandingPoint.x, stateProgress);
        transform.position = new Vector2(jumpX, arenaCenter.y + jumpYOffset - ArenaHeight / 2 + 1);
        if (stateTimer > JumpToOtherSideDuration)
        {
            animator.CrossFade(AnimIDIdle, 0);
            if (Random2.OneIn(3))
            {
                SwitchState(StateIDSpikeBallThrow, SpikeBallThrowActionCount);
            }
            else if (Random2.Bool)
            {
                SwitchState(StateIDSpikeShockwave, SpikeShockwaveActionCount);
            }
            else
            {
                SwitchState(StateIDCeilingSpikes, CeilingSpikesActionCount);
            }
        }
    }
    void State_CeilingSpikes()
    {
        float jumpProgress = actionCounter <= 0 ? 1 : Mathf.InverseLerp(0, CeilingSpikesActionRate, (stateTimer - CeilingSpikesStartup * 2f) % CeilingSpikesActionRate);
        float jumpPoint = ParabolaFrom0to1(jumpProgress);
        if (actionCounter <= 0 && stateTimer > CeilingSpikesStartup * 2 + CeilingSpikesActionRate * CeilingSpikesActionCount)
        {
            jumpPoint = 0;
            animator.CrossFade(AnimIDIdle, 0);
        }
        else
        {
            float derivative = ParabolaFrom0To1Derivative(jumpProgress);
            if (derivative > 0)
            {
                animator.CrossFade(AnimIDMidairUp, 0);
            }
            else if (derivative < 0)
            {
                animator.CrossFade(AnimIDSlamFall, 0);
            }
            else
            {
                animator.CrossFade(AnimIDIdle, 0);
            }
        }
        Vector2 position = CurrentArenaSidePoint + new Vector2(0, jumpPoint * SpikeBallThrowJumpHeight);
        transform.position = position;
        if (StateJustStarted || ShouldDoAction(CeilingSpikesStartup, CeilingSpikesActionRate))
        {
            ScreenShakeManager.AddSmallShake();
            Vector2 spikePos = arenaCenter;
            spikePos.x -= 7.5f;
            spikePos.y += 3.5f;
            int amountToGenerate = Random.Range(2, CeilingSpikesMaxSpikesInRow + 1);
            bool dontPlaySpikeSound = false;
            if (whichSpikesFallOnlastCeilingSpikeWave == null)
            {
                whichSpikesFallOnlastCeilingSpikeWave = new bool[(int)ArenaWidth];
                for (int i = 0; i < ArenaWidth; i++)
                {
                    if (amountToGenerate > 0)
                    {
                        whichSpikesFallOnlastCeilingSpikeWave[i] = true;
                        SpawnSpike(spikePos, dontPlaySpikeSound);
                        dontPlaySpikeSound = true;
                        amountToGenerate--;
                        spikePos.x += 1;
                        continue;
                    }
                    spikePos.x += 2;
                    i++;
                    //2 as the minimum so the secondary wave where it drops spikes on places that spikes didn't spawn doesn't have any 1-block gaps
                    amountToGenerate = Random.Range(2, CeilingSpikesMaxSpikesInRow + 1);
                }
            }
            else
            {
                for (int i = 0; i < ArenaWidth; i++)
                {
                    if (!whichSpikesFallOnlastCeilingSpikeWave[i])
                    {
                        SpawnSpike(spikePos, dontPlaySpikeSound);
                        dontPlaySpikeSound = true;
                    }
                    spikePos.x += 1;
                }
                whichSpikesFallOnlastCeilingSpikeWave = null;
            }
        }
        TrySwitchState(StateIDSwingingSpikeBall, CeilingSpikesActionCount, CeilingSpikesStartup, CeilingSpikesActionRate, CeilingSpikesDelay, SwingingSpikeBallActionCount);
        if (state != StateIDCeilingSpikes)
        {
            whichSpikesFallOnlastCeilingSpikeWave = null;
        }
    }
    void State_SwingingSpikeBall()
    {
        if (StateJustStarted || ShouldDoAction(SwingingSpikeBallStartup, SwingingSpikeBallActionRate))
        {
            float delay = 0.9f;
            Vector2 spikePos = arenaCenter;
            spikePos.x -= 7.5f;
            spikePos.y += 3.5f;
            SpawnSpike(spikePos, delay);
            spikePos.x+= 2;
            SpawnSpike(spikePos, delay);
            spikePos.x += ArenaWidth - 5;
            SpawnSpike(spikePos, delay, false);
            spikePos.x+= 2;
            SpawnSpike(spikePos, delay);

            if (actionCounter == SwingingSpikeBallActionCount - 1)
            {
                swingingSpikeBall.StartAnimation(SwingingSpikeBallSwingDuration);
            }
            else if ((SwingingSpikeBallActionCount - actionCounter) % 2 == 0)
            {
                spikePos.x -= 3;
                SpawnSpike(spikePos, delay);
                spikePos.x -= 2;
                SpawnSpike(spikePos, delay);
                spikePos.x -= 5;
                SpawnSpike(spikePos, delay);
                spikePos.x -= 2;
                SpawnSpike(spikePos, delay);
            }

        }
        TrySwitchState(StateIDSpikeBallThrow, SwingingSpikeBallActionCount, SwingingSpikeBallStartup, SwingingSpikeBallActionRate, SwingingSpikeBallDelay, SpikeBallThrowActionCount);
    }
    void SpawnSpike(Vector2 pos, bool dontPlaySound = true)
    {
        for (int i = 0; i < fallingSpikePool.Length; i++)
        {
            FallingSpike spike = fallingSpikePool[i];
            if (!spike.enabled)
            {
                spike.transform.position = pos;
                spike.Start();
                spike.StartFallAndMakeNotRespawnAndNotDestroy(Random2.Float(0.7f, 0.8f), dontPlaySound);
                break;
            }
        }
    }
    void SpawnSpike(Vector2 pos, float delay, bool dontPlaySound = true)
    {
        delay += Random.value * .1f - 0.05f;
        for (int i = 0; i < fallingSpikePool.Length; i++)
        {
            FallingSpike spike = fallingSpikePool[i];
            if (!spike.enabled)
            {
                spike.transform.position = pos;
                spike.Start();
                spike.StartFallAndMakeNotRespawnAndNotDestroy(delay, dontPlaySound);
                break;
            }
        }
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
    void GetArenaPoints(out Vector2 leftStandingPoint, out Vector2 rightStandingPoint)
    {
        leftStandingPoint = arenaCenter - new Vector2(ArenaWidth / 2 - 2, ArenaHeight / 2 - 1);
        rightStandingPoint = arenaCenter + new Vector2(ArenaWidth / 2 - 2, -ArenaHeight / 2 + 1);
    }
    void DiscardCurrentSpikeBall()
    {
        if (currentSpikeBall != null)
        {
            currentSpikeBall.gameObject.SetActive(false);
            currentSpikeBall = null;
        }
    }
    void PrepareSpikeBall()
    {
        if (currentSpikeBall == null)
        {
            if (Helper.TryFindFreeIndex(spikeBalls, out int index))
            {
                currentSpikeBall = spikeBalls[index];
                currentSpikeBall.transform.localPosition = new Vector3(.097f * DirectionSign, 1.364f);
                currentSpikeBall.gameObject.SetActive(true);
                currentSpikeBall.DisablePhysics();
                currentSpikeBall.rb.rotation = -314;
            }
        }
    }
    private static float ParabolaFrom0to1(float stateProgress)
    {
        float jumpPoint = 2 * stateProgress - 1;
        jumpPoint *= jumpPoint;
        jumpPoint = 1 - jumpPoint;
        if (jumpPoint < 0)
        {
            return 0;
        }
        //don't check for 1 because the result of this math will never be > 1
        return jumpPoint;
    }
    static float ParabolaFrom0To1Derivative(float stateProgress)
    {
        return -8 * stateProgress + 4;
    }
    /// <param name="velDirection">MUST BE NORMALIZED!!</param>
    private void MidairAnim(Vector2 velDirection)
    {
        float threshold = 0.7071067812f;// ~ 1/sqrt(2) with some leniency
        if (Vector2.Dot(velDirection, Vector2.left) >= threshold)
        {
            animator.CrossFade(FlipX ? AnimIDMidairRight : AnimIDMidairLeft, 0);
        }
        else if (Vector2.Dot(velDirection, Vector2.up) >= threshold)
        {
            animator.CrossFade(AnimIDMidairUp, 0);
        }
        else if (Vector2.Dot(velDirection, Vector2.right) >= threshold)
        {
            animator.CrossFade(FlipX ? AnimIDMidairLeft : AnimIDMidairRight, 0);
        }
        else
        {
            animator.CrossFade(AnimIDMidairDown, 0);
        }
    }
    public void ChangeToIntro()
    {
        GetComponent<Collider2D>().enabled = true;
        state = StateIDIntro;
        actionTimer = stateTimer = -Time.deltaTime;
    }
    public override void OnHit(int damageTaken)
    {
        UIManager.UpdateBossLifeBar(LifePercent);
    }
    public override bool PreKill()
    {
        for (int i = 0; i < spikeBalls.Length; i++)
        {
            spikeBalls[i].gameObject.SetActive(false);
        }
        DeathParticle.Spawn(transform.position, FlipnoteColors.Yellow, audioSource);
        rb.isKinematic = true;
        GetComponent<BoxCollider2D>().enabled = false;
        StartCoroutine(DoBigExplosionReturnToMainMenuAfter3SecAndUnlockUpgrade());
        return false;
    }
    IEnumerator DoBigExplosionReturnToMainMenuAfter3SecAndUnlockUpgrade()
    {
        ScreenShakeManager.AddTinyShake();
        yield return new WaitForSecondsRealtime(DeathParticle.SpinEffectDuration);
        ScreenShakeManager.AddLargeShake();
        sprite.enabled = false;
        EffectsHandler.SpawnMediumExplosion(FlipnoteColors.ColorID.Yellow, transform.position);
        yield return new WaitForSecondsRealtime(3f - DeathParticle.SpinEffectDuration);
        PlayerWeaponManager.UnlockSpike();
        LevelInfo.PrepareStageChange();
        SceneManager.LoadScene(SceneIndices.MainMenu);
    }
    Vector2 LimitDirection(Vector2 throwPos, Vector2 target)
    {
        Vector2 arenaBottomLeft = arenaCenter;
        arenaBottomLeft.x -= ArenaWidth / 2;
        arenaBottomLeft.y -= ArenaHeight / 2;
        Vector2 arenaBottomRight = arenaBottomLeft;
        arenaBottomRight.x += ArenaWidth;
        Vector2 toBottomRight = (arenaBottomRight - throwPos).normalized;
        Vector2 toBottomLeft = (arenaBottomLeft - throwPos).normalized;
        Vector2 throwDir = (target - throwPos).normalized;
        Vector2 halfwaybackwards = -(toBottomRight + toBottomLeft).normalized;

        float dotBottomRightThrowDir = Vector2.Dot(throwDir, toBottomRight);
        float dotBottomLeftThrowDir = Vector2.Dot(throwDir, toBottomLeft);
        float dotBottomRightBottomLeft = Vector2.Dot(toBottomRight, toBottomLeft);
        float dotThrowDirHalfwayBackwards = Vector2.Dot(throwDir, halfwaybackwards);

        if (dotBottomLeftThrowDir <= dotBottomRightBottomLeft || dotBottomRightThrowDir <= dotBottomRightBottomLeft || dotThrowDirHalfwayBackwards > 0)
        {
            if (dotBottomRightThrowDir > dotBottomLeftThrowDir)
            {
                throwDir = toBottomRight;
            }
            else
            {
                throwDir = toBottomLeft;
            }
        }

        return throwDir;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
    }
#endif
}
