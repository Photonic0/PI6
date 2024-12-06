using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] SpriteRenderer bodySprite;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator bodyAnimator;
    [SerializeField] Animator armNormalAnimator;
    [SerializeField] Animator armCannonAnimator;
    [SerializeField] SpriteRenderer armNormalSprite;
    [SerializeField] SpriteRenderer armCannonSprite;
    [SerializeField] PlayerControl playerControl;
    [SerializeField] AudioSource footstepAudioSource;
    float footstepTimer;
    const float FootstepSoundTimerThreshold = .2f;
    FlipnoteColors.ColorID currentColorID;
    public Color Color { get => FlipnoteColors.GetColor(currentColorID); }
    public int SpriteDirection => bodySprite.flipX ? 1 : -1;
    static readonly int idle = Animator.StringToHash("Idle");
    static readonly int hurt = Animator.StringToHash("Hurt");
    static readonly int walk = Animator.StringToHash("Walk");
    static readonly int rise = Animator.StringToHash("Rise");
    static readonly int fall = Animator.StringToHash("Fall");
    static readonly int idleArmCannon = Animator.StringToHash("IdleArmCannon");
    static readonly int idleArmNormal = Animator.StringToHash("IdleArmNormal");
    static readonly int fallArmCannon = Animator.StringToHash("FallArmCannon");
    static readonly int fallArmNormal = Animator.StringToHash("FallArmNormal");
    static readonly int riseArmCannon = Animator.StringToHash("RiseArmCannon");
    static readonly int riseArmNormal = Animator.StringToHash("RiseArmNormal");
    static readonly int walkArmCannon = Animator.StringToHash("WalkArmCannon");
    static readonly int walkArmNormal = Animator.StringToHash("WalkArmNormal");
    bool oldCannonState;
    private void Awake()//can do in awake because gamemanager instance will already be loaded from previous scene
    {
        GameManager.instance.playerRenderer = this;
    }
    void Update()
    {
        bool armCannon = playerControl.shootCooldown + .1f > 0;
        //still render the player while the spin effect of the particles is happening
        if (GameManager.PlayerLife.Dead)
        {
            if (GameManager.PlayerLife.deathRestartTimer > PlayerLife.DeathRestartDuration - DeathParticle.SpinEffectDuration)
            {
                armCannonSprite.enabled = armCannon;
                armNormalSprite.enabled = !armCannon;
                bodySprite.enabled = true;
                return;
            }
            armCannonSprite.enabled = false;
            bodySprite.enabled = false;
            armNormalSprite.enabled = false;
            return;
        }
        armCannonSprite.enabled = armCannon;
        armNormalSprite.enabled = !armCannon;
        bodySprite.enabled = true;
        if (GameManager.PlayerLife.immuneTime > PlayerLife.ImmuneTimeMax - PlayerControl.KBTime)
        {
            armCannonSprite.enabled = false;
            armNormalSprite.enabled = false;
            bodyAnimator.CrossFade(hurt, 0);
            return;
        }
        Vector2 velocity = rb.velocity;
        //get rid of very small decimal values, else animations become weird
        velocity.x = (int)(10000 * velocity.x) / 10000;
        velocity.y = (int)(10000 * velocity.y) / 10000;

        float absVelX = Mathf.Abs(velocity.x);

        bool? flip = null;

        if (absVelX > 0)
        {
            flip = rb.velocity.x > 0;
        }
        if (armCannon)
        {
            flip = Helper.MouseWorld.x > rb.position.x;
        }
        if (flip != null)
        {
            bodySprite.flipX = flip.Value;
            armCannonSprite.flipX = flip.Value;
            armNormalSprite.flipX = flip.Value;
        }
        if (velocity.y > 0f)
        {
            bodyAnimator.CrossFade(rise, 0);
            SetFootstepTimerForPlayingSound();
            armNormalAnimator.CrossFade(riseArmNormal, 0);
            armCannonAnimator.CrossFade(riseArmCannon, 0);
            return;
        }
        if (velocity.y < 0f || GameManager.PlayerControl.jumpTimeLeft <= 0)
        {
            SetFootstepTimerForPlayingSound();
            bodyAnimator.CrossFade(fall, 0);
            armNormalAnimator.CrossFade(fallArmNormal, 0);
            armCannonAnimator.CrossFade(fallArmCannon, 0);
            return;
        }

        if (absVelX > 0)
        {
            bodyAnimator.CrossFade(walk, 0);
            footstepTimer += Time.deltaTime;
            if (footstepTimer > FootstepSoundTimerThreshold)
            {
                footstepTimer %= FootstepSoundTimerThreshold;
                CommonSounds.PlayFootstep(footstepAudioSource);
            }
            armNormalAnimator.CrossFade(walkArmNormal, 0);
            armCannonAnimator.CrossFade(walkArmCannon, 0);
            return;
        }
        SetFootstepTimerForPlayingSound();
        bodyAnimator.CrossFade(idle, 0);
        armNormalAnimator.CrossFade(idleArmNormal, 0);
        armCannonAnimator.CrossFade(idleArmCannon, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Helper.TileCollision(collision) && GameManager.PlayerControl.jumpTimeLeft <= 0)
        {
            footstepTimer = 0;
            CommonSounds.PlayFootstep(footstepAudioSource);
        }
    }
    private void SetFootstepTimerForPlayingSound()
    {
        footstepTimer = FootstepSoundTimerThreshold + 0.001f;
    }
    public void SetPlayerColor(FlipnoteColors.ColorID colorID)
    {
        currentColorID = colorID;
        UIManager.ChangePlayerLifeBarColor(Color);
    }
}
