using Assets.Common.Consts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Common.Characters.Main.Scripts.Weapons
{
    public class PlayerWeaponManager : MonoBehaviour
    {
        public enum PlayerWeaponID
        {
            Basic = 0,
            Typhoon = 1,
            Spike = 2,
            Disco = 3,
            Explosive = 4
        }

        static PlayerWeaponManager instance;
        GameObject weaponUpgradePanel;
        Image basicWeaponBarFill;
        Image typhoonWeaponBarFill;
        Image discoWeaponBarFill;
        Image spikeWeaponBarFill;
        RectTransform weaponSelectArrow;

        int selectedWeaponIndex;
        const int DefaultWeaponIndex = 0;
        const int TyphoonWeaponIndex = 1;
        const int SpikeWeaponIndex = 2;
        const int DiscoWeaponIndex = 3;
       
        //just use images of the projectiles + a bar
        //invisible buttons
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            selectedWeaponIndex = DefaultWeaponIndex;
        }
        private void Update()
        {
            if (SceneManager.GetActiveScene().buildIndex == SceneIndices.MainMenu)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (weaponUpgradePanel.activeInHierarchy)
                {
                    CloseMenu();
                }
                else
                {
                    OpenMenu();
                }
            }
            if (weaponUpgradePanel.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    selectedWeaponIndex--;
                }
                if (Input.GetKeyDown(KeyCode.D))
                {
                    selectedWeaponIndex++;
                }
                selectedWeaponIndex = (int)Mathf.Repeat(selectedWeaponIndex, 4);
            }
        }
        public static void ChangeWeapon(PlayerWeaponID id)
        {
            PlayerWeapon weapon = GetWeapon(id);
            GameManager.PlayerRenderer.SetPlayerColor(weapon.EquipColor);
            GameManager.PlayerControl.weapon = weapon;
        }
        public static PlayerWeapon GetWeapon(PlayerWeaponID id)
        {
            return weapons[(int)id];
        }
        static readonly PlayerWeapon[] weapons = new PlayerWeapon[]
        {
            new WeaponBasic(),
            new WeaponTyphoon(),
            new WeaponSpike(),
            new WeaponDisco()
        };
        public static void SetPlayerWeapon(PlayerWeaponID id)
        {
            GameManager.PlayerControl.weapon = GetWeapon(id);
        }
        public static void RechargeAll()
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].charge = PlayerWeapon.MaxCharge;
            }
        }
        //add refs to panel and buttons to open and close
        //then add selections andpreview bars for all 3 upgrades
        //add a neutral select with no bar

        public static void OpenMenu()
        {
            GameManager.PauseGame();
            instance.typhoonWeaponBarFill.fillAmount = (float)weapons[1].charge / PlayerWeapon.MaxCharge;
            instance.spikeWeaponBarFill.fillAmount = (float)weapons[2].charge / PlayerWeapon.MaxCharge;
            instance.discoWeaponBarFill.fillAmount = (float)weapons[3].charge / PlayerWeapon.MaxCharge;
            instance.weaponUpgradePanel.SetActive(true);
        }
        public static void CloseMenu()
        {
            GameManager.UnpauseGame();
            instance.weaponUpgradePanel.SetActive(false);
        }
        public static void Populate(GameObject weaponUpgradePanel, Image basicWeaponBarFill, Image typhoonWeaponBarFill, Image discoWeaponBarFill, Image spikeWeaponBarFill, RectTransform weaponSelectArrow)
        {
            instance.weaponUpgradePanel = weaponUpgradePanel;
            instance.basicWeaponBarFill = basicWeaponBarFill;
            instance.typhoonWeaponBarFill = typhoonWeaponBarFill;
            instance.discoWeaponBarFill = discoWeaponBarFill;
            instance.spikeWeaponBarFill = spikeWeaponBarFill;
            instance.weaponSelectArrow = weaponSelectArrow;
        }
    }
}
