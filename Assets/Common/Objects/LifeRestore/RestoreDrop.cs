using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using UnityEngine;

public class RestoreDrop : MonoBehaviour
{
    [SerializeField] GameObject parentObj;
    //put this on enemy class
    [SerializeField] SpriteRenderer sprite;
    public static void SpawnRestore(Vector3 position)
    { 
        GameObject obj = Instantiate(CommonPrefabs.RestoreDrop, position, Quaternion.identity);
        obj.GetComponentInChildren<RestoreDrop>().sprite.color =
           PlayerWeapon.GetWeaponColorSafely(GameManager.PlayerControl.weapon);
        Debug.Log("spawned restore at " + position.ToString());
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.Player))
        {
            //todo: add sound effect later
            Destroy(parentObj);
            GameManager.PlayerLife.Heal(8);
            GameManager.PlayerControl.weapon.RestoreCharge((int)(PlayerWeapon.MaxCharge / 7f));
        }
    }
}
