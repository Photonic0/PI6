using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Common.Systems
{
    public class Checkpoint : MonoBehaviour
    {
        //index is assigned by multi scene singleton populator
        public int index;
        [SerializeField] Transform respawnPoint;
        [SerializeField] GameObject[] objsToDespawn;


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.CompareTag(Tags.Player))
            {
                GameManager.latestCheckpointIndex = index;
            }
        }
        //make int and label each checkpoint with an index
        //keep int[] data through the scene reload
        //that way you kinda keep the reference, because the checkpoint obj will be replaced by a new one 
        public void RespawnAt()
        {
            for (int i = 0; i < objsToDespawn.Length; i++)
            {
                objsToDespawn[i].SetActive(false);
            }
            PlayerControl player = GameManager.PlayerControl;
            Vector3 spawnPoint = respawnPoint.position;
            player.transform.position = spawnPoint;
            GameManager.PlayerLife.HealMax();
            PlayerWeaponManager.RechargeAll();
            if (CameraRailSystem.instance != null)
            {
                CameraRailSystem.QueueCameraPositionSnap();
            }
            else
            {

                spawnPoint.y += 1f;
                TyphoonCameraSystem.SetCameraPos(spawnPoint);
            }
        }
#if UNITY_EDITOR
        [SerializeField] new BoxCollider2D collider;
        private void OnDrawGizmos()
        {
            if (collider != null)
            {
                Gizmos.color = Color.green;
                Gizmos2.DrawLinesInBoxCollider(collider, 2, true);
            }
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(respawnPoint.position, new Vector3(1.4f, 2, .05f));
        }
#endif
    }
}
