using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Common.Systems;
using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public Vector2 acceleration;
    public new Transform transform;
    public static KeyCode jumpKey = KeyCode.W;
    public static KeyCode shootKey = KeyCode.Mouse0;
    public PlayerWeapon weapon;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpSpeed;
    public AudioSource shootAudioSource;
    public Rigidbody2D rb;
    public new Collider2D collider;
    public float shootCooldown;
    public float jumpTimeLeft = 0.15f;
    const float MaxJumpTime = 0.15f;
    public const float KBTime = .3f;
    const float KBPushbackVelocity = 2;
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
        if (jumpTimeLeft < MaxJumpTime && jumpTimeLeft > 0 && Input.GetKeyUp(KeyCode.W) &&rb.velocity.y > 0)
        {
            move.y *= .5f;
            jumpTimeLeft = 0;
        }
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
        collider.enabled = false;
    }
    public void EnableCollision()
    {
        rb.isKinematic = false;
        collider.enabled = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GetOverlapBoxParams(out Vector2 point, out Vector2 size);
        Collider2D collisionDetect = Physics2D.OverlapBox(point, size, 0, Layers.Tiles);
        if (collisionDetect != null && collision.gameObject.CompareTag(Tags.Tiles))
        {
            jumpTimeLeft = MaxJumpTime;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        OnCollisionEnter2D(collision);
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
        Gizmos.DrawCube(point, size);
    }
}
