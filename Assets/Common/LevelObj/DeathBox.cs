using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class DeathBox : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] new BoxCollider2D collider;
#endif
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.immuneTime = 0;
            GameManager.PlayerLife.Damage(99999999);
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Bounds bounds = collider.bounds;
        Gizmos2.DrawRectangle(bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y, Color.red);
    }
#endif

}
