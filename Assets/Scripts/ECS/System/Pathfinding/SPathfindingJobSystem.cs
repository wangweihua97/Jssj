using Game.GlobalSetting;
using Game.Map;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(SPhysicsJob2System))]
    public partial class SPathfindingJobSystem : SystemBase ,ICustomSystem
    {
        public static NativeArray<MapNodeDir> MapDirs;
        EntityQuery m_Group;
        private const float ROTATE_SPEED = 100.0f;
        private UpdateForceJob m_UpdateForceJob;
        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(typeof(CMove),typeof(CRotation), ComponentType.ReadOnly<CPosition>());
        }

        public void Init()
        {
            int xCount = Setting.MapSize.x;
            int yCount = Setting.MapSize.y;
            MapDirs = 
                new NativeArray<MapNodeDir>(xCount * yCount , Allocator.Persistent);
            
            MapDirJob mapCanWalkJob = new MapDirJob();
            mapCanWalkJob.xCount = xCount;
            mapCanWalkJob.MapDirs = MapDirs;
            JobHandle mapCanWalkJobHandle = mapCanWalkJob.Schedule(xCount * yCount, 1);  
            mapCanWalkJobHandle.Complete();
            m_UpdateForceJob = new UpdateForceJob();
        }
        
        public struct MapDirJob: IJobParallelFor
        {
            public int xCount;
            public NativeArray<MapNodeDir> MapDirs;
            public void Execute(int index)
            {
                MapNodeDir mapDir;
                mapDir.dir = float2.zero;
                int y = index / xCount;
                int x = index - y * xCount;
                int2 xy = new int2(x ,y);
                mapDir.canWalk = MapService.FlowFieldMap.map[y * xCount + x].CellType < 10;
                mapDir.dir= MapService.FlowFieldMap.map[xy.y * Setting.MapSize.x + xy.x].Dir;
                mapDir.isTowar = false;
                MapDirs[index] = mapDir;
            }
        }
        
        [BurstCompile]
        struct UpdateForceJob : IJobChunk
        {
            public int xCount;
            public float deltaTime;
            public ComponentTypeHandle<CMove> CMovHandle;
            public ComponentTypeHandle<CRotation> cRotationHandle;
            [ReadOnly] public ComponentTypeHandle<CPosition> CPositionHandle;
            [ReadOnly] public NativeArray<MapNodeDir> MapDirs;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkMoves = chunk.GetNativeArray(CMovHandle);
                var chunkRotations = chunk.GetNativeArray(cRotationHandle);
                var chunkPositions = chunk.GetNativeArray(CPositionHandle);
                for (var i = 0; i < chunk.Count; i++)
                {
                    var move = chunkMoves[i];
                    var position = chunkPositions[i];
                    var rotation = chunkRotations[i];
                    
                    
                    int2 xy = new int2((int)position.position.x ,(int)position.position.y);
                    MapNodeDir mapNodeDir = MapDirs[xy.y * xCount + xy.x];
                    move.last_f = move.f;
                    if (mapNodeDir.canWalk)
                    {
                        float2 dir = mapNodeDir.dir;
                        move.f =  4 * (dir * move.i_m  - move.v);
                        float curRotation = math.atan2(dir.x, dir.y) * Mathf.Rad2Deg;
                        float dif = curRotation - rotation.rotation;
                        float dif_dir = dif > 0 ? 1 : -1;
                        dif = math.min(math.abs(dif), ROTATE_SPEED * deltaTime);
                        chunkRotations[i] = new CRotation(){ rotation = rotation.rotation + dif * dif_dir};
                    }
                    else
                    {
                        move.f = - 2 * move.v;
                    }
                    
                    // Rotate something about its up vector at the speed given by RotationSpeed_IJobChunkStructBased.
                    chunkMoves[i] = move;
                }
            }
        }
        

        protected override void OnUpdate()
        {
            int xCount = Setting.MapSize.x;
            
            var moveType = GetComponentTypeHandle<CMove>();
            var rotationType = GetComponentTypeHandle<CRotation>();
            var positionType = GetComponentTypeHandle<CPosition>(true);

            m_UpdateForceJob.deltaTime = Time.DeltaTime;
            m_UpdateForceJob.xCount = xCount;
            m_UpdateForceJob.MapDirs = MapDirs;
            m_UpdateForceJob.CMovHandle = moveType;
            m_UpdateForceJob.cRotationHandle = rotationType;
            m_UpdateForceJob.CPositionHandle = positionType;

            Dependency = m_UpdateForceJob.ScheduleParallel(m_Group, Dependency);
            Dependency.Complete();
        }
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}