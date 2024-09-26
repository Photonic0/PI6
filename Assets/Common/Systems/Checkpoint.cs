using Assets.Common.Characters.Main.Scripts.Weapons;
using Assets.Common.Consts;
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
            player.transform.position = respawnPoint.position;
            GameManager.PlayerLife.HealMax();
            PlayerWeaponManager.RechargeAll();
            CameraRailSystem.QueueCameraPositionSnap();
        }
    }
}
