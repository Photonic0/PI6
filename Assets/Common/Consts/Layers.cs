namespace Assets.Common.Consts
{
    public static class Layers
    {
        public const int Default = 1;
        public const int TransparentFX = 2;
        public const int IgnoreRaycast = 4;
        public const int Tiles = 8;
        public const int Water = 16;
        public const int UI = 32;
        public const int CameraTriggers = 64;
        public const int Enemy = 128;
        public const int Player = 256;
        public const int PlayerOnly = 512;
        public const int FriendlyProj = 1024;
        public const int HostileProj = 2048;
        public const int TilesOnly = 4096;

        public const int Nothing = 0;
        public const int Everything = int.MaxValue;
    }
}
