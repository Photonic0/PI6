using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Common.Consts;

public class DiscoEnemyBallRiderScript : MonoBehaviour, IMusicSyncable
{
    [SerializeField] GameObject BallriderLeft, BallriderRight;
    public bool MovingSide = true; // Se true, move para a direita; se false, para a esquerda
    public float Speed = 2.0f; // Velocidade de movimento padrão
    public float ChasingSpeed = 4.0f; // Velocidade ao perseguir o jogador
    public float RaycastSize = 10000.0f; // Distância do Raycast para detectar o jogador ou obstáculos
    public float GroundCheckDistance = 1.0f; // Distância do Raycast para verificar o chão

    private Transform player; // Referência ao jogador
    private float currentSpeed; // Velocidade atual do inimigo

    private void Start()
    {
        DiscoMusicEventManager.AddSyncableObject(this);
        // Definindo a velocidade inicial do inimigo
        currentSpeed = Speed;
    }

    public int BeatsPerAction => 4;
    public int BeatOffset => 0;


    void Update()
    {
        // Detectando o jogador e obstáculos
        DetectPlayerAndObstacles();

        // Movendo o inimigo com base na direção e na velocidade atual
        MoveEnemy();
    }

    public void DoMusicSyncedAction()
    {
        if (BallriderLeft.activeSelf)
        {
            BallriderLeft.SetActive(false);
            BallriderRight.SetActive(true);
        }
        else
        {
            BallriderRight.SetActive(false);
            BallriderLeft.SetActive(true);
        }
        MovingSide = !MovingSide;
    }

    void DetectPlayerAndObstacles()
    {
        // Define a direção do Raycast com base no movimento
        Vector2 direction = MovingSide ? Vector2.right : Vector2.left;

        // Lança o Raycast na direção do movimento
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, RaycastSize);

        // Verifica se o Raycast atingiu algum objeto
        if (hit.collider != null)
        {
            // Se o Raycast detectar o jogador, aumenta a velocidade para perseguir
            if (hit.collider.CompareTag("ChFr_Main"))
            {
                currentSpeed = ChasingSpeed;
            }
            // Se o Raycast detectar um obstáculo, interrompe o movimento
            else
            {
                currentSpeed = 0;
                print("Atingiu obstáculo");
                return; // Evita verificar o chão se já parou
            }
        }

        // Verifica a presença de chão à frente
        Vector2 groundCheckDirection = new Vector2(direction.x, -1).normalized;
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, groundCheckDirection, GroundCheckDistance);

        if (groundHit.collider == null)
        {
            currentSpeed = 0; // Para o movimento se não houver chão
            print("Sem chão à frente!");
        }
        else
        {
            // Se não houver detecção de obstáculos ou falta de chão, restaura a velocidade padrão
            currentSpeed = Speed;
        }
    }

    void MoveEnemy()
    {
        // Define o movimento com base na direção e velocidade atuais
        float moveDirection = MovingSide ? 1 : -1;
        transform.Translate(Vector2.right * moveDirection * currentSpeed * Time.deltaTime);
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
}