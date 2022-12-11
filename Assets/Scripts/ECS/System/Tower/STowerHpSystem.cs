using Game.GlobalSetting;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CRayHitSystem))]
    public partial class STowerHpSystem: SystemBase ,ICustomSystem
    {
        public enum UpdateType
        {
            ClearData,
            UpdateHp,
        }
        public static NativeArray<MapBeAttacked> MapBeAttackeds;
        public static UpdateType CurUpdateType = UpdateType.ClearData;
        private ClearJob m_ClearJob;
        protected override void OnCreate()
        {
        }

        public void Init()
        {
            int xCount = Setting.MapSize.x;
            int yCount = Setting.MapSize.y;
            MapBeAttackeds = new NativeArray<MapBeAttacked>(xCount * yCount , Allocator.Persistent);
            m_ClearJob = new ClearJob();
            
            m_ClearJob.mapBeAttackeds = MapBeAttackeds;
            JobHandle clearHandle = m_ClearJob.Schedule(xCount * yCount, 1);  
            clearHandle.Complete();
        }
        
        [BurstCompile]
        public struct ClearJob: IJobParallelFor
        {
            public NativeArray<MapBeAttacked> mapBeAttackeds;
            public void Execute(int index)
            {
                MapBeAttacked mapDir = mapBeAttackeds[index];
                mapDir.damage = 0.0f;
                mapBeAttackeds[index] = mapDir;
            }
        }

        protected override void OnUpdate()
        {
            int xCount = Setting.MapSize.x;
            int yCount = Setting.MapSize.y;
            float deltaTime = Time.DeltaTime;
            if (CurUpdateType == UpdateType.ClearData)
            {
                /*m_ClearJob.mapBeAttackeds = MapBeAttackeds;
                JobHandle clearHandle = m_ClearJob.Schedule(xCount * yCount, 1);  
                clearHandle.Complete();
                CurUpdateType = UpdateType.UpdateHp;*/
                CurUpdateType = UpdateType.UpdateHp;
            }
            else
            {
                NativeArray<MapBeAttacked> mapBeAttackeds = MapBeAttackeds;
                Entities
                    .WithBurst()
                    .ForEach((ref CMonsterState cMonsterState, in CMonsterInfo cMonsterInfo) =>
                    {
                        if (cMonsterState.isAfterAtk)
                        {
                            MapBeAttacked mapBeAttacked;
                            mapBeAttacked.damage = cMonsterInfo.atkDamage;
                            cMonsterState.isAfterAtk = false;
                            mapBeAttackeds[cMonsterState.atkPos.y * xCount + cMonsterState.atkPos.x] = mapBeAttacked;
                        }
                    }).Schedule();
                Dependency.Complete();
                
                Entities
                    .WithBurst()
                    .ForEach((ref CHpPlane cHpPlane, ref CTowerState cTowerState ,in CTowerInfo cTowerInfo ,in CTowerTransform cTowerTransform) =>
                    {
                        var mapBeAttacked = mapBeAttackeds[cTowerTransform.xy.y * xCount + cTowerTransform.xy.x];
                        if (mapBeAttacked.damage > 0.1)
                        {
                            cTowerState.curHP -= mapBeAttacked.damage;
                            if (cTowerState.curHP < 0)
                            {
                                cTowerState.curState = 2;
                            }
                            cHpPlane.cur_show_time = 0;
                            cHpPlane.percent = cTowerState.curHP / cTowerInfo.maxHP;
                            
                            mapBeAttacked.damage = 0;
                            mapBeAttackeds[cTowerTransform.xy.y * xCount + cTowerTransform.xy.x] = mapBeAttacked;
                        }
                        else
                        {
                            if (cHpPlane.cur_show_time < cHpPlane.max_show_time)
                                cHpPlane.cur_show_time += deltaTime;
                        }
                    }).Schedule();
                CurUpdateType = UpdateType.ClearData;
            }
            
        }
    }
}