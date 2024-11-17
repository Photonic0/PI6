using Assets.Common.Consts;
using Assets.Common.Effects.Lightning;
using UnityEngine;

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
    const float LightningOrbsActionRate = 7;
    const float LightningOrbsDelay = 1;

    const float LightningStartup = 1;
    const short LightningActionCount = 3;
    const float LightningActionRate = 2;
    const float LightningDelay = 1;

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

    Vector2 arenaCenter;

    bool StateJustStarted => stateTimer >= 0 && stateTimer < 1E-20f;
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
        animator.CrossFade(AnimIDIdle, 0);
        if (arenaCenter == default)
        {
            arenaCenter = arenaCenterTransform.position;
        }
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
    private void State_LightningOrbs()
    {
        if (StateJustStarted)
        {
            for (int i = 0; i < lightningOrbs.Length; i++)
            {
                TyphoonBossLightningOrb orb = lightningOrbs[i];
                orb.timeLeftUntilColliderActivates = LightningOrbsStartup + 1;
                orb.collider.enabled = false;
                orb.sprite.color = new Color(1, 1, 1, 0);
            }
        }
        if(stateTimer < LightningOrbsStartup)
        {
            //fade in, stay still
        }
        else if (stateTimer < LightningOrbsStartup + LightningOrbsActionCount * LightningOrbsActionRate)
        {
            //acelerate spin
           // float relativeTimer
        }
    }
    private void State_Lightning()
    {

    }
    private void State_Rain()
    {

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
        stateTimer = 0;
        actionTimer = 0;
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
}
