using Unity.Mathematics;

namespace Game.GlobalSetting
{
    public static class Setting
    {
        public readonly static int2 MapSize = new int2(200,200);
        public readonly static int ObstacleNumber = 300;
        public readonly static int RandomObstacleDeep = 10;
        public readonly static int2 ObstacleRange = new int2(30,70);
    }
}