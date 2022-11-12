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
    [UpdateBefore(typeof(CMoveSystem))]
    public partial class CPositionJobSystem : SystemBase
    {
        private NativeArray<MapNodeDir> mapDirs;
        EntityQuery m_Group;
        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(typeof(CMove), ComponentType.ReadOnly<CPosition>());
            
        }

        public void Init()
        {
            int xCount = FlowFieldMap.Size.x;
            int yCount = FlowFieldMap.Size.y;
            mapDirs = 
                new NativeArray<MapNodeDir>(xCount * yCount , Allocator.Persistent);
            
            MapDirJob mapCanWalkJob = new MapDirJob();
            mapCanWalkJob.xCount = xCount;
            mapCanWalkJob.mapDirs = mapDirs;
            JobHandle mapCanWalkJobHandle = mapCanWalkJob.Schedule(xCount * yCount, 1);  
            mapCanWalkJobHandle.Complete();
        }
        
        public struct MapDirJob: IJobParallelFor
        {
            public int xCount;
            public NativeArray<MapNodeDir> mapDirs;
            public void Execute(int index)
            {
                MapNodeDir mapDir;
                mapDir.dir = float2.zero;
                int y = index / xCount;
                int x = index - y * xCount;
                int2 xy = new int2(x ,y);
                mapDir.canWalk = MapService.FlowFieldMap.map[y * xCount + x].CellType < 10;
                mapDir.dir= MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].Dir;
                mapDirs[index] = mapDir;
            }
        }
        
        [BurstCompile]
        struct UpdateForceJob : IJobChunk
        {
            public int xCount;
            public ComponentTypeHandle<CMove> CMovHandle;
            [ReadOnly] public ComponentTypeHandle<CPosition> CPositionHandle;
            [ReadOnly] public NativeArray<MapNodeDir> mapDirs;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkMoves = chunk.GetNativeArray(CMovHandle);
                var chunkPositions = chunk.GetNativeArray(CPositionHandle);
                for (var i = 0; i < chunk.Count; i++)
                {
                    var move = chunkMoves[i];
                    var position = chunkPositions[i];
                    
                    
                    int2 xy = new int2((int)position.position.x ,(int)position.position.y);
                    MapNodeDir mapNodeDir = mapDirs[xy.y * xCount + xy.x];
                    
                    if (mapNodeDir.canWalk)
                    {
                        float2 dir = mapNodeDir.dir;
                        move.f =  4 * (dir * move.i_m  - move.v);
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
            int xCount = FlowFieldMap.Size.x;
            
            var moveType = GetComponentTypeHandle<CMove>();
            var positionType = GetComponentTypeHandle<CPosition>(true);

            var job = new UpdateForceJob()
            {
                xCount = xCount,
                mapDirs = mapDirs,
                CMovHandle = moveType,
                CPositionHandle = positionType
            };

            Dependency = job.ScheduleParallel(m_Group, Dependency);
            Dependency.Complete();
        }
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}