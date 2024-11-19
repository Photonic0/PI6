using Assets.Helpers;
using UnityEngine;
#if UNITY_EDITOR

public class TilemapSnap : MonoBehaviour
{
    [SerializeField] float tileSize;
    [SerializeField] int debugRenderSize = 1;
    Vector2 Snap(Vector2 position)
    {
        position = IsoToSquare(position);
        position.x = Mathf.Floor(position.x);
        position.y = Mathf.Floor(position.y);
        position.x += 0.5f;
        position.y += 0.5f;
        return SquareToIso(position);
    }
    Vector2 IsoToSquare(Vector2 position)
    {
        position.Set(
            0.25f / tileSize * position.x + 0.5f / tileSize * position.y,
            -0.25f / tileSize * position.x + 0.5f / tileSize * position.y
        );
        return position;
    }
    Vector2 SquareToIso(Vector2 position)
    {
        position.Set(
            2 * tileSize * position.x + -2 * tileSize * position.y,
            tileSize * position.x + tileSize * position.y
        );
        return position;
    }
    private void OnDrawGizmos()
    {
        int width = debugRenderSize / 2;
        for (int i = -width; i < width; i++)
        {
            Vector2 from = new(i, -20);
            Vector2 to = new(i, 20);
            from = SquareToIso(from);
            to = SquareToIso(to);
            Gizmos.DrawLine(from, to);
        }
        for (int i = -width; i < width; i++)
        {
            Vector2 from = new(-20, i);
            Vector2 to = new(20, i);
            from = SquareToIso(from);
            to = SquareToIso(to);
            Gizmos.DrawLine(from, to);
        }
        Vector2 snappedPos = Snap(Helper.MouseWorld);
        Gizmos.DrawSphere(snappedPos, 0.1f);
        Gizmos2.DrawQuad(snappedPos + new Vector2(2, 0), snappedPos + new Vector2(0, 1), snappedPos + new Vector2(0, -1), snappedPos + new Vector2(-2, 0));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.zero, Vector3.up);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, Vector3.right);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.zero, (SquareToIso(Vector3.up)));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, (SquareToIso(Vector3.right)));
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Helper.MouseWorld, 0.1f);
        Gizmos.color = Color.cyan;
          }
}
#endif
