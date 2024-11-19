using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Helpers;
using Assets.Systems;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscoBossAI : Enemy
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
    const byte StateIDSpotlight = 2;//lightning orb
    const byte StateIDConfetti = 3;//lightning
    const byte StateIDThrowBall = 4;//rain

    const float IntroDuration = 1;

    const float SpotlightStartup = 1;
    const sbyte SpotlightActionCount = 1;
    const float SpotlightActionRate = 4;
    const float SpotlightDelay = 1;


    const float ConfettiStartup = .5f;
    const sbyte ConfettiActionCount = 3;
    const float ConfettiActionRate = .5f;
    const float ConfettiDelay = 1;


    const float ThrowBallStartup = 0.7f;
    const sbyte ThrowBallActionCount = 20;
    const float ThrowBallActionRate = 0.1f;
    const float ThrowBallDelay = 0.5f;

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
    []

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
            case StateIDSpotlight:
                State_Spotlight();
                break;
            case StateIDConfetti:
                State_Confetti();
                break;
            case StateIDThrowBall:
                State_ThrowBall();
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
            UIManager.ActivateBossLifeBar(FlipnoteColors.Magenta);
            DiscoBossMusicHandler.StartMusic();
        }
        float progress = stateTimer / IntroDuration;      
        UIManager.UpdateBossLifeBar(Mathf.Clamp01(progress));
        if (stateTimer > IntroDuration)
        {
            SwitchState(StateIDSpotlight, SpotlightActionCount);
        }
    }

    private void State_Spotlight()
    {
        if (StateJustStarted)
        {

        }
        if (stateTimer < SpotlightStartup)
        {
        }
        else if (stateTimer < SpotlightStartup + SpotlightActionCount * SpotlightActionRate)
        {

        }
        else if (stateTimer < SpotlightStartup + SpotlightActionCount * SpotlightActionRate + SpotlightDelay)
        {
            float relativeTimer = stateTimer - SpotlightStartup - SpotlightActionCount * SpotlightActionRate;

        }
        else
        {
            SwitchState(StateIDConfetti, ConfettiActionCount);
        }
    }

    private void State_Confetti()
    {
        if (stateTimer < SpotlightStartup)
        {

        }
        else if (stateTimer < SpotlightStartup + ConfettiActionRate * ConfettiActionCount)
        {
            float relativeTimer = (stateTimer - ConfettiStartup) % ConfettiActionRate;
            if (ShouldDoAction(ConfettiStartup, ConfettiActionRate))
            {
              
            }
          
        }
        else if (stateTimer < SpotlightStartup + ConfettiActionRate * ConfettiActionCount + ConfettiDelay)
        {
            animator.CrossFade(AnimIDIdle, 0);
            float progress = (stateTimer - SpotlightStartup - (ConfettiActionRate * ConfettiActionCount)) / ConfettiDelay;
        }
        else
        {
            SwitchState(StateIDThrowBall, ThrowBallActionCount);
        }
    }

    private void State_ThrowBall()
    {
        if (StateJustStarted)
        {

        }
        if (stateTimer < ThrowBallStartup)
        {

        }
        else if (stateTimer < ThrowBallStartup + ThrowBallActionCount * ThrowBallActionRate)
        {


            if (ShouldDoAction(ThrowBallStartup, ThrowBallActionRate))
            {
                if (Helper.TryFindFreeIndex(rainProjPool, out int index))
                {
                    TyphoonEnemyCloudProjectile rainProj = rainProjPool[index];

                }
            }
        }
        else if (stateTimer < ThrowBallStartup + ThrowBallActionCount * ThrowBallActionRate + ThrowBallDelay)
        {

        }
        else
        {
            //SwitchState(StateIDLightningOrbs, LightningOrbsActionCount);
            SwitchState(StateIDConfetti, ConfettiActionCount);
        }
    }
    public void ChangeToIntro()
    {
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
