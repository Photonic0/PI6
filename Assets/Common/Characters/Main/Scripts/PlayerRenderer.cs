using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] SpriteRenderer bodySprite;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator bodyAnimator;
    [SerializeField] Animator armAnimator;
    [SerializeField] SpriteRenderer armSprite;
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
    private void Awake()//can do in awake because gamemanager instance will already be loaded from previous scene
    {
        GameManager.instance.playerRenderer = this;
    }
    void Update()
    {   
        //still render the player while the spin effect of the particles is happening
        if (GameManager.PlayerLife.Dead)
        {
            if (GameManager.PlayerLife.deathRestartTimer > PlayerLife.DeathRestartDuration - DeathParticle.SpinEffectDuration)
            {
                armSprite.enabled = true;
                bodySprite.enabled = true;
                return;
            }
            armSprite.enabled = false;
            bodySprite.enabled = false;
            return;
        }
        armSprite.enabled = true;
        bodySprite.enabled = true;
        if (GameManager.PlayerLife.immuneTime > PlayerLife.ImmuneTimeMax - PlayerControl.KBTime)
        {
            armSprite.enabled = false;
            bodyAnimator.CrossFade(hurt, 0);
            return;
        }
        Vector2 velocity = rb.velocity;
        //get rid of very small decimal values, else animations become weird
        velocity.x = (int)(1000 * velocity.x) / 1000;
        velocity.y = (int)(1000 * velocity.y) / 1000;

        float absVelX = Mathf.Abs(velocity.x);
        bool armCannon = playerControl.shootCooldown + .1f > 0;

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
            armSprite.flipX = flip.Value;
        }
        if (velocity.y > 0f)
        {
            bodyAnimator.CrossFade(rise, 0);
            SetFootstepTimerForPlayingSound();
            armAnimator.CrossFade(armCannon ? riseArmCannon : riseArmNormal, 0);
            return;
        }
        if (velocity.y < 0f || GameManager.PlayerControl.jumpTimeLeft <= 0)
        {
            SetFootstepTimerForPlayingSound();
            bodyAnimator.CrossFade(fall, 0);
            armAnimator.CrossFade(armCannon ? fallArmCannon : fallArmNormal, 0);
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
            armAnimator.CrossFade(armCannon ? walkArmCannon : walkArmNormal, 0);
            return;
        }
        SetFootstepTimerForPlayingSound();
        bodyAnimator.CrossFade(idle, 0);
        armAnimator.CrossFade(armCannon ? idleArmCannon : idleArmNormal, 0);

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
