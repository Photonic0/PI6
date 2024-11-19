using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
using Assets.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Common.Systems
{
    public class Checkpoint : MonoBehaviour
    {
        //index is assigned by populator
        public int index;
        [SerializeField] Transform respawnPoint;
        [SerializeField] GameObject[] objsToDespawn;
#if UNITY_EDITOR
        [SerializeField] new BoxCollider2D collider;
#endif

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
            if (SceneManager.GetActiveScene().buildIndex == SceneIndices.SpikeStage)
            {
                CameraRailSystem.QueueCameraPositionSnap();
            }
            else
            {
                Camera cam = Camera.main;
                Vector3 pos = cam.transform.position;
                pos.x = spawnPoint.x;
                pos.y = spawnPoint.y;
                cam.transform.position = pos;
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (collider != null)
            {
                Gizmos.color = Color.green;
                Gizmos2.DrawLinesInBoxCollider(collider, 2, true);
            }
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(transform.position, new Vector3(1.4f, 2, .05f));
        }
#endif
    }
}
