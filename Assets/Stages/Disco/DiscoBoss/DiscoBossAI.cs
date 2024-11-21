using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Helpers;
using System;
using System.Collections;
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

    static readonly int AnimIDIdle = Animator.StringToHash("Idle");
    static readonly int AnimIDHide = Animator.StringToHash("Hide");
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
    [SerializeField] DiscoBossSpotlight[] middleAttackSpotlights    ;
    [SerializeField] DiscoBossSpotlight[] cornerAttackSpotlights;
#if UNITY_EDITOR
    [Header("debug fields")]
    [SerializeField] StateID forceState = StateID.Debug_DontForce;
    [SerializeField] int forcedStateStartup;
    [SerializeField] int forcedStateActionCount;
    [SerializeField] int forcedStateActionRate;
    [SerializeField] int forcedStateDelay;
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

    [SerializeField] StateData[] states; //=
    public override void Start()
    {
        base.Start();
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
                    SwitchState(stateData.stateID, stateData.actionCount, stateData.actionRate, stateData.delay);
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
                State_SpotlightAttackMiddle();
                break;
            case StateID.ConfettiAlternating:
                State_ConfettiAlternating();
                break;
            case StateID.ThrowBallFixedPoint:
                State_ThrowBallFixedPoint();
                break;
            case StateID.ThrowBallAtPlayer:
                State_ThrowBallAtPlayer();
                break;
            case StateID.SpotlightAttackCorners:
                State_SpotlightAttackCorners();
                break;
            case StateID.ConfettiAllAtOnce:
                State_ConfettiAllAtOnce();
                break;
            default://case StateIDNone 

                break;
        }
        //outside switch because executes no matter which of the cases executed
        if (forceState != StateID.Debug_DontForce && state == StateID.None)
        {
            SwitchState(forceState, forcedStateActionCount, forcedStateActionRate, forcedStateDelay);
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
    private void State_SpotlightAttackMiddle()
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
                middleAttackSpotlights[0].StartAnimation(SecondsPerBeat * actionRate);
                middleAttackSpotlights[1].StartAnimation(SecondsPerBeat * actionRate);
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
    private void State_ConfettiAlternating()
    {
        if (stateBeatCounter < startup)
        {
        }
        else if (stateBeatCounter < startup + actionRate * actionCount)
        {
            if (ShouldDoAction())
            {
                DiscoBossConfettiEmitter[] confettiEmitters = (int)Mathf.Repeat((stateBeatCounter - startup) / actionRate, 2) == 0 ? confettiEmitters1 : confettiEmitters2;
                for (int i = 0; i < confettiEmitters.Length; i++)
                {
                    //3 beats taken for full cycle
                    float timeUnit = SecondsPerBeat * .5f;
                    Debug.Log("time unit: " + timeUnit);
                    confettiEmitters[i].StartAnimation(SecondsPerBeat * .5f, SecondsPerBeat * 1.5f, SecondsPerBeat * .5f, SecondsPerBeat * .5f);
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
    [SerializeField] float ballGrav = 10;
    private void State_ThrowBallFixedPoint()
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
                Vector2[] launchTargetPoints = new Vector2[]
                {
                    new(arenaCenter.x, arenaCenter.y - ArenaHeight * .5f),
                    new(arenaCenter.x - ArenaWidth * .5f + 1, arenaCenter.y - ArenaHeight * .5f),
                };
                Vector2 from = transform.position;
                from.y += .5f;
                for (int i = 0; i < launchTargetPoints.Length; i++)
                {
                    if (Helper.TryFindFreeIndex(ballProjPool, out int index))
                    {
                        DiscoBossBallProj proj = ballProjPool[index];
                        proj.gameObject.SetActive(true);
                        proj.rb.gravityScale = ballGrav;

                        DiscoBossBallProj.GetLaunchVelocity(from, launchTargetPoints[i], ballArcBeatsTimeTaken * SecondsPerBeat, Physics2D.gravity.y * proj.rb.gravityScale, ArenaHeight * 2 - 2, out _, out Vector2 onWayDown);
                        proj.rb.velocity = onWayDown;
                        proj.transform.position = from;
                        proj.Start();
                    }
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
    void State_ThrowBallAtPlayer()
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
                Vector2 from = transform.position;
                from.y += .5f;
                if (Helper.TryFindFreeIndex(ballProjPool, out int index))
                {
                    DiscoBossBallProj proj = ballProjPool[index];
                    proj.transform.position = from;
                    proj.gameObject.SetActive(true);
                    proj.rb.gravityScale = ballGrav;
                    DiscoBossBallProj.GetLaunchVelocity(from, GameManager.PlayerPosition, ballArcBeatsTimeTaken * SecondsPerBeat, Physics2D.gravity.y * proj.rb.gravityScale, ArenaHeight * 2 - 2, out _, out Vector2 onWayDown);
                    proj.rb.velocity = onWayDown;
                    proj.Start();
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
    void State_SpotlightAttackCorners()
    {

    }
    void State_ConfettiAllAtOnce()
    {

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
    }
    void SwitchState(StateID nextStateID, int nextStateActionCount, int nextStateActionRate, int nextStateDelay)
    {
        floatTimer = -Time.deltaTime;
        state = nextStateID;
        stateBeatCounter = -1;
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
