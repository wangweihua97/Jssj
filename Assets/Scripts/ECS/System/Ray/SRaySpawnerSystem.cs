using System.Collections.Generic;
using Game.GlobalSetting;
using Game.Map;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Game.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [AlwaysUpdateSystem]
    public partial class SRaySpawnerSystem : SystemBase
    {
        BeginInitializationEntityCommandBufferSystem m_rayBeginInitializationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
            m_rayBeginInitializationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var commandBuffer = m_rayBeginInitializationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .ForEach((Entity entity, int entityInQueryIndex, in CAttackRay cAttackRay) =>
                {
                    if(cAttackRay.length < 0.001f)
                        commandBuffer.DestroyEntity(entityInQueryIndex ,entity);
                }).ScheduleParallel();
            m_rayBeginInitializationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
            var createCommandBuffer = m_rayBeginInitializationEntityCommandBufferSystem.CreateCommandBuffer();
            for (int i = 0; i < STowerAttackSystem.TowarAttackResultsCount; i++)
            {
                var instance = createCommandBuffer.Instantiate(BulletEntity.Prefab);
                createCommandBuffer.SetComponent(instance, new CAttackRay()
                {
                    bulletType = STowerAttackSystem.TowarAttackResults[i].bulletType,
                    pos = STowerAttackSystem.TowarAttackResults[i].pos,
                    dir = STowerAttackSystem.TowarAttackResults[i].dir,
                    speed = STowerAttackSystem.TowarAttackResults[i].speed,
                    length = STowerAttackSystem.TowarAttackResults[i].length,
                    damage = STowerAttackSystem.TowarAttackResults[i].damage,
                    monsterIndex = -1
                });
            }
            STowerAttackSystem.TowarAttackResultsCount = 0;
            m_rayBeginInitializationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
        
    }
}