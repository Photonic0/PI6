using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class PlatformingSectionDamager : MonoBehaviour
{
    [SerializeField] Transform respawnPoint;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(4);
            if (!GameManager.PlayerLife.Dead)
            {
                GameManager.PlayerControl.transform.position = respawnPoint.position;
                if (CameraRailSystem.instance != null)//don't reset if it has a lerp smoothing camera
                {
                    Helper.ResetCamera(respawnPoint.position);
                }
            }
        }
    }
#if UNITY_EDITOR
    [SerializeField] BoxCollider2D colliderToVisualize;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos2.DrawRectangle(colliderToVisualize.bounds);
        Bounds bounds = new(respawnPoint.position, new Vector3(1, 2));
        Gizmos2.DrawLinesInBounds(bounds);
        Gizmos2.DrawRectangle(bounds);
    }
#endif
}
