using Assets.Common.Consts;
using Assets.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

//this is for optimizing levels to not needing to keep the entire level loaded at once
//REMEMBER THAT THESE NEED TO COVER CHECKPOINTS!!!

//should have made something that loads when enters and unloads when exits....
public class LevelSeparator : MonoBehaviour
{
    [SerializeField] GameObject[] objsToDisable;
    [SerializeField] GameObject[] objsToDestroy;
    [SerializeField] GameObject[] objsToEnable;
    private void Start()
    {
        //failsafe
        objsToEnable = CheckForLevelSeparatorsInArrayAndRemoveThem(objsToEnable);
        objsToDisable = CheckForLevelSeparatorsInArrayAndRemoveThem(objsToDisable);
        objsToDestroy = CheckForLevelSeparatorsInArrayAndRemoveThem(objsToDestroy);
        //doing this so that things can remain active in the editor
        if (objsToEnable != null && objsToEnable.Length > 0)
        {
            for (int i = 0; i < objsToEnable.Length; i++)
            {
                if (objsToEnable[i] != null)
                {
                    objsToEnable[i].SetActive(false);
                }
            }
        }
    }
    static GameObject[] CheckForLevelSeparatorsInArrayAndRemoveThem(GameObject[] arrayToCheck)
    {
        if (arrayToCheck == null || arrayToCheck.Length == 0)
            return arrayToCheck;
        List<GameObject> result = new(arrayToCheck.Length);
        for (int i = 0; i < arrayToCheck.Length; i++)
        {
            if (arrayToCheck[i] != null && !arrayToCheck[i].TryGetComponent<LevelSeparator>(out _))
            {
                result.Add(arrayToCheck[i]);
            }
        }
        return result.ToArray();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.Player))
        {
            for (int i = 0; i < objsToDisable.Length; i++)
            {
                if (objsToDisable[i] != null)
                {
                    objsToDisable[i].SetActive(false);
                }
            }
            for (int i = 0; i < objsToDestroy.Length; i++)
            {
                if (objsToDestroy[i] != null)
                {
                    Destroy(objsToDestroy[i]);
                }
            }
            for (int i = 0; i < objsToEnable.Length; i++)
            {
                if (objsToEnable[i] != null)
                {
                    objsToEnable[i].SetActive(true);
                }
            }
            enabled = false;
            if (objsToDisable == null || objsToDisable.Length > 0)
            {
                Destroy(gameObject);
            }
        }
    }
#if UNITY_EDITOR
    [SerializeField] BoxCollider2D hitboxForPreview;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos2.DrawRectangle(hitboxForPreview.bounds);
    }
    static bool SafeArray(Array array)
    {
        return array != null && array.Length > 0;
    }
    private void OnDrawGizmosSelected()
    {
        Vector2 hitboxMiddle = transform.position;

        if (SafeArray(objsToEnable))
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < objsToEnable.Length; i++)
            {
                if (objsToEnable[i] != null)
                {
                    Gizmos.DrawLine(hitboxMiddle, objsToEnable[i].transform.position);
                }
            }
        }
        if (SafeArray(objsToDisable))
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < objsToDisable.Length; i++)
            {
                if (objsToDisable[i] != null)
                {
                    Gizmos.DrawLine(hitboxMiddle, objsToDisable[i].transform.position);
                }
            }
        }
        if (SafeArray(objsToDestroy))
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < objsToDestroy.Length; i++)
            {
                if (objsToDestroy[i] != null)
                {
                    Gizmos.DrawLine(hitboxMiddle, objsToDestroy[i].transform.position);
                }
            }
        }
    }
#endif
}
