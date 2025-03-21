using Assets.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreDropParent : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!RestoreDrop.spawnedText && Helper.TileCollision(collision))
        {
            RestoreDrop.spawnedText = true;
            FloatingText.SpawnText(transform.position + new Vector3(0, 1.5f), "pick up to restore health\nand ammo of selected weapon\n↓");
        }
    }
}
