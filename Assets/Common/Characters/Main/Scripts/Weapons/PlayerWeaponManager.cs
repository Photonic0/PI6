namespace Assets.Common.Characters.Main.Scripts.Weapons
{
    public static class PlayerWeaponManager
    {
        public enum PlayerWeaponID
        {
            Basic = 0,
            Typhoon = 1,
            Spike = 2,
            Disco = 3,
            Explosive = 4
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
            new WeaponTyphoon()
        };
        public static void RechargeAll()
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].charge = PlayerWeapon.MaxCharge;
            }
        }
    }
}
