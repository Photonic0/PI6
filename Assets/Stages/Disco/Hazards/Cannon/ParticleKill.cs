using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleKill : MonoBehaviour
{
    void Start()
    {
        // Chama a função Destroy após 2 segundos
        Destroy(gameObject, 2f);
    }
}
