namespace Assets.Common.Consts
{
    public static class Tags
    {

        //FORMAT
        //1st and 2nd letters 
        //Ch for character
        //Pr for projectile
        //3rd and 4th letters
        //Fr for friendly
        //Hs for hostile
        //add more data later if needed ig??
        /// <summary>
        /// Main from Main Character
        /// </summary>
        public const string Player = "ChFr_Main";
        public const string BaseProj = "PrFr_Default";
        public const string FallingSpike = "PrHs_FallingSpike";
        public const string Tiles = "_Tiles";
        public const string CharacterHostile = "ChHs";

    }
    public static class TagInterpreter
    {
        public static bool IsFriendly(string tag)
        {
            return tag[2] == 'F' && tag[3] == 'r';
        }
        public static bool IsHostile(string tag)
        {
            return tag[2] == 'H' && tag[3] == 's';
        }
        public static bool IsCharacter(string tag)
        {
            return tag[0] == 'C' && tag[1] == 'h';
        }
        public static bool IsProj(string tag)
        {
            return tag[0] == 'P' && tag[1] == 'r';
        }
    }
}
