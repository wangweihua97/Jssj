using Unity.Burst;

namespace Game.ECS
{
    [BurstCompile]
    public struct MapObstacleInfo
    {
        public int obstacleIndex;
    }
}