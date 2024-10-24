using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon_SCRIPT : MonoBehaviour
{
    // Referência ao GameObject que será disparado como projétil
    public GameObject projectilePrefab;

    // Referência ao GameObject alvo (Player)
    public GameObject target;

    // Ponto de onde o projétil será disparado
    public GameObject firePoint;

    // Velocidade do projétil
    public float projectileSpeed = 5f;

    // Intervalo entre disparos
    public float fireRate = 1f;

    // Distância máxima permitida para disparar
    public float maxDistanceToFire = 10f;

    // Tempo que deve esperar até poder disparar novamente
    private float nextFireTime = 0f;

    // Variável para armazenar o estado atual de flip do canhão
    private bool isFlipped = false;

    void Update()
    {
        // Verifica se o GameObject alvo e o ponto de disparo existem
        // e se o tempo entre disparos já passou
        if (target != null && firePoint != null && Time.time > nextFireTime)
        {
            // Calcula a distância entre o ponto de disparo (firePoint) e o alvo (target)
            float distanceToTarget = Vector2.Distance(firePoint.transform.position, target.transform.position);

            // Verifica se a distância até o alvo é menor ou igual à distância máxima permitida
            if (distanceToTarget <= maxDistanceToFire)
            {
                // Atualiza o tempo do próximo disparo
                nextFireTime = Time.time + fireRate;

                // Verifica se o alvo está à frente ou atrás para dar o flip
                CheckFlip();

                // Dispara o projétil a partir do ponto de disparo em direção ao alvo
                FireProjectile();
            }
        }
    }

    // Função que verifica se o alvo está atrás do canhão e realiza o flip se necessário
    void CheckFlip()
    {
        // Verifica se o alvo está à esquerda ou à direita do canhão
        bool targetIsBehind = target.transform.position.x < firePoint.transform.position.x;

        // Se o alvo está atrás e o canhão ainda não foi flipado, ou se está à frente e foi flipado
        if (targetIsBehind && isFlipped)
        {
            FlipCannon();
        }
        else if (!targetIsBehind && !isFlipped)
        {
            FlipCannon();
        }
    }

    // Função para dar o flip no eixo Z do canhão
    void FlipCannon()
    {
        // Inverte o eixo Z para "flipar" o canhão
        transform.Rotate(0f, 180f, 0f);
        isFlipped = !isFlipped; // Alterna o estado de flip
    }

    // Função que instanciará o projétil e o lançará em direção ao alvo
    void FireProjectile()
    {
        // Instancia o projétil na posição do firePoint
        GameObject projectile = Instantiate(projectilePrefab, firePoint.transform.position, Quaternion.identity);

        // Calcula a direção em que o projétil deve ser disparado
        Vector2 direction = (target.transform.position - firePoint.transform.position).normalized;

        // Verifica se o projétil possui um Rigidbody2D (para utilizar física)
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Se o projétil tiver um Rigidbody2D, aplica velocidade na direção calculada
            rb.velocity = direction * projectileSpeed;
        }

        // Se o projétil não tiver um Rigidbody2D, faz o movimento manualmente
        else
        {
            // Inicia a Coroutine para mover o projétil manualmente
            StartCoroutine(MoveProjectile(projectile, direction));
        }
    }

    // Coroutine que move o projétil caso ele não utilize física (Rigidbody2D)
    IEnumerator MoveProjectile(GameObject projectile, Vector2 direction)
    {
        // Enquanto o projétil existir, ele se moverá
        while (projectile != null)
        {
            // Move o projétil manualmente usando a direção e velocidade
            projectile.transform.position += (Vector3)(direction * projectileSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
