using Assets.Common.Consts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TyphoonHazardCloudPlatform : MonoBehaviour
{
    const int StateIDHarmless = 0;
    const int StateIDHarmful = 1;
    const float HarmlessDuration = 2.5f;
    const float HarmfulDuration = 1;
    static readonly int animHarmless = Animator.StringToHash("CloudPlatformHarmless");
    static readonly int animHarmful = Animator.StringToHash("CloudPlatformHarmful");
    [SerializeField] Animator animator;
    float timer = 0;
    int state;

    void Update()
    {
        switch (state)
        {
            case StateIDHarmful:
                State_Harmful();
                break;
            default://StateIDHarmless
                State_Harmless();
                break;
        }
        timer += Time.deltaTime;
    }
    private void State_Harmful()
    {
        if(timer > HarmfulDuration)
        {
            state = StateIDHarmless;
            animator.CrossFade(animHarmless, 0);
            timer = 0;
        }
    }
    private void State_Harmless()
    {
        if(timer > HarmlessDuration)
        {
            timer = 0;
            animator.CrossFade(animHarmful,0);
            state = StateIDHarmful;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForDamage(collision);

    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckForDamage(collision);
    }
    private void CheckForDamage(Collision2D collision)
    {
        if (state == StateIDHarmful && collision.gameObject.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(5);
        }
    }
}
