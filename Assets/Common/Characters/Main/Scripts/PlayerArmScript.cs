using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArmScript : MonoBehaviour
{
    private void OnTransformParentChanged()
    {
        if(GameManager.PlayerLife.Dead)
        {
            return;
        }
        GameManager.PlayerControl.UpdateCachedPosition();
    }
}
