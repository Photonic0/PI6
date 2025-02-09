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
        [SerializeField] new GameObject gameObject;
        public GameObject GameObject => gameObject;
        [SerializeField] bool debug_clickToRechargeAll;
        static PlayerWeaponManager instance;
        RectTransform leftWheelPartTransform;
        RectTransform leftWheelPartLockTransform;
        RectTransform rightWheelPartTransform;
        RectTransform rightWheelPartLockTransform;
        RectTransform upWheelPartLockTransform;
        RectTransform upWheelPartTransform;
        RectTransform bottomWheelPartTransform;
        RectTransform bottomWheelPartLockTransform;
        RectTransform wheelRectTransform;
        GameObject wheelGameObj;
        RectTransform typhoonWeaponIcon;
        RectTransform basicWeaponIcon;
        RectTransform spikeWeaponIcon;
        RectTransform discoWeaponIcon;
        Image typhoonWeaponChargeDisplayWheelFill;
        Image basicWeaponChargeDisplayWheelFill;
        Image spikeWeaponChargeDisplayWheelFill;
        Image discoWeaponChargeDisplayWheelFill;
        int selectedWeaponIndex;
        int hoveredWeaponIndex;
        const float SelectedScale = 1.5f;
        const float UnselectedScale = 1f;
        const int BasicWeaponIndex = 0;
        const int TyphoonWeaponIndex = 1;
        const int SpikeWeaponIndex = 2;
        const int DiscoWeaponIndex = 3;
        float pauseButtonReleaseLockoutTimer = 0;
        public bool UnlockedTyphoonWeapon { get => weaponUnlockFlags[TyphoonWeaponIndex]; set => weaponUnlockFlags[TyphoonWeaponIndex] = value; }
        public bool UnlockedSpikeWeapon { get => weaponUnlockFlags[SpikeWeaponIndex]; set => weaponUnlockFlags[SpikeWeaponIndex] = value; }
        public bool UnlockedDiscoWeapon { get => weaponUnlockFlags[DiscoWeaponIndex]; set => weaponUnlockFlags[DiscoWeaponIndex] = value; }
        [SerializeField] bool[] weaponUnlockFlags;

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
            RechargeAll();
        }
        private void Update()
        {
            UpdateWithDT(Time.deltaTime);
        }
        void UpdateWithDT(float dt)
        {
            if (debug_clickToRechargeAll)
            {
                debug_clickToRechargeAll = false;
                RechargeAll();
            }
            if (SceneManager.GetActiveScene().buildIndex == SceneIndices.MainMenu)
                return;
            bool justPaused = false;
            pauseButtonReleaseLockoutTimer -= dt;
            if (!GameManager.Paused && Input.GetKeyDown(KeyCode.Escape))
            {
                pauseButtonReleaseLockoutTimer = .2f;
                justPaused = true;
                OpenMenu();
            }
            else if (GameManager.Paused && ((pauseButtonReleaseLockoutTimer < 0.1f && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))) || (pauseButtonReleaseLockoutTimer <= 0 && Input.GetKeyUp(KeyCode.Escape))))
            {
                CloseMenu();
            }
            if (GameManager.Paused)
            {
                float scale = wheelRectTransform.localScale.x;
                scale = Decay(scale, 1f, 30f, dt);
                wheelRectTransform.localScale = new Vector3(scale, scale, scale);
                UpdateWeaponWheel(dt, justPaused);
            }
            else
            {
                float scale = wheelRectTransform.localScale.x;
                scale = Decay(scale, 0f, 30f, dt);
                wheelRectTransform.localScale = new Vector3(scale, scale, scale);
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
        public static void OpenMenu()
        {
            //instance.wheelGameObj.SetActive(true);
            CommonSounds.PlayUIConfirm();
            GameManager.PauseGame();
            UIManager.LivesLeftText.text = $"Lives: {PlayerLife.chances}";
            instance.rightWheelPartLockTransform.gameObject.SetActive(!instance.weaponUnlockFlags[BasicWeaponIndex]);
            instance.bottomWheelPartLockTransform.gameObject.SetActive(!instance.weaponUnlockFlags[TyphoonWeaponIndex]);
            instance.leftWheelPartLockTransform.gameObject.SetActive(!instance.weaponUnlockFlags[SpikeWeaponIndex]);
            instance.upWheelPartLockTransform.gameObject.SetActive(!instance.weaponUnlockFlags[DiscoWeaponIndex]);
            instance.basicWeaponChargeDisplayWheelFill.fillAmount = (float)weapons[BasicWeaponIndex].charge / PlayerWeapon.MaxCharge;
            instance.typhoonWeaponChargeDisplayWheelFill.fillAmount = (float)weapons[TyphoonWeaponIndex].charge / PlayerWeapon.MaxCharge;
            instance.spikeWeaponChargeDisplayWheelFill.fillAmount = (float)weapons[SpikeWeaponIndex].charge / PlayerWeapon.MaxCharge;
            instance.discoWeaponChargeDisplayWheelFill.fillAmount = (float)weapons[DiscoWeaponIndex].charge / PlayerWeapon.MaxCharge;
        }
        public static void CloseMenu()
        {
            instance.ResetScaleOfAllTransforms();
            //instance.wheelGameObj.SetActive(false);
            CommonSounds.PlayUIConfirm();
            GameManager.UnpauseGame();
            GameManager.PlayerControl.weapon = weapons[instance.selectedWeaponIndex];
        }
        public static void Initialize(
            RectTransform leftWheelPartTransform, RectTransform leftWheelPartLockTransform, RectTransform rightWheelPartTransform, RectTransform rightWheelPartLockTransform,
            RectTransform upWheelPartTransform, RectTransform upWheelPartLockTransform, RectTransform bottomWheelPartTransform, RectTransform bottomWheelPartLockTransform,
            RectTransform wheelRectTransform, GameObject wheelGameObj,
            RectTransform typhoonWeaponIcon, RectTransform basicWeaponIcon, RectTransform spikeWeaponIcon, RectTransform discoWeaponIcon,
            Image typhoonWeaponChargeDisplayWheelFill, Image basicWeaponChargeDisplayWheelFill, Image spikeWeaponChargeDisplayWheelFill, Image discoWeaponChargeDisplayWheelFill)
        {
            instance.leftWheelPartTransform = leftWheelPartTransform;
            instance.leftWheelPartLockTransform = leftWheelPartLockTransform;
            instance.rightWheelPartTransform = rightWheelPartTransform;
            instance.rightWheelPartLockTransform = rightWheelPartLockTransform;
            instance.upWheelPartTransform = upWheelPartTransform;
            instance.upWheelPartLockTransform = upWheelPartLockTransform;
            instance.bottomWheelPartTransform = bottomWheelPartTransform;
            instance.bottomWheelPartLockTransform = bottomWheelPartLockTransform;
            instance.wheelRectTransform = wheelRectTransform;
            instance.wheelGameObj = wheelGameObj;
            instance.typhoonWeaponIcon = typhoonWeaponIcon;
            instance.basicWeaponIcon = basicWeaponIcon;
            instance.spikeWeaponIcon = spikeWeaponIcon;
            instance.discoWeaponIcon = discoWeaponIcon;
            instance.typhoonWeaponChargeDisplayWheelFill = typhoonWeaponChargeDisplayWheelFill;
            instance.basicWeaponChargeDisplayWheelFill = basicWeaponChargeDisplayWheelFill;
            instance.spikeWeaponChargeDisplayWheelFill = spikeWeaponChargeDisplayWheelFill;
            instance.discoWeaponChargeDisplayWheelFill = discoWeaponChargeDisplayWheelFill;
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
        public static void ResetUnlocks()
        {
            instance.weaponUnlockFlags = new bool[] { true, false, false, false };
            instance.selectedWeaponIndex = BasicWeaponIndex;
        }
        public void PausedUpdate(float unscaledDeltaTime)
        {
            UpdateWithDT(unscaledDeltaTime);
        }
        void UpdateWeaponWheel(float dt, bool justPaused)
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 wheelCenter = wheelRectTransform.position;
            Vector2 toMousePos = (mousePos - wheelCenter).normalized;
            int previousHoveredIndex = hoveredWeaponIndex;
            if (Within45Deg(toMousePos, Vector2.right))//cloest to right sector
            {
                hoveredWeaponIndex = BasicWeaponIndex;
                selectedWeaponIndex = BasicWeaponIndex;
                DecayScale(rightWheelPartTransform, rightWheelPartLockTransform, basicWeaponIcon, SelectedScale, dt);
                DecayScale(leftWheelPartTransform, leftWheelPartLockTransform, spikeWeaponIcon, UnselectedScale, dt);
                DecayScale(bottomWheelPartTransform, bottomWheelPartLockTransform, typhoonWeaponIcon, UnselectedScale, dt);
                DecayScale(upWheelPartTransform, upWheelPartLockTransform, discoWeaponIcon, UnselectedScale, dt);
            }
            else if (Within45Deg(toMousePos, Vector2.down))//closest to lower sector
            {
                hoveredWeaponIndex = TyphoonWeaponIndex;
                if (UnlockedTyphoonWeapon)
                {
                    selectedWeaponIndex = TyphoonWeaponIndex;
                }
                DecayScale(rightWheelPartTransform, rightWheelPartLockTransform, basicWeaponIcon, UnselectedScale, dt);
                DecayScale(leftWheelPartTransform, leftWheelPartLockTransform, spikeWeaponIcon, UnselectedScale, dt);
                DecayScale(bottomWheelPartTransform, bottomWheelPartLockTransform, typhoonWeaponIcon, SelectedScale, dt);
                DecayScale(upWheelPartTransform, upWheelPartLockTransform, discoWeaponIcon, UnselectedScale, dt);
            }
            else if (Within45Deg(toMousePos, Vector2.left))//closest to left sector
            {
                hoveredWeaponIndex = SpikeWeaponIndex;
                if (UnlockedSpikeWeapon)
                {
                    selectedWeaponIndex = SpikeWeaponIndex;
                }
                DecayScale(rightWheelPartTransform, rightWheelPartLockTransform, basicWeaponIcon, UnselectedScale, dt);
                DecayScale(leftWheelPartTransform, leftWheelPartLockTransform, spikeWeaponIcon, SelectedScale, dt);
                DecayScale(bottomWheelPartTransform, bottomWheelPartLockTransform, typhoonWeaponIcon, UnselectedScale, dt);
                DecayScale(upWheelPartTransform, upWheelPartLockTransform, discoWeaponIcon, UnselectedScale, dt);
            }
            else//closest to up sector
            {
                hoveredWeaponIndex = DiscoWeaponIndex;
                if (UnlockedDiscoWeapon)
                {
                    selectedWeaponIndex = DiscoWeaponIndex;
                }
                DecayScale(rightWheelPartTransform, rightWheelPartLockTransform, basicWeaponIcon, UnselectedScale, dt);
                DecayScale(leftWheelPartTransform, leftWheelPartLockTransform, spikeWeaponIcon, UnselectedScale, dt);
                DecayScale(bottomWheelPartTransform, bottomWheelPartLockTransform, typhoonWeaponIcon, UnselectedScale, dt);
                DecayScale(upWheelPartTransform, upWheelPartLockTransform, discoWeaponIcon, SelectedScale, dt);
            }
            if (hoveredWeaponIndex != previousHoveredIndex && !justPaused)
            {
                CommonSounds.PlayUIChange();
            }
        }
        void ResetScaleOfAllTransforms()
        {
            rightWheelPartLockTransform.localScale = Vector3.one;
            rightWheelPartTransform.localScale = Vector3.one;
            upWheelPartLockTransform.localScale = Vector3.one;
            upWheelPartTransform.localScale = Vector3.one;
            bottomWheelPartLockTransform.localScale = Vector3.one;
            bottomWheelPartTransform.localScale = Vector3.one;
            leftWheelPartLockTransform.localScale = Vector3.one;
            leftWheelPartTransform.localScale = Vector3.one;
            basicWeaponIcon.localScale = Vector3.one;
            spikeWeaponIcon.localScale = Vector3.one;
            typhoonWeaponIcon.localScale = Vector3.one;
            discoWeaponIcon.localScale = Vector3.one;
        }
        static void DecayScale(RectTransform sector, RectTransform @lock, RectTransform icon, float targetScale, float dt)
        {
            float scale = sector.localScale.x;
            scale = Decay(scale, targetScale, 20f, dt);
            Vector3 scaleVec = new(scale, scale, scale);
            sector.localScale = scaleVec;
            scale = 1f / scale;
            scaleVec.Set(scale, scale, scale);
            @lock.localScale = scaleVec;
            icon.localScale = scaleVec;
        }
        bool Within45Deg(Vector2 direction1, Vector2 direction2)
        {
            return Vector2.Dot(direction1, direction2) > 0.707106782f;//1/sqrt(2) with some leniency
        }
        static float Decay(float currentValue, float targetValue, float decay, float dt)
        {
            //thanks freya holmer
            currentValue = targetValue + (currentValue - targetValue) * Mathf.Exp(-decay * dt);
            currentValue = Mathf.MoveTowards(currentValue, targetValue, 0.001f);
            return currentValue;
        }
    }
}
