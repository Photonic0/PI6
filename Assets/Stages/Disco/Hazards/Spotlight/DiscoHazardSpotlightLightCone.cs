using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Diagnostics.Tracing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Common.Consts;

public class DiscoHazardSpotlightLightCone : MonoBehaviour
{
   [SerializeField] PolygonCollider2D collider;
   [SerializeField] SpriteRenderer spriterenderer;
   private float repeatInterval = 120f / 144f;
   private bool LightsOn = true;


    void OnTriggerEnter2D(Collider2D collision) 
    {
        print("colidiu");
        GameManager.PlayerLife.Damage(4);
    }

    void Start()
    {
        InvokeRepeating("ExecuteRepeatingFunction", 0f, repeatInterval);
    }

    void ExecuteRepeatingFunction()
    {
        
        if (LightsOn)
        {
            collider.enabled = false;
            spriterenderer.enabled = false;
            LightsOn = !LightsOn;
        }
        
        else
        {
            collider.enabled = true;
            spriterenderer.enabled = true;
            LightsOn = !LightsOn;
        }
    }
}
