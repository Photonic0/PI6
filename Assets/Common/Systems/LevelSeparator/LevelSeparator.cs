using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

//this is for optimizing levels to not needing to keep the entire level loaded at once
//REMEMBER THAT THESE NEED TO COVER CHECKPOINTS!!!
public class LevelSeparator : MonoBehaviour
{
    [SerializeField] GameObject[] objsToDisable;
    [SerializeField] GameObject[] objsToDestroy;
    [SerializeField] GameObject[] objsToEnable;
    private void Start()
    {
        //failsafe
        CheckForLevelSeparatorsInArrayAndRemoveThem(objsToEnable);
        CheckForLevelSeparatorsInArrayAndRemoveThem(objsToDisable);
        CheckForLevelSeparatorsInArrayAndRemoveThem(objsToDestroy);
        //doing this so that things can remain active in the editor
        if (objsToEnable != null)
        {
            for (int i = 0; i < objsToEnable.Length; i++)
            {
                objsToEnable[i].SetActive(false);
            }
        }
    }
    static void CheckForLevelSeparatorsInArrayAndRemoveThem(GameObject[] arrayToCheck)
    {
        if (arrayToCheck != null && arrayToCheck.Length > 0)
        {
            for (int i = 0; i < arrayToCheck.Length; i++)
            {
                if (arrayToCheck[i].TryGetComponent<LevelSeparator>(out _))
                {
                    arrayToCheck[i] = null;
                }
            }
        }
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
                objsToEnable[i].SetActive(true);
            }
            enabled = false;
            Destroy(gameObject);
        }
    }
#if UNITY_EDITOR
    [SerializeField] BoxCollider2D hitboxForPreview;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos2.DrawRectangle(hitboxForPreview.bounds);
    }
    private void OnDrawGizmosSelected()
    {
        Vector2 hitboxMiddle = transform.position;
        Gizmos.color = Color.green;
        for (int i = 0; i < objsToEnable.Length; i++)
        {
            if (objsToEnable[i] != null)
            {
                Gizmos.DrawLine(hitboxMiddle, objsToEnable[i].transform.position);
            }
        }
        Gizmos.color = Color.yellow;
        for (int i = 0; i < objsToDisable.Length; i++)
        {
            if (objsToDisable[i] != null)
            {
                Gizmos.DrawLine(hitboxMiddle, objsToDisable[i].transform.position);
            }
        }
        Gizmos.color = Color.red;
        for (int i = 0; i < objsToDestroy.Length; i++)
        {
            if (objsToDestroy[i] != null)
            {
                Gizmos.DrawLine(hitboxMiddle, objsToDestroy[i].transform.position);
            }
        }
    }
#endif
}
