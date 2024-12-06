using Assets.Common.Consts;
using Assets.Common.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Common.Characters.Main.Scripts.Weapons
{
    public class PlayerWeaponManager : MonoBehaviour, IUpdatableWhenPaused
    {
        public enum PlayerWeaponID
        {
            Basic = 0,
            Typhoon = 1,
            Spike = 2,
            Disco = 3,
            Explosive = 4
        }

        [SerializeField] bool debug_clickToRechargeAll;
        static PlayerWeaponManager instance;
        GameObject weaponUpgradePanel;
        Image basicWeaponBarFill;
        Image typhoonWeaponBarFill;
        Image spikeWeaponBarFill;
        Image discoWeaponBarFill;
        RectTransform[] weaponBarFillTransforms;
        RectTransform weaponSelectArrow;
        GameObject typhoonWeaponBarBack;
        GameObject spikeWeaponBarBack;
        GameObject discoWeaponBarBack;

        int selectedWeaponIndex;
        const int BasicWeaponIndex = 0;
        const int TyphoonWeaponIndex = 1;
        const int SpikeWeaponIndex = 2;
        const int DiscoWeaponIndex = 3;

        public bool UnlockedTyphoonWeapon { get => weaponUnlockFlags[TyphoonWeaponIndex]; set => weaponUnlockFlags[TyphoonWeaponIndex] = value; }
        public bool UnlockedSpikeWeapon { get => weaponUnlockFlags[SpikeWeaponIndex]; set => weaponUnlockFlags[SpikeWeaponIndex] = value; }
        public bool UnlockedDiscoWeapon {get => weaponUnlockFlags[DiscoWeaponIndex]; set => weaponUnlockFlags[DiscoWeaponIndex] = value; }
        [SerializeField] bool[] weaponUnlockFlags;

        int aKeyPressLockout;
        int dKeyPressLockout;
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
            weaponUnlockFlags = new bool[] { true, false, false, false };
            selectedWeaponIndex = BasicWeaponIndex;
        }
        private void Update()
        {
            if (debug_clickToRechargeAll)
            {
                debug_clickToRechargeAll = false;
                RechargeAll();
            }
            aKeyPressLockout--;
            dKeyPressLockout--;
            if (SceneManager.GetActiveScene().buildIndex == SceneIndices.MainMenu)
                return;
            //operação XOR
            if(GameManager.Paused ^ weaponUpgradePanel.activeInHierarchy)
                return;
    
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameManager.Paused)
                {
                    CloseMenu();
                }
                else
                {
                    OpenMenu();
                }
            }
            if (GameManager.Paused)
            {
                //has lockout windows because for some reason sometimesthe key presses were registering twice
                int arrowKeyChangeDirection = 0;
                if (Input.GetKeyDown(KeyCode.A) && aKeyPressLockout < 0)
                {
                    selectedWeaponIndex--;
                    arrowKeyChangeDirection = -1;
                    aKeyPressLockout = 2;
                }
                if (Input.GetKeyDown(KeyCode.D) && dKeyPressLockout < 0)
                {
                    selectedWeaponIndex++;
                    arrowKeyChangeDirection = 1;
                    dKeyPressLockout = 2;
                }
                selectedWeaponIndex = (int)Mathf.Repeat(selectedWeaponIndex, weaponUnlockFlags.Length);

                //ensure that it will skip non unlocked weapons when trying to change the selected weapons
                while(true)
                {
                    if (weaponUnlockFlags[selectedWeaponIndex])
                    {
                        break;
                    }
                    selectedWeaponIndex += arrowKeyChangeDirection;
                    selectedWeaponIndex = (int)Mathf.Repeat(selectedWeaponIndex, weaponUnlockFlags.Length);
                }

                Vector2 pos = weaponSelectArrow.position;
                pos.x = weaponBarFillTransforms[selectedWeaponIndex].position.x;
                weaponSelectArrow.position = pos;
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
            instance.typhoonWeaponBarBack.SetActive(instance.UnlockedTyphoonWeapon);
            instance.spikeWeaponBarBack.SetActive(instance.UnlockedSpikeWeapon);
            instance.discoWeaponBarBack.SetActive(instance.UnlockedDiscoWeapon);

            instance.typhoonWeaponBarFill.fillAmount = (float)weapons[TyphoonWeaponIndex].charge / PlayerWeapon.MaxCharge;
            instance.spikeWeaponBarFill.fillAmount = (float)weapons[SpikeWeaponIndex].charge / PlayerWeapon.MaxCharge;
            instance.discoWeaponBarFill.fillAmount = (float)weapons[DiscoWeaponIndex].charge / PlayerWeapon.MaxCharge;
            instance.weaponUpgradePanel.SetActive(true);
        }
        public static void CloseMenu()
        {
            GameManager.UnpauseGame();
            GameManager.PlayerControl.weapon = weapons[instance.selectedWeaponIndex];
            instance.weaponUpgradePanel.SetActive(false);
        }
        public static void Initialize(GameObject weaponUpgradePanel, Image basicWeaponBarFill, Image typhoonWeaponBarFill, Image discoWeaponBarFill, Image spikeWeaponBarFill, RectTransform weaponSelectArrow, GameObject typhoonWeaponBarBack, GameObject spikeBarBack, GameObject discoBarBack)
        {
            instance.weaponUpgradePanel = weaponUpgradePanel;
            instance.basicWeaponBarFill = basicWeaponBarFill;
            instance.typhoonWeaponBarFill = typhoonWeaponBarFill;
            instance.spikeWeaponBarFill = spikeWeaponBarFill;
            instance.discoWeaponBarFill = discoWeaponBarFill;
            instance.weaponSelectArrow = weaponSelectArrow;
            instance.typhoonWeaponBarBack = typhoonWeaponBarBack;
            instance.spikeWeaponBarBack = spikeBarBack;
            instance.discoWeaponBarBack = discoBarBack;
            instance.weaponBarFillTransforms = new RectTransform[] { basicWeaponBarFill.rectTransform, typhoonWeaponBarFill.rectTransform, spikeWeaponBarFill.rectTransform, discoWeaponBarFill.rectTransform };
            GameManager.AddToPausedUpatedObjs(instance);
        }
        public static void UnlockDisco()
        {
            instance.weaponUnlockFlags[DiscoWeaponIndex] = true;
        }
        public static void UnlockSpike()
        {
            instance.weaponUnlockFlags[SpikeWeaponIndex] = true;
        }
        public static void UnlockTyphoon()
        {
            instance.weaponUnlockFlags[TyphoonWeaponIndex] = true;
        }
        public void PausedUpdate(float unscaledDeltaTime)
        {
            Update();
        }
    }
}
