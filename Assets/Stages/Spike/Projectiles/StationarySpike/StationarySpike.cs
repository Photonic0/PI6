using Assets.Common.Consts;
using UnityEngine;

public class StationarySpike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.Player))
        {
            GameManager.PlayerLife.Damage(4);
        }
    }
}
