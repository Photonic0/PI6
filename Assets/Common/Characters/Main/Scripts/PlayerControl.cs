using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float coyoteTimeLeft;
    public const float MaxCoyoteTime = 4f / 60f;
    public Vector2 acceleration;
    public new Transform transform;
    public static KeyCode jumpKey = KeyCode.W;
    public static KeyCode shootKey = KeyCode.Mouse0;
    public PlayerWeapon weapon;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpSpeed;
    public AudioSource shootAudioSource;
    public Rigidbody2D rb;
    public Collider2D tileCollider;
    public CapsuleCollider2D hurtboxCollider;
    public float shootCooldown;
    public float jumpTimeLeft = 0.15f;
    const float MaxJumpTime = 0.15f;
    public const float KBTime = .3f;
    public const float KBPushbackVelocity = 2;
    //do control shoot mouse direction
    //should face in mouse direction
    public Vector3 Position { get; private set; }
    private void Awake()//can do in awake because gamemanager instance will already be loaded from previous scene
    {
        GameManager.instance.playerControl = this;
        weapon = PlayerWeaponManager.GetWeapon(PlayerWeaponManager.PlayerWeaponID.Basic);
    }
    private void Start()
    {
        transform = base.transform;
        Position = transform.position;
        shootCooldown = float.NegativeInfinity;
    }
    void Update()
    {
        if (GameManager.PlayerLife.immuneTime < PlayerLife.ImmuneTimeMax - KBTime)
        {
            //don't do early return because there's more after the else
            if (!GameManager.PlayerLife.Dead)
            {
                if (!GameManager.Paused)
                {
                    Movement();
                    CheckWeaponUse();
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            if (!GameManager.PlayerLife.Dead)
            {
                Vector2 vel = rb.velocity;
                vel.x = Mathf.Max(KBPushbackVelocity, Mathf.Abs(vel.x)) * Mathf.Sign(vel.x);
                rb.velocity = vel;
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
        Position = transform.position;//so other objects can access the position without having to access transform.position
    }

    private void Movement()
    {

        Vector2 velocity = rb.velocity;
        Vector2 move = new(Input.GetAxisRaw("Horizontal") * moveSpeed, velocity.y);
        if (jumpTimeLeft > 0 && Input.GetKey(KeyCode.W))
        {
            if (move.y < jumpSpeed)
            {
                move.y = jumpSpeed;
            }
            jumpTimeLeft -= Time.deltaTime;
        }
        if (jumpTimeLeft < MaxJumpTime && jumpTimeLeft > 0 && Input.GetKeyUp(KeyCode.W) && rb.velocity.y > 0)
        {
            move.y *= .5f;
            jumpTimeLeft = 0;
        }
        //check jumptimeleft >= maxjumptime so that the jump time left
        //doesn't get cut off early if we already started using it
        if (coyoteTimeLeft <= 0 && jumpTimeLeft >= MaxJumpTime)
        {
            jumpTimeLeft = 0;
        }
        coyoteTimeLeft -= Time.deltaTime;
        rb.velocity = move;
    }

    private void CheckWeaponUse()
    {
        shootCooldown -= Time.deltaTime;
        if (shootCooldown <= 0 && Input.GetKey(shootKey))
            if (shootCooldown <= 0 && Input.GetKey(shootKey))
            {
                weapon.TryUse(ref shootCooldown);
            }
    }

    private void FixedUpdate()
    {
        if (GameManager.PlayerLife.Dead)
        {
            rb.velocity = Vector2.zero;
            Position = rb.position;
            return;
        }

        acceleration += 10 * Time.fixedDeltaTime * Physics2D.gravity;
        rb.velocity += acceleration;
        acceleration.x = 0;
        acceleration.y = 0;
    }
    public void UpdateCachedPosition()
    {
        Position = rb.position;
    }
    public void DisableCollision()
    {
        rb.isKinematic = true;
        tileCollider.enabled = false;
        hurtboxCollider.enabled = false;
    }
    public void EnableCollision()
    {
        rb.isKinematic = false;
        tileCollider.enabled = true;
        hurtboxCollider.enabled = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GetOverlapBoxParams(out Vector2 point, out Vector2 size);
        Collider2D collisionDetect = Physics2D.OverlapBox(point, size, 0, Layers.Tiles);
        if (collisionDetect != null && collision.gameObject.CompareTag(Tags.Tiles))
        {
            coyoteTimeLeft = MaxCoyoteTime;
            jumpTimeLeft = MaxJumpTime;
            coyoteTimeLeft = Mathf.Infinity;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        OnCollisionEnter2D(collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Tags.Tiles) && rb.velocity.y <= 0)
        {
            coyoteTimeLeft = MaxCoyoteTime;
        }
    }
    void GetOverlapBoxParams(out Vector2 point, out Vector2 size)
    {
        point = (Vector2)transform.position - new Vector2(0, 1.05f);
        size = new Vector2(1.35f, 0.1f);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(Position, 1);
        GetOverlapBoxParams(out Vector2 point, out Vector2 size);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)acceleration);
        if(jumpTimeLeft <= 0)
        {
            Gizmos.color = Color.red;
        }
        else if(coyoteTimeLeft < MaxCoyoteTime)
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawCube(point, size);
    }
}
