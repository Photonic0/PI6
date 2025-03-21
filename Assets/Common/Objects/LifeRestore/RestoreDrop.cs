using Assets.Common.Characters.Main.Scripts;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;

public class RestoreDrop : MonoBehaviour
{
    [SerializeField] GameObject parentObj;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] bool levelSpawned = false;
    [SerializeField] Rigidbody2D rb;
    public static bool spawnedText;//initialized as false in gamemanager
    private void Start()
    {
        rb = parentObj.GetComponent<Rigidbody2D>();
        if (levelSpawned)
        {
            gameObject.GetComponent<RestoreDrop>().sprite.color =
         PlayerWeapon.GetWeaponColorSafely(GameManager.PlayerControl.weapon);
            rb.velocity = new Vector2(0, -0.01f);
        }
      
    }
    public static void SpawnRestore(Vector3 position)
    {
        position.z = 0;
        GameObject obj = Instantiate(CommonPrefabs.RestoreDrop, position, Quaternion.identity);
        obj.GetComponentInChildren<RestoreDrop>().sprite.color =
           PlayerWeapon.GetWeaponColorSafely(GameManager.PlayerControl.weapon);
        obj.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -0.01f);
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
