using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Helpers;
using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// script should be AFTER the music handler.
/// </summary>
public class DiscoBossAI : Enemy
{
    enum StateID
    {
        Debug_DontForce = -2,
        None = -1,
        Start = 0,
        Intro = 1,
        SpotlightAttackMiddle = 2,
        ConfettiAlternating = 3,
        ThrowBallMiddleAndOppositeCorner = 4,
        ThrowBallAtPlayer = 5,
        SpotlightAttackCorners = 6,
        ConfettiAllAtOnce = 7,
        ThrowBallThenDoubleConfetti = 8,
        ThrowBallMiddleLeftAndMiddleRight = 9,
        ConfettiQuick = 10,
        ConfettiConverging = 11,
        ThrowBallAlternateBetween_MiddleLeftAndMiddleRight_And_MiddleAndOppositeCorner = 12,
        SpotlightMiddleAndLeft = 13,
        SpotlightMiddleAndRight = 14,
        ThrowBallThenSingleConfetti = 15
    }
    [Serializable]
    class StateData
    {
        public int beatToStartOn;
        public StateID stateID;
        public int startup;
        public int actionCount;
        public int actionRate;
        public int delay;
        public StateData(int beatToStartOn, StateID stateID, int startup, int actionCount, int actionRate, int delay)
        {
            this.beatToStartOn = beatToStartOn;
            this.stateID = stateID;
            this.startup = startup;
            this.actionCount = actionCount;
            this.actionRate = actionRate;
            this.delay = delay;
        }
    }
    const float SecondsPerBeat = (float)DiscoBossMusicHandler.SecondsPerBeat;
    readonly int AnimIDThrow0 = Animator.StringToHash("Throw02");
    readonly int AnimIDThrow1 = Animator.StringToHash("Throw1");
    readonly int AnimIDThrow2 = Animator.StringToHash("Throw02");
    readonly int AnimIDThrow3 = Animator.StringToHash("Throw3");
    readonly int AnimIDThrow4 = Animator.StringToHash("Throw4");
    static readonly int AnimIDSlide = Animator.StringToHash("Slide");
    static readonly int AnimIDPostSlide = Animator.StringToHash("PostSlide");
    static readonly int AnimIDPose = Animator.StringToHash("Pose");

    public override int LifeMax => 150;
    const float ArenaWidth = 16;
    const float ArenaHeight = 8;
    const float IntroDuration = 6;
    public const int ConfettiDamage = 4;
    public new Transform transform;
    [SerializeField] Transform arenaCenterTransform;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] int stateBeatCounter;
    [SerializeField] int startup;
    [SerializeField] int actionCount;
    [SerializeField] int actionRate;
    [SerializeField] int delay;
    [SerializeField] StateID state;
    [SerializeField] float floatTimer;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] DiscoBossBallProj[] ballProjPool;
    [SerializeField] DiscoBossConfettiEmitter[] confettiEmitters1;
    [SerializeField] DiscoBossConfettiEmitter[] confettiEmitters2;
    [SerializeField] DiscoBossMusicHandler musicHandler;
    [SerializeField] DiscoBossSpotlight[] middleAttackSpotlights;
    [SerializeField] DiscoBossSpotlight[] cornerAttackSpotlights;
    [SerializeField] DiscoBossBallProj currentHeldBall;
    [SerializeField] ParticleSystem[] teleportParticles;
    [SerializeField] AudioClip teleportSound;
    [SerializeField] float slideTimer;

#if UNITY_EDITOR
    [Header("debug fields")]
    [SerializeField] StateID forceState = StateID.Debug_DontForce;
    [SerializeField] int forcedStateStartup;
    [SerializeField] int forcedStateActionCount;
    [SerializeField] int forcedStateActionRate;
    [SerializeField] int forcedStateDelay;
    [SerializeField] TextMeshProUGUI debugText;
#endif
    public bool beatFrame;
    public int TotalBeats => musicHandler.beatCounter;
    Vector2 arenaCenter;
    bool StateJustStarted => floatTimer >= 0 && floatTimer < 1E-20f;
    float CurrentArenaSideSign => Mathf.Sign(transform.position.x - arenaCenter.x);//to the left: -1, to the right: 1
    float DirectionSign => sprite.flipX ? -1 : 1;

    [SerializeField] StateData[] states;
    public override void Start()
    {
        base.Start();
#if UNITY_EDITOR
        debugText.gameObject.SetActive(true);
#endif
        arenaCenter = arenaCenterTransform.position;
    }
    int GetBeat() => (musicHandler.beatCounter - 1) % 120;
    void Update()
    {
        if (life <= 0)
        {
            return;
        }
        if (beatFrame)
        {
            //sets beat frame to false at the end of update
            stateBeatCounter++;
            if (state == StateID.None)
            {
                int beat = GetBeat();//subtract 1 because increased just before this
                for (int i = 0; i < states.Length; i++)
                {
                    StateData stateData = states[i];
                    if (beat == stateData.beatToStartOn)
                    {
                        TryDiscardCurrentHeldBall();
                        SwitchState(stateData.stateID, stateData.startup, stateData.actionCount, stateData.actionRate, stateData.delay);
                        break;
                    }
                }
            }
        }
        if (GetBeat() >= 63 && GetBeat() <= 79)
        {
            ConfettiBeat67To79();
        }
        switch (state)
        {
            case StateID.Start:
                State_Start();
                break;
            case StateID.Intro:
                State_Intro();
                break;
            case StateID.SpotlightAttackMiddle:
            case StateID.SpotlightAttackCorners:
            case StateID.SpotlightMiddleAndLeft:
            case StateID.SpotlightMiddleAndRight:
                State_SpotlightAttack();
                break;
            case StateID.ConfettiAllAtOnce:
            case StateID.ConfettiAlternating:
            case StateID.ConfettiQuick:
                State_Confetti();
                break;
            case StateID.ThrowBallMiddleAndOppositeCorner:
            case StateID.ThrowBallAtPlayer:
            case StateID.ThrowBallThenDoubleConfetti:
            case StateID.ThrowBallMiddleLeftAndMiddleRight:
            case StateID.ThrowBallAlternateBetween_MiddleLeftAndMiddleRight_And_MiddleAndOppositeCorner:
            case StateID.ThrowBallThenSingleConfetti:
                State_ThrowBall();
                break;
            default://case StateIDNone 
                State_None();
                break;
        }
        //outside switch because executes no matter which of the cases executed
#if UNITY_EDITOR

        if (forceState != StateID.Debug_DontForce && state == StateID.None)
        {
            SwitchState(forceState, forcedStateStartup, forcedStateActionCount, forcedStateActionRate, forcedStateDelay);
        }
#endif
        if (beatFrame)
        {
            int beat = GetBeat();//subtract 1 because increased just before this
            for (int i = 0; i < states.Length; i++)
            {
                StateData stateData = states[i];
                if (beat == stateData.beatToStartOn)
                {
                    TryDiscardCurrentHeldBall();
                    int remainingActions = actionCount - (stateBeatCounter - startup) - 1;
                    if (state != StateID.None && remainingActions == 0)
                    {
                        TeleportToOtherSide();
                    }
                    SwitchState(stateData.stateID, stateData.startup, stateData.actionCount, stateData.actionRate, stateData.delay);
                    break;
                }
            }
        }
        floatTimer += Time.deltaTime;

        beatFrame = false;
    }


    private void State_Start()
    {
        if (arenaCenter == default)
        {
            arenaCenter = arenaCenterTransform.position;
        }
    }
    private void State_Intro()
    {
        FlipSpriteOnBeat();
        if (StateJustStarted)
        {
            UIManager.ActivateBossLifeBar(FlipnoteColors.Magenta);
            musicHandler.StartMusic();
        }
        float progress = floatTimer / (IntroDuration * SecondsPerBeat);
        UIManager.UpdateBossLifeBar(Mathf.Clamp01(progress));
        if(beatFrame && stateBeatCounter == 5)
        {
            for (int i = 0; i < confettiEmitters1.Length; i++)
            {
                //4 beats taken for full cycle
                confettiEmitters1[i].StartAnimation(SecondsPerBeat * .5f, SecondsPerBeat * 2.5f, SecondsPerBeat * .5f, SecondsPerBeat * .5f);
                confettiEmitters2[i].StartAnimation(SecondsPerBeat * .5f, SecondsPerBeat * 2.5f, SecondsPerBeat * .5f, SecondsPerBeat * .5f);
            }
        }
        if (stateBeatCounter > IntroDuration)
        {
            SwitchToNoState();
        }
    }
    private void State_SpotlightAttack()
    {
        FlipSpriteOnBeat();
        if (StateJustStarted)
        {

        }
        if (stateBeatCounter < startup)
        {

        }
        else if (stateBeatCounter < startup + actionCount * actionRate)
        {
            if (ShouldDoAction())
            {
                if (state == StateID.SpotlightAttackMiddle || state == StateID.SpotlightMiddleAndLeft)
                {
                    middleAttackSpotlights[0].StartAnimation(SecondsPerBeat * actionRate);
                }
                if (state == StateID.SpotlightAttackMiddle || state == StateID.SpotlightMiddleAndRight)
                {
                    middleAttackSpotlights[1].StartAnimation(SecondsPerBeat * actionRate);
                }
                if (state == StateID.SpotlightAttackCorners || state == StateID.SpotlightMiddleAndRight)
                {
                    cornerAttackSpotlights[0].StartAnimation(SecondsPerBeat * actionRate);
                }
                if (state == StateID.SpotlightAttackCorners || state == StateID.SpotlightMiddleAndLeft)
                {
                    cornerAttackSpotlights[1].StartAnimation(SecondsPerBeat * actionRate);
                }
            }
        }
        else if (stateBeatCounter < startup + actionCount * actionRate + delay)
        {

        }
        else
        {
            SwitchToNoState();
        }
    }
    private void State_Confetti()
    {
        int offsetBeatCounter = stateBeatCounter - state == StateID.ConfettiAllAtOnce ? 3 : 2;
        bool switchToPoseSprite = beatFrame && offsetBeatCounter >= startup && (offsetBeatCounter - startup) % actionRate == 0 && offsetBeatCounter < actionRate * actionCount + startup;
        if (!switchToPoseSprite)
        {
            FlipSpriteOnBeat();
        }
        if (stateBeatCounter < startup)
        {
        }
        else if (stateBeatCounter < startup + actionRate * actionCount)
        {
            if (switchToPoseSprite)
            {
                animator.CrossFade(AnimIDPose, 0);
            }
            if (ShouldDoAction())
            {
                if (state == StateID.ConfettiAllAtOnce)
                {
                    for (int i = 0; i < confettiEmitters1.Length; i++)
                    {
                        //4 beats taken for full cycle
                        confettiEmitters1[i].StartAnimation(SecondsPerBeat * .5f, SecondsPerBeat * 2.5f, SecondsPerBeat * .5f, SecondsPerBeat * .5f);
                        confettiEmitters2[i].StartAnimation(SecondsPerBeat * .5f, SecondsPerBeat * 2.5f, SecondsPerBeat * .5f, SecondsPerBeat * .5f);
                    }
                }
                else if (state != StateID.ConfettiConverging)
                {
                    float timeSpentWaitingForExplosion = state == StateID.ConfettiQuick ? 0 : (1.5f * SecondsPerBeat);
                    DiscoBossConfettiEmitter[] confettiEmitters = (int)Mathf.Repeat((stateBeatCounter - startup) / actionRate, 2) == 0 ? confettiEmitters1 : confettiEmitters2;
                    for (int i = 0; i < confettiEmitters.Length; i++)
                    {
                        //3 beats taken for full cycle
                        confettiEmitters[i].StartAnimation(SecondsPerBeat * .5f, timeSpentWaitingForExplosion, SecondsPerBeat * .5f, SecondsPerBeat * .5f);
                    }
                }
            }

        }
        else if (stateBeatCounter < startup + actionRate * actionCount + delay)
        {
            animator.CrossFade(AnimIDThrow0, 0);
        }
        else
        {
            //put here also in case there is no delay assigned
            animator.CrossFade(AnimIDThrow0, 0);
            SwitchToNoState();
        }
    }
    [SerializeField] float ballArcBeatsTimeTaken;
    [SerializeField] float ballGrav;
    [SerializeField] float ballPeakHeight = 5;
    [SerializeField] float animOffset;
    private void State_ThrowBall()
    {
        sprite.flipX = transform.position.x < arenaCenter.x;
        if (stateBeatCounter < startup)
        {
            State_ThrowBall_SetBallThrowAnimationAndHoldOffsets();
        }
        else if (stateBeatCounter < startup + actionCount * actionRate)
        {
            if (state == StateID.ThrowBallThenDoubleConfetti)
            {
                int remainingActions = actionCount - (stateBeatCounter - startup) - 1;
#if UNITY_EDITOR
                debugText.text = $"remaining actions: {remainingActions}";
#endif
                if (remainingActions == 1 && beatFrame)
                {
                    bool flipOrder = Random2.Bool;
                    DiscoBossConfettiEmitter[] confettiEmitters1 = flipOrder ? this.confettiEmitters2 : this.confettiEmitters1;
                    DiscoBossConfettiEmitter[] confettiEmitters2 = flipOrder ? this.confettiEmitters1 : this.confettiEmitters2;
                    for (int i = 0; i < confettiEmitters1.Length; i++)
                    {
                        //3 beats taken for full cycle
                        confettiEmitters1[i].StartAnimation(SecondsPerBeat * .5f, SecondsPerBeat * 1.5f, SecondsPerBeat * .5f, SecondsPerBeat * .5f);
                        confettiEmitters2[i].StartAnimation(SecondsPerBeat * .5f, SecondsPerBeat * 1.5f, SecondsPerBeat * .5f, SecondsPerBeat * .5f, SecondsPerBeat / 2f);
                    }
                }
            }
            else if (state == StateID.ThrowBallThenSingleConfetti)
            {
                int remainingActions = actionCount - (stateBeatCounter - startup) - 1;
                if (remainingActions == 1 && beatFrame)
                {
                    for (int i = 0; i < confettiEmitters1.Length; i++)
                    {
                        //3 beats taken for full cycle
                        confettiEmitters1[i].StartAnimation(SecondsPerBeat * .5f, SecondsPerBeat * 1.5f, SecondsPerBeat * .5f, SecondsPerBeat * .5f);
                    }
                }
            }
            State_ThrowBall_SetBallThrowAnimationAndHoldOffsets();
            //debugText.text = "relative timer: " + relativeTimer.ToString() + ", progress: " + actionProgress.ToString() + "\ntime taken: " + timeTakenForThrowAnim.ToString();
            if (ShouldDoAction())
            {
                TryDiscardCurrentHeldBall();
                Vector2[] launchTargetPoints;
                StateID stateToCheck;
                if (state == StateID.ThrowBallAlternateBetween_MiddleLeftAndMiddleRight_And_MiddleAndOppositeCorner)
                {
                    stateToCheck = ((stateBeatCounter - startup) / actionRate) % 2 == 0 ? StateID.ThrowBallMiddleLeftAndMiddleRight : StateID.ThrowBallMiddleAndOppositeCorner;
                }
                else
                {
                    stateToCheck = state;
                }
                if (stateToCheck == StateID.ThrowBallMiddleLeftAndMiddleRight)
                {
                    launchTargetPoints = new Vector2[]
                    {
                        new(arenaCenter.x + ArenaWidth * .25f, arenaCenter.y - ArenaHeight * .5f),
                        new(arenaCenter.x - ArenaWidth * .25f, arenaCenter.y - ArenaHeight * .5f),
                    };
                }
                else if (stateToCheck != StateID.ThrowBallMiddleAndOppositeCorner)
                {
                    launchTargetPoints = new Vector2[] { GameManager.PlayerPosition };
                }
                else
                {
                    launchTargetPoints = new Vector2[]
                    {
                        new(arenaCenter.x, arenaCenter.y - ArenaHeight * .5f),
                        new(arenaCenter.x - CurrentArenaSideSign * (ArenaWidth * .5f - 1), arenaCenter.y - ArenaHeight * .5f),
                    };
                }

                Vector2 from = transform.position;
                from.x -= 0.4f;
                from.y += 1.2f;
                for (int i = 0; i < launchTargetPoints.Length; i++)
                {
                    if (Helper.TryFindFreeIndex(ballProjPool, out int index))
                    {
                        DiscoBossBallProj proj = ballProjPool[index];
                        proj.transform.position = from;
                        proj.EnablePhysics();
                        proj.Start();
                        proj.gameObject.SetActive(true);
                        proj.rb.gravityScale = ballGrav;
                        DiscoBossBallProj.GetLaunchVelocity(from, launchTargetPoints[i], 4.5f, Physics2D.gravity.y * proj.rb.gravityScale, ballPeakHeight, out _, out Vector2 onWayDown);
                        proj.rb.velocity = onWayDown;

                    }
                }

            }
        }
        else if (stateBeatCounter < startup + actionCount * actionRate + delay)
        {
            State_ThrowBall_SetBallThrowAnimationAndHoldOffsets();
        }
        else
        {
            SwitchToNoState();
        }


    }
    void State_None()
    {
        FlipSpriteOnBeat();
    }
    [SerializeField] float timeTakenForThrowAnim;
    private void State_ThrowBall_SetBallThrowAnimationAndHoldOffsets()
    {
        float rateSec = actionRate * SecondsPerBeat;
        float startupSec = startup * SecondsPerBeat;
        float relativeTimer = floatTimer - startupSec + SecondsPerBeat + animOffset;
        float relativeTimerWrapped = relativeTimer % rateSec;
        float offsetAnimationProgress = Helper.Remap(relativeTimerWrapped, rateSec - timeTakenForThrowAnim, rateSec, 0, 1);
        offsetAnimationProgress = Mathf.Repeat(offsetAnimationProgress, 1);

        if (floatTimer < startupSec || stateBeatCounter + 1 >= startup + actionCount * actionRate)
        {
            //sprite.color = Color.Lerp(Color.white, Color.cyan, .5f);
            SetAnimatorAndBallPosition(0);
            TryDiscardCurrentHeldBall();
        }
        else
        {
            //sprite.color = Color.Lerp(Color.white, Color.blue, .5f);

            //animationProgress += animOffset; //doing it here also offsets the starting frame..
            //actionProgress = Easings.SqrIn(actionProgress);

            if (offsetAnimationProgress < .2f)
            {
                TryGetBall();
            }
            SetAnimatorAndBallPosition(offsetAnimationProgress);
        }
    }
    private void ConfettiBeat67To79()
    {
        if (!beatFrame)
        {
            return;
        }
        //left most and right most: emitters2[0], emitters1[3]
        //second left most and second right most: emitters1[1], emitters2[3]
        //second inner most left and second inner most right: emitters2[1], emitters1[2]
        //inner most left and inner most right: emitters1[0], emitters2[2]
        int relativeBeat = GetBeat() - 66;
        switch (relativeBeat)
        {
            case 0:
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[0]);                      //o___
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[3]);                      //o___
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[1], SecondsPerBeat * .5f);//_o__
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[3], SecondsPerBeat * .5f);//_o__
                break;
            case 1:
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[1], SecondsPerBeat * .5f);//__o_
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[2], SecondsPerBeat * .5f);//__o_
                break;
            case 2:
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[0], SecondsPerBeat * .5f);//___o
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[2], SecondsPerBeat * .5f);//___o
                break;
            case 3:
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[1]);                      //__o_                      
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[2]);                      //__o_
                break;
            case 4:
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[1]);                      //_o__
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[3]);                      //_o__
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[0], SecondsPerBeat * .5f);//o___
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[3], SecondsPerBeat * .5f);//o___
                break;
            case 5:
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[1], SecondsPerBeat * .5f);//_o__
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[3], SecondsPerBeat * .5f);//_o__
                break;
            case 6:
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[1], SecondsPerBeat * .5f);//__o_
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[2], SecondsPerBeat * .5f);//__o_
                break;
            case 7:
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[0], SecondsPerBeat * .5f);//___o
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[2], SecondsPerBeat * .5f);//___o
                break;
            case 8:
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[1]);                      //__o_
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[2]);                      //__o_
                TriggerConfettiEmitterDefaultParams(confettiEmitters1[1], SecondsPerBeat * .5f);//_o__
                TriggerConfettiEmitterDefaultParams(confettiEmitters2[3], SecondsPerBeat * .5f);//_o__
                break;
        }
    }
    void TriggerConfettiEmitterDefaultParams(DiscoBossConfettiEmitter emitter, float timeSpentBeforeRising = 0)
    {
        //make method adapt depending on which state it is currently in
        emitter.StartAnimation(SecondsPerBeat * .5f, SecondsPerBeat * 0.5f, SecondsPerBeat * .5f, SecondsPerBeat * .5f, timeSpentBeforeRising);
    }
    private void SetAnimatorAndBallPosition(float actionProgress)
    {
        int animID;
        if (actionProgress < .2f || actionProgress >= 1f)
        {
            animID = AnimIDThrow0;
        }
        else if (actionProgress < .4f)
        {
            animID = AnimIDThrow1;
        }
        else if (actionProgress < .6f)
        {
            animID = AnimIDThrow2;
        }
        else if (actionProgress < .8f)
        {
            animID = AnimIDThrow3;
        }
        else  //won't hold anything at 4
        {
            animID = AnimIDThrow4;
        }
        animator.CrossFade(animID, 0);
        HoldBallAtPosFromAnimID(animID);
    }

    void HoldBallAtPosFromAnimID(int animID)
    {
        if (ReferenceEquals(currentHeldBall, null))
            return;
        float dirSign = DirectionSign;
        if (animID == AnimIDThrow1)
        {
            Vector2 holdPos = new Vector2(1.086f * dirSign, 0.186f) + (Vector2)transform.position;
            currentHeldBall.MoveTo(holdPos);
        }
        else if (animID == AnimIDThrow2 || animID == AnimIDThrow0)
        {
            Vector2 holdPos = new Vector2(0.429f * dirSign, 0.811f) + (Vector2)transform.position;
            currentHeldBall.MoveTo(holdPos);
        }
        else if (animID == AnimIDThrow3)
        {
            Vector2 holdPos = new Vector2(-0.4f * dirSign, 1.2f) + (Vector2)transform.position;
            currentHeldBall.MoveTo(holdPos);
        }
    }
    bool TryDiscardCurrentHeldBall()
    {
        if (ReferenceEquals(currentHeldBall, null))
            return false;
        currentHeldBall.gameObject.SetActive(false);
        currentHeldBall = null;
        return true;
    }
    public bool TryGetBall()
    {
        if (!ReferenceEquals(currentHeldBall, null))
            return true;
        if (Helper.TryFindFreeIndex(ballProjPool, out int index))
        {
            currentHeldBall = ballProjPool[index];
            currentHeldBall.DisablePhysics();
            currentHeldBall.gameObject.SetActive(true);
            return true;
        }
        return false;
    }
    void Teleport(Vector3 destination)
    {
        teleportParticles[0].transform.position = transform.position;
        teleportParticles[0].Play();
        transform.position = destination;
        teleportParticles[1].transform.position = destination;
        teleportParticles[1].Play();
        audioSource.PlayOneShot(teleportSound);
    }
    public void ChangeToIntro()
    {
        state = StateID.Intro;
        state = StateID.Intro;
        floatTimer = -Time.deltaTime;
        stateBeatCounter = 0;
    }
    void FlipSpriteOnBeat()
    {
        if (beatFrame)
        {
            sprite.flipX = !sprite.flipX;//flip sprite every beat
        }
    }
    void SwitchToNoState()
    {
        floatTimer = -Time.deltaTime;
        state = StateID.None;
        stateBeatCounter = -1;
        actionCount = 0;
        actionRate = 0;
        delay = 0;
        startup = 0;
        TeleportToOtherSide();
    }

    private void TeleportToOtherSide()
    {
        Vector3 target = GetArenaSide(CurrentArenaSideSign < 0, 2f);
        target.y += 1;//compensate for hitbox
        Teleport(target);
    }

    void SwitchState(StateID nextStateID, int nextStateStartup, int nextStateActionCount, int nextStateActionRate, int nextStateDelay)
    {
        floatTimer = -Time.deltaTime;
        state = nextStateID;
        stateBeatCounter = -1;
        startup = nextStateStartup;
        actionCount = nextStateActionCount;
        actionRate = nextStateActionRate;
        delay = nextStateDelay;
    }
    //void TrySwitchState(int nextStateID, int currentActionCount, float currentStartup, float currentActionRate, float currentExtraDelay, int nextStateActionCount)
    //{
    //    if (stateBeatCounter > currentActionCount * currentActionRate + currentStartup + currentExtraDelay)
    //    {
    //        SwitchState(nextStateID, nextStateActionCount);
    //    }
    //}
    bool ShouldDoAction()
    {
        return beatFrame && stateBeatCounter >= startup && (stateBeatCounter - startup) % actionRate == 0 && stateBeatCounter < actionRate * actionCount + startup;
    }
    public override void OnHit(int damageTaken)
    {
        UIManager.UpdateBossLifeBar(LifePercent);
    }
    public override bool PreKill()
    {
        for (int i = 0; i < ballProjPool.Length; i++)
        {
            ballProjPool[i].gameObject.SetActive(false);
        }

        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        sprite.enabled = false;
        DeathParticle.Spawn(transform.position, FlipnoteColors.Magenta, audioSource);
        EffectsHandler.SpawnBigExplosion(FlipnoteColors.ColorID.Magenta, transform.position);
        StartCoroutine(ReturnToMainMenuAfter3SecAndUnlockUpgrade());
        return false;
    }
    IEnumerator ReturnToMainMenuAfter3SecAndUnlockUpgrade()
    {
        yield return new WaitForSecondsRealtime(3f);
        PlayerWeaponManager.UnlockDisco();
        GameManager.CleanupCheckpoints();
        SceneManager.LoadScene(SceneIndices.MainMenu);
    }
    Vector3 GetArenaSide(bool falseForLeftTrueForRight, float padding = 1)
    {
        if (falseForLeftTrueForRight)
        {
            //right side
            return new Vector3(arenaCenter.x + ArenaWidth / 2 - padding, arenaCenter.y - ArenaHeight / 2);
        }
        //left side
        return new Vector3(arenaCenter.x - ArenaWidth / 2 + padding, arenaCenter.y - ArenaHeight / 2);


    }
#if UNITY_EDITOR
    Color handlesColor;
    float debug_numberDisplay;
    private void OnDrawGizmos()
    {
        Handles.color = handlesColor;
        Handles.Label(arenaCenter + Vector2.up, debug_numberDisplay.ToString());
        for (int i = 0; i < confettiEmitters1.Length; i++)
        {
            Handles.Label(confettiEmitters1[i].transform.position + new Vector3(0, .25f), "1: " + i);
            Handles.Label(confettiEmitters2[i].transform.position + new Vector3(0, .25f), "2: " + i);
        }
    }
#endif

}
