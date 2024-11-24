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
        ThrowBallFixedPoint = 4,
        ThrowBallAtPlayer = 5,
        SpotlightAttackCorners = 6,
        ConfettiAllAtOnce = 7
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

    static readonly int AnimIDIdle = Animator.StringToHash("Throw02");

    readonly int AnimIDThrow0 = Animator.StringToHash("Throw02");
    readonly int AnimIDThrow1 = Animator.StringToHash("Throw1");
    readonly int AnimIDThrow2 = Animator.StringToHash("Throw02");
    readonly int AnimIDThrow3 = Animator.StringToHash("Throw3");
    readonly int AnimIDThrow4 = Animator.StringToHash("Throw4");
    //readonly int[] throwFrames = { Animator.StringToHash("Throw02"), Animator.StringToHash("Throw1"), Animator.StringToHash("Throw02"), Animator.StringToHash("Throw3"), Animator.StringToHash("Throw4") };
    static readonly int AnimIDSlide = Animator.StringToHash("Slide");
    static readonly int AnimIDPostSlide = Animator.StringToHash("PostSlide");

    public override int LifeMax => 70;
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
    Vector2 movementTargetPoint;//only some states use the movement target point
    Vector2 arenaCenter;
    bool StateJustStarted => floatTimer >= 0 && floatTimer < 1E-20f;
    bool IsOnLeftSideOfArena => transform.position.x < arenaCenter.x;
    bool IsOnRightSideOfArena => transform.position.x > arenaCenter.x;
    bool IsOnMiddleOfArena => transform.position.x == arenaCenter.x;
    float CurrentArenaSideSign => Mathf.Sign(transform.position.x - arenaCenter.x);
    float DirectionSign => sprite.flipX ? -1 : 1;
    bool FlipX => transform.position.x < GameManager.PlayerPosition.x;
    Vector2 CurrentArenaSidePoint => IsOnLeftSideOfArena ? (arenaCenter - new Vector2(ArenaWidth / 2 - 2, ArenaHeight / 2 - 1)) : (arenaCenter + new Vector2(ArenaWidth / 2 - 2, -ArenaHeight / 2 + 1));
    bool HoldingBall => currentHeldBall != null && currentHeldBall.gameObject.activeInHierarchy;

    [SerializeField] StateData[] states; //=
    public override void Start()
    {
        base.Start();
#if UNITY_EDITOR
        debugText.gameObject.SetActive(true);
#endif
        arenaCenter = arenaCenterTransform.position;
    }
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
            int beat = musicHandler.beatCounter - 1;//subtract 1 because increased just before this
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
        sprite.flipX = FlipX;

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
                State_SpotlightAttack();
                break;
            case StateID.ConfettiAllAtOnce:
            case StateID.ConfettiAlternating:
                State_Confetti();
                break;
            case StateID.ThrowBallFixedPoint:
            case StateID.ThrowBallAtPlayer:
                State_ThrowBall();
                break;
            default://case StateIDNone 

                break;
        }
        //outside switch because executes no matter which of the cases executed
#if UNITY_EDITOR

        if (forceState != StateID.Debug_DontForce && state == StateID.None)
        {
            SwitchState(forceState, forcedStateStartup, forcedStateActionCount, forcedStateActionRate, forcedStateDelay);
        }
#endif

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
        if (StateJustStarted)
        {
            UIManager.ActivateBossLifeBar(FlipnoteColors.Magenta);
            musicHandler.StartMusic();
        }
        float progress = floatTimer / (IntroDuration * SecondsPerBeat);
        UIManager.UpdateBossLifeBar(Mathf.Clamp01(progress));
        if (stateBeatCounter > IntroDuration)
        {
            SwitchToNoState();
        }
    }
    private void State_SpotlightAttack()
    {
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
                if (state == StateID.SpotlightAttackMiddle)
                {
                    middleAttackSpotlights[0].StartAnimation(SecondsPerBeat * actionRate);
                    middleAttackSpotlights[1].StartAnimation(SecondsPerBeat * actionRate);
                }
                else
                {
                    cornerAttackSpotlights[0].StartAnimation(SecondsPerBeat * actionRate);
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
        if (stateBeatCounter < startup)
        {
        }
        else if (stateBeatCounter < startup + actionRate * actionCount)
        {
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
                else
                {
                    DiscoBossConfettiEmitter[] confettiEmitters = (int)Mathf.Repeat((stateBeatCounter - startup) / actionRate, 2) == 0 ? confettiEmitters1 : confettiEmitters2;
                    for (int i = 0; i < confettiEmitters.Length; i++)
                    {
                        //3 beats taken for full cycle
                        confettiEmitters[i].StartAnimation(SecondsPerBeat * .5f, SecondsPerBeat * 1.5f, SecondsPerBeat * .5f, SecondsPerBeat * .5f);
                    }
                }
            }
        }
        else if (stateBeatCounter < startup + actionRate * actionCount + delay)
        {
        }
        else
        {
            SwitchToNoState();
        }
    }
    [SerializeField] float ballArcBeatsTimeTaken;
    [SerializeField] float ballGrav;
    [SerializeField] float ballPeakHeight = 5;
    [SerializeField] float animOffset;
    private void State_ThrowBall()
    {
        if (stateBeatCounter < startup)
        {
            State_ThrowBall_SetBallThrowAnimationAndHoldOffsets();
        }
        else if (stateBeatCounter < startup + actionCount * actionRate)
        {
            State_ThrowBall_SetBallThrowAnimationAndHoldOffsets();
            //debugText.text = "relative timer: " + relativeTimer.ToString() + ", progress: " + actionProgress.ToString() + "\ntime taken: " + timeTakenForThrowAnim.ToString();
            if (ShouldDoAction())
            {
                TryDiscardCurrentHeldBall();
                if (state == StateID.ThrowBallAtPlayer)
                {
                    Vector2 from = transform.position;
                    from.x -= 0.4f;
                    from.y += 1.2f;
                    if (Helper.TryFindFreeIndex(ballProjPool, out int index))
                    {
                        DiscoBossBallProj proj = ballProjPool[index];
                        proj.transform.position = from;
                        proj.EnablePhysics();
                        proj.Start();
                        proj.gameObject.SetActive(true);
                        proj.rb.gravityScale = ballGrav;
                        DiscoBossBallProj.GetLaunchVelocity(from, GameManager.PlayerPosition, 4.5f, Physics2D.gravity.y * proj.rb.gravityScale, ballPeakHeight, out _, out Vector2 onWayDown);
                        proj.rb.velocity = onWayDown;

                    }
                }
                else
                {
                    Vector2[] launchTargetPoints = new Vector2[]
                    {
                    new(arenaCenter.x, arenaCenter.y - ArenaHeight * .5f),
                    new(arenaCenter.x - ArenaWidth * .5f + 1, arenaCenter.y - ArenaHeight * .5f),
                    };
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
    [SerializeField] float timeTakenForThrowAnim;
    private void State_ThrowBall_SetBallThrowAnimationAndHoldOffsets()
    {
        //framing not working correctly wtf?

        float rateSec = actionRate * SecondsPerBeat;
        float startupSec = startup * SecondsPerBeat;
        float relativeTimer = floatTimer - startupSec + SecondsPerBeat + animOffset;
        float relativeTimerWrapped = relativeTimer % rateSec;
        float offsetAnimationProgress = Helper.Remap(relativeTimerWrapped, rateSec - timeTakenForThrowAnim, rateSec, 0, 1);
        offsetAnimationProgress = Mathf.Repeat(offsetAnimationProgress, 1);
#if UNITY_EDITOR
        debugText.text = $"relative timer {relativeTimer}, wrapped:{relativeTimerWrapped}\noffset progress:{offsetAnimationProgress}";
#endif

        if (relativeTimer < 0 || stateBeatCounter + 1 >= startup + actionCount * actionRate)
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
        if (animID == AnimIDThrow1)
        {
            Vector2 holdPos = new Vector2(1.086f, 0.186f) + (Vector2)transform.position;
            currentHeldBall.MoveTo(holdPos);
        }
        else if (animID == AnimIDThrow2 || animID == AnimIDThrow0)
        {
            Vector2 holdPos = new Vector2(0.429f, 0.811f) + (Vector2)transform.position;
            currentHeldBall.MoveTo(holdPos);
        }
        else if (animID == AnimIDThrow3)
        {
            Vector2 holdPos = new Vector2(-0.4f, 1.2f) + (Vector2)transform.position;
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
    }
    public void ChangeToIntro()
    {
        state = StateID.Intro;
        state = StateID.Intro;
        floatTimer = -Time.deltaTime;
        stateBeatCounter = 0;
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
