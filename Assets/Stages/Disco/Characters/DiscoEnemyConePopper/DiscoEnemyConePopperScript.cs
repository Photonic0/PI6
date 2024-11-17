using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Common.Consts;

public class DiscoEnemyConePopperScript : MonoBehaviour, IMusicSyncable
{
    public float JumpHeight = 5f; // Altura do pulo, ajustável no Inspector
    public float JumpCooldown = 2f; // Tempo entre os pulos, ajustável no Inspector
    public bool Chase = false, MovingSide = true; // Define se o inimigo está se movendo para a direita ou esquerda

    private Rigidbody2D rb;
    private float lastJumpTime;

    public float RaycastSize = 1f;
    public float GroundCheckDistance = 3f;

    [SerializeField] private GameObject targetObject; // GameObject que será detectado

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
        if (Time.time > lastJumpTime + JumpCooldown)
        {
            MovingSide = !MovingSide;
            // Aplica impulso vertical para o pulo
            float moveDirection = MovingSide ? 1 : -1;
            rb.velocity = new Vector2(moveDirection, JumpHeight);

            // Define movimento horizontal opcional (para mover no eixo X enquanto pula)
            
            //rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);

            // Atualiza o tempo do último pulo
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, RaycastSize, 256);

        // Verifica se o Raycast atingiu algum objeto
        if (hit.collider != null && hit.collider.gameObject == targetObject)
        {
            print("Chase");
            Chase = true;
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
}