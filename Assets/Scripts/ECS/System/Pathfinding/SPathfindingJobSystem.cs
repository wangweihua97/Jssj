using System.ComponentModel.Design;
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
        private const float MoveForce = 4.0f;
        private const float IDLE_TIME = 1.0f;
        
        public static NativeArray<MapNodeInfo> MapNodeInfos;
        EntityQuery m_Group;
        private const float ROTATE_SPEED = 120.0f;
        private UpdateForceJob m_UpdateForceJob;
        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(typeof(CMove),typeof(CRotation), typeof(CMonsterState) ,typeof(CMonsterAnim),
            ComponentType.ReadOnly<CPosition>(), ComponentType.ReadOnly<CMonsterInfo>());
        }

        public void Init()
        {
            int xCount = Setting.MapSize.x;
            int yCount = Setting.MapSize.y;
            MapNodeInfos = 
                new NativeArray<MapNodeInfo>(xCount * yCount , Allocator.Persistent);
            
            MapDirJob mapCanWalkJob = new MapDirJob();
            mapCanWalkJob.xCount = xCount;
            mapCanWalkJob.mapNodeInfos = MapNodeInfos;
            JobHandle mapCanWalkJobHandle = mapCanWalkJob.Schedule(xCount * yCount, 1);  
            mapCanWalkJobHandle.Complete();
            m_UpdateForceJob = new UpdateForceJob();
        }
        
        public struct MapDirJob: IJobParallelFor
        {
            public int xCount;
            public NativeArray<MapNodeInfo> mapNodeInfos;
            public void Execute(int index)
            {
                MapNodeInfo mapDir;
                mapDir.dir = float2.zero;
                int y = index / xCount;
                int x = index - y * xCount;
                int2 xy = new int2(x ,y);
                mapDir.canWalk = MapService.FlowFieldMap.map[y * xCount + x].CellType < 10;
                mapDir.dir= MapService.FlowFieldMap.map[xy.y * Setting.MapSize.x + xy.x].Dir;
                mapDir.isTowar = false;
                mapDir.damage = 0.0f;
                mapNodeInfos[index] = mapDir;
            }
        }

        [BurstCompile]
        struct UpdateForceJob : IJobChunk
        {
            public int xCount;
            public int yCount;
            public float deltaTime;
            public ComponentTypeHandle<CMove> CMovHandle;
            public ComponentTypeHandle<CRotation> cRotationHandle;
            public ComponentTypeHandle<CMonsterState> cMonsterStateHandle;
            public ComponentTypeHandle<CMonsterAnim> cMonsterAnimHandle;
            [ReadOnly] public ComponentTypeHandle<CPosition> CPositionHandle;
            [ReadOnly] public ComponentTypeHandle<CMonsterInfo> CMonsterInfoHandle;
            [ReadOnly] public NativeArray<MapNodeInfo> MapDirs;
            
            
            float GetRotate(float2 look ,float curRotation)
            {
                float rotation = math.atan2(look.x, look.y) * Mathf.Rad2Deg;
                float dif = rotation - curRotation;
                float dif_dir = dif > 0 ? 1 : -1;  
                return dif_dir * math.min(math.abs(dif), ROTATE_SPEED * deltaTime);
            }

            bool Check(int2 xy ,float atkRange ,float2 position)
            {
                if (xy.x < 0 || xy.x >= xCount || xy.y < 0 || xy.y >= yCount)
                    return false;
                MapNodeInfo  mapNodeInfo = MapDirs[xy.y * xCount + xy.x];
                if (mapNodeInfo.isTowar)
                {
                    float distance = math.length(xy + new float2(0.5f, 0.5f) - position) - 0.705f;
                    if (distance < atkRange)
                    {
                        return true;
                    }
                }
                return false;
            }
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkMoves = chunk.GetNativeArray(CMovHandle);
                var chunkRotations = chunk.GetNativeArray(cRotationHandle);
                var chunkPositions = chunk.GetNativeArray(CPositionHandle);
                var chunkMonsterStates = chunk.GetNativeArray(cMonsterStateHandle);
                var chunkMonsterInfos = chunk.GetNativeArray(CMonsterInfoHandle);
                var chunkMonsterAnims= chunk.GetNativeArray(cMonsterAnimHandle);
                for (var i = 0; i < chunk.Count; i++)
                {
                    var move = chunkMoves[i];
                    var position = chunkPositions[i];
                    var rotation = chunkRotations[i];
                    var monsterState = chunkMonsterStates[i];
                    var monsterInfo = chunkMonsterInfos[i];
                    var monsterAnim = chunkMonsterAnims[i];
                    
                    int2 xy = new int2((int)position.position.x ,(int)position.position.y);
                    if (monsterState.CurState == 0)
                    {
                        monsterState.curAtkIdleTime += deltaTime;
                        bool hasAttackTarget = false;
                        if (monsterState.curAtkIdleTime > IDLE_TIME)
                        {
                            int2 atkXY = xy;
                            hasAttackTarget = Check(atkXY, monsterInfo.atkRange, position.position);
                            if (!hasAttackTarget)
                            {
                                atkXY = xy + new int2(1, 0);
                                hasAttackTarget = Check(atkXY, monsterInfo.atkRange, position.position);
                            }
                            if (!hasAttackTarget)
                            {
                                atkXY = xy + new int2(-1, 0);
                                hasAttackTarget = Check(atkXY, monsterInfo.atkRange, position.position);
                            }
                            if (!hasAttackTarget)
                            {
                                atkXY = xy + new int2(0, 1);
                                hasAttackTarget = Check(atkXY, monsterInfo.atkRange, position.position);
                            }
                            if (!hasAttackTarget)
                            {
                                atkXY = xy + new int2(0, -1);
                                hasAttackTarget = Check(atkXY, monsterInfo.atkRange, position.position);
                            }
                            if (!hasAttackTarget)
                            {
                                atkXY = xy + new int2(1, 1);
                                hasAttackTarget = Check(atkXY, monsterInfo.atkRange, position.position);
                            }
                            if (!hasAttackTarget)
                            {
                                atkXY = xy + new int2(1, -1);
                                hasAttackTarget = Check(atkXY, monsterInfo.atkRange, position.position);
                            }
                            if (!hasAttackTarget)
                            {
                                atkXY = xy + new int2(-1, 1);
                                hasAttackTarget = Check(atkXY, monsterInfo.atkRange, position.position);
                            }
                            if (!hasAttackTarget)
                            {
                                atkXY = xy + new int2(-1, -1);
                                hasAttackTarget = Check(atkXY, monsterInfo.atkRange, position.position);
                            }

                            if (hasAttackTarget)
                            {
                                monsterState.atkPos = atkXY;
                                monsterState.curAtkTime = 0;
                                monsterState.CurState = 1;
                                monsterAnim.cur_playTime = 0;
                            }
                            else
                            {
                                monsterState.curAtkIdleTime = 0f;
                            }
                        }
                        if(!hasAttackTarget)
                        {
                            MapNodeInfo mapNodeInfo = MapDirs[xy.y * xCount + xy.x];
                            move.last_f = move.f;
                            if (mapNodeInfo.canWalk)
                            {
                                float2 dir = mapNodeInfo.dir;
                                move.f =  MoveForce * (dir * move.i_m  - move.v);
                                chunkRotations[i] = new CRotation(){ rotation = rotation.rotation + GetRotate(dir ,rotation.rotation)};
                            }
                            else
                            {
                                move.f = - 2 * move.v;
                            }
                        }
                    }
                    else if (monsterState.CurState == 1)
                    {
                        monsterState.curAtkTime += deltaTime;
                        float2 dir = monsterState.atkPos + new float2(0.5f, 0.5f) - position.position;
                        chunkRotations[i] = new CRotation(){ rotation = rotation.rotation + GetRotate(dir ,rotation.rotation)};
                        if (monsterState.curAtkTime > monsterInfo.atkInterval)
                        {
                            monsterState.CurState = 0;
                            monsterAnim.cur_playTime = 0;
                            monsterState.isInAtkCD = false;
                        }
                        else if (monsterState.curAtkTime > monsterInfo.maxBeforeAtkTime && !monsterState.isAfterAtk && !monsterState.isInAtkCD)
                        {
                            /*MapBeAttacked mapBeAttacked;
                            mapBeAttacked.damage = monsterInfo.atkDamage;
                            mapBeAttackeds[monsterState.atkPos.y * xCount + monsterState.atkPos.x] =
                                mapBeAttacked;*/
                            monsterState.isAfterAtk = true;
                            monsterState.isInAtkCD = true;
                        }
                    }
                    else
                    {
                        ;
                    }

                    chunkMonsterAnims[i] = monsterAnim;
                    chunkMonsterStates[i] = monsterState;
                    chunkMoves[i] = move;
                }
            }
        }
        

        protected override void OnUpdate()
        {
            int xCount = Setting.MapSize.x;
            int yCount = Setting.MapSize.y;
            
            /*m_ClearJob.mapBeAttackeds = MapBeAttackeds;
            JobHandle clearHandle = m_ClearJob.Schedule(xCount * yCount, 1);  
            clearHandle.Complete();*/

            var moveType = GetComponentTypeHandle<CMove>();
            var rotationType = GetComponentTypeHandle<CRotation>();
            var positionType = GetComponentTypeHandle<CPosition>(true);
            var monsterStateType = GetComponentTypeHandle<CMonsterState>();
            var monsterAnimType = GetComponentTypeHandle<CMonsterAnim>();
            var monsterInfoType = GetComponentTypeHandle<CMonsterInfo>(true);

            m_UpdateForceJob.deltaTime = Time.DeltaTime;
            m_UpdateForceJob.xCount = xCount;
            m_UpdateForceJob.yCount = yCount;
            m_UpdateForceJob.MapDirs = MapNodeInfos;
            m_UpdateForceJob.CMovHandle = moveType;
            m_UpdateForceJob.cRotationHandle = rotationType;
            m_UpdateForceJob.CPositionHandle = positionType;
            m_UpdateForceJob.cMonsterStateHandle = monsterStateType;
            m_UpdateForceJob.CMonsterInfoHandle = monsterInfoType;
            m_UpdateForceJob.cMonsterAnimHandle = monsterAnimType;
            
            Dependency = m_UpdateForceJob.ScheduleParallel(m_Group, Dependency);
            //Dependency = m_UpdateForceJob.Schedule(m_Group, Dependency);
            Dependency.Complete();
        }
        public void OnDestroy(ref SystemState state)
        {
            MapNodeInfos.Dispose();
        }
    }
}