using System.Collections.Generic;
using Game.GlobalSetting;
using Game.Map;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Collider = Game.Map.Collider;
using Random = UnityEngine.Random;

namespace Game.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [AlwaysUpdateSystem]
    public partial class SMonsterSpawnerSystem : SystemBase
    {
        BeginInitializationEntityCommandBufferSystem m_MonsterCommandBufferSystem;

        private List<int2> m_willCreatMonsterPos;
        private static uint MonsterId = 1000;

        protected override void OnCreate()
        {
            // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
            m_MonsterCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            m_willCreatMonsterPos = new List<int2>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = m_MonsterCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            int2 mapSize = Setting.MapSize;
            Entities
                .ForEach((Entity entity, int entityInQueryIndex,in CPosition cp,in CMonsterState cMonsterState ,in CMonsterInfo cMonsterInfo) =>
                {
                    if(cMonsterState.CurState == 2 && cMonsterState.curDeathTime > cMonsterInfo.maxDeathTime)
                        commandBuffer.DestroyEntity(entityInQueryIndex ,entity);
                }).ScheduleParallel();
            m_MonsterCommandBufferSystem.AddJobHandleForProducer(Dependency);
            Dependency.Complete();
        }

        public void Creat_A_Large_Number_Monsters()
        {
            for (int i = 0; i < 10000; i++)
            {
                while (!RandomPosCreatMonster())
                {
                    
                }
            }
        }

        bool RandomPosCreatMonster()
        {
            int2 random_pos = new int2(UnityEngine.Random.Range(0,Setting.MapSize.x),
                Random.Range(0,Setting.MapSize.y));

            if (Mathf.Abs(random_pos.x - Setting.MapSize.x) < 40 || Mathf.Abs(random_pos.y - Setting.MapSize.y) < 40)
                return false;
            if (SPhysicsJob2System.MapBlock[random_pos.y * Setting.MapSize.x + random_pos.x].canWalk
                && !MapService.FlowFieldMap.map[random_pos.y * Setting.MapSize.x + random_pos.x].Dir.Equals(half2.zero))
            {
                CreatMonster(random_pos);
                return true;
            }

            return false;
        }
        

        public void CreatMonster(int2 xy)
        {
            //m_willCreatMonsterPos.Add(xy);
            /*var commandBuffer = m_MonsterCommandBufferSystem.CreateCommandBuffer();
            var instance = commandBuffer.Instantiate(MonsterEntity.Prefab);

            // Place the instantiated in a grid with some noise
            var position = new float3(xy.x ,0,xy.y);
            commandBuffer.SetComponent(instance, new CPosition() { position =  position});
            commandBuffer.SetComponent(instance, new CRotation() { rotation =  float3.zero});
            commandBuffer.SetComponent(instance, new CMove() {  moveSpeed =  2.0f,moveDir = float3.zero});*/
            var EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //var instance = EntityManager.Instantiate(MonsterEntity.Prefab);
            var instance = EntityManager.CreateEntity();
            var position = new float2(xy.x + 0.5f ,xy.y+ 0.5f);
            MonsterId++;
            EntityManager.AddComponentData(instance, new CPosition() { position =  position ,id = MonsterId ,radius = 0.25f});
            EntityManager.AddComponentData(instance, new CRotation() { rotation =  0.0f});
            EntityManager.AddComponentData(instance, new CMove() { i_m =  2.0f,v = float2.zero ,f = float2.zero ,last_f = float2.zero});
            float random_play_time = Random.Range(0.0f, 1.0f);
            EntityManager.AddComponentData(instance, new CMonsterState()
            {
                CurState =  0,MonsterType = 0 ,curHP = 1000 ,curAtkTime = random_play_time,
                isAfterAtk = false ,isInAtkCD = false
            });
            EntityManager.AddComponentData(instance, new CMonsterAnim() { cur_playTime = random_play_time});
            EntityManager.AddComponentData(instance, new CMonsterInfo()
            {
                maxHP = 1000 ,maxDeathTime = 1.0f ,atkDamage = 30 ,atkInterval = 2.0f ,atkRange = 0.5f,
                maxBeforeAtkTime = 1.0f,
            });
            EntityManager.AddComponentData(instance, new CHpPlane() { max_show_time = 1.0f ,cur_show_time = 1.0f,percent = 1.0f});
            
            
            //Insert(xy ,MonsterId ,new float2(xy.x + 0.5f ,xy.y+ 0.5f), 0.5f);
        }
        
        void Insert(int2 xy,uint id, float2 pos, float radius)
        {
            Collider collider = new Collider();
            collider.pos = pos;
            collider.radius = radius;
            collider.id = id;
            MapService.FlowFieldMap.map[xy.y * Setting.MapSize.x + xy.x].Colliders.Add(collider);
        }

    }
}