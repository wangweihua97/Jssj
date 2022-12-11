using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CRayHitSystem))]
    public partial class SMonsterHpSystem: SystemBase ,ICustomSystem
    {
        protected override void OnCreate()
        {
        }

        public void Init()
        {
            
        }

        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            NativeArray<MonsterBeHit> monsterBeHits = CRayHitSystem.MonsterBeHits;
            Entities
                .WithBurst()
                .ForEach((int entityInQueryIndex, ref CMonsterState cMonsterState
                    , ref CMonsterAnim cMonsterAnim, ref CHpPlane cHpPlane, in CMonsterInfo cMonsterInfo ,in CPosition cPosition) =>
                {
                    if (cMonsterState.CurState == 2)
                    {
                        cMonsterState.curDeathTime += deltaTime;
                    }

                    MonsterBeHit monsterBeHit = monsterBeHits[entityInQueryIndex];
                    float len = math.length(monsterBeHit.hitPos - cPosition.position);
                    
                    if (monsterBeHit.damage > 0.01f && len < 2)
                    {
                        cMonsterState.curHP -= monsterBeHit.damage;
                        if (cMonsterState.curHP < 0)
                        {
                            cMonsterState.CurState = 2;
                            cMonsterState.curDeathTime = 0.0f;
                            cMonsterAnim.cur_playTime = 0;
                            cMonsterState.curHP = 0;
                        }

                        cHpPlane.cur_show_time = 0;
                        cHpPlane.percent = cMonsterState.curHP / cMonsterInfo.maxHP;
                    }
                    else
                    {
                        if (cHpPlane.cur_show_time < cHpPlane.max_show_time)
                            cHpPlane.cur_show_time += deltaTime;
                    }
                })
                .ScheduleParallel();
            Dependency.Complete();
        }
    }
}