using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Common.Consts;

public class DiscoEnemyConePopperScript : MonoBehaviour, IMusicSyncable
{
    public float JumpHeight = 5f; // Altura do pulo, ajustável no Inspector
    public float JumpCooldown = 2f; // Tempo entre os pulos, ajustável no Inspector
    public bool Attacking, isGrounded, MovingSide = true; // Define se o inimigo está se movendo para a direita ou esquerda

    private Rigidbody2D rb;
    private float lastJumpTime;
    public float JumpArcTop;

    public float RaycastSize = 1f;
    public float GroundCheckDistance = 3f;
    [SerializeField] private GameObject targetObject; // GameObject que será detectado
    public GameObject Idle, PreAttack, Attack, JumpUp, JumpDown, particlePrefab;
    public SpriteRenderer IdleRenderer, PreAttackRenderer, AttackRenderer, JumpUpRenderer, JumpDownRenderer;
    public Transform spawnPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D não encontrado");
        }
        DiscoMusicEventManager.AddSyncableObject(this);
    }
    public int BeatsPerAction => 2;

    public void DoMusicSyncedAction()
    {
        if (Time.time > lastJumpTime + JumpCooldown && !Attacking)
        {
            MovingSide = !MovingSide;
            isGrounded = false;
            JumpArcTop = transform.position.y + 0.9f;
            print(JumpArcTop);

            // Aplica impulso vertical para o pulo
            float moveDirection = MovingSide ? 1 : -1;
            rb.velocity = new Vector2(moveDirection, JumpHeight);
            Idle.SetActive(false);
            JumpUp.SetActive(true);

            lastJumpTime = Time.time;
        }
    } 

    public void OnBecameVisible()
    {
        DoMusicSyncedAction();
    }

    void DetectPlayerAndObstacles()
    {
        // Define a direção do Raycast com base no movimento
        Vector2 direction = MovingSide ? Vector2.right : Vector2.left;

        // Lança o Raycast na direção do movimento
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, RaycastSize, Layers.PlayerHurtbox);

        // Verifica se o Raycast atingiu algum objeto
        if (hit.collider != null && hit.collider.gameObject == targetObject && isGrounded && !Attacking)
        {
            rb.velocity = new Vector2(0 , 0);
            StartCoroutine(AttackFunction());
        }

        // Verifica a presença de chão à frente
        Vector2 groundCheckDirection = new Vector2(direction.x, -2).normalized;

        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, groundCheckDirection, GroundCheckDistance, 8);

        if (groundHit.collider == null)
        {
            //print("Sem chão à frente!");
            rb.velocity = new Vector2(0 , 0);
        }
    }
    private void OnDrawGizmosSelected()
    {
        // Define a cor e direção do Raycast para obstáculos
        Gizmos.color = Color.red;
        Vector2 direction = MovingSide ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(direction * RaycastSize));

        // Define a cor e direção do Raycast para verificar o chão
        Gizmos.color = Color.blue;
        Vector2 groundCheckDirection = new Vector2(direction.x, -1).normalized;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(groundCheckDirection * GroundCheckDistance));
    }

    void Update()
    {
        DetectPlayerAndObstacles();
    }

    void FixedUpdate()
    {
        if (Attacking)
        {
            Idle.SetActive(false);
        }

        if (transform.position.y >= JumpArcTop)
        {
            JumpUp.SetActive(false);
            JumpDown.SetActive(true);
        }

        if(MovingSide)
        {
            IdleRenderer.flipX = true;
            AttackRenderer.flipX = true;
            PreAttackRenderer.flipX = true;
            JumpUpRenderer.flipX = true;
            JumpDownRenderer.flipX = true;
        }
        else
        {
            IdleRenderer.flipX = false;
            AttackRenderer.flipX = false;
            PreAttackRenderer.flipX = false;
            JumpUpRenderer.flipX = false;
            JumpDownRenderer.flipX = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("_Tiles"))
        {
            isGrounded = true;
            Idle.SetActive(true);
            JumpUp.SetActive(false);
            JumpDown.SetActive(false);
        }
    }

    IEnumerator AttackFunction()
    {
        Attacking = true;
        Idle.SetActive(false);
        PreAttack.SetActive(true);

        yield return new WaitForSeconds(1f);

        PreAttack.SetActive(false);
        Attack.SetActive(true);
        GameObject particleInstance = Instantiate(particlePrefab, spawnPoint.position, Quaternion.identity);
        Destroy(particleInstance, 2f);

        yield return new WaitForSeconds(2f);
        Attack.SetActive(false);
        Idle.SetActive(true);
        Attacking = false;
    }
}