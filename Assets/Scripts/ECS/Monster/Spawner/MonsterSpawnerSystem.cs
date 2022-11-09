using System.Collections.Generic;
using Game.Map;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class MonsterSpawnerSystem : SystemBase
    {
        BeginInitializationEntityCommandBufferSystem m_MonsterCommandBufferSystem;

        private List<int2> m_willCreatMonsterPos;

        protected override void OnCreate()
        {
            // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
            m_MonsterCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            m_willCreatMonsterPos = new List<int2>();
        }

        protected override void OnUpdate()
        {
            ;
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
            Insert(xy, new float2(xy.x + 0.5f, xy.y + 0.5f), 0.5f ,out int index);
            if(index == -1)
                return;
            var EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var instance = EntityManager.Instantiate(MonsterEntity.Prefab);
            var position = new float3(xy.x + 0.5f ,0,xy.y+ 0.5f);
            EntityManager.AddComponentData(instance, new CPosition() { position =  position ,indexInCell = index});
            EntityManager.AddComponentData(instance, new CRotation() { rotation =  float3.zero});
            EntityManager.AddComponentData(instance, new CMove() {  moveSpeed =  4.0f,moveDir = float3.zero});
            
        }
        
        bool Insert(int2 newXy, float2 pos ,float radius ,out int index)
        {
            var cell = MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x];
                
            var entityInCell = cell.EntityInCell;
            bool isFull = entityInCell.is_contarin_1 && entityInCell.is_contarin_2 && entityInCell.is_contarin_3 &&
                          entityInCell.is_contarin_4 && entityInCell.is_contarin_5 && entityInCell.is_contarin_6;
            index = -1;
            if (isFull)
            {
                Debug.LogError("无法插入");
                return false;
            }

            if (!entityInCell.is_contarin_1)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_1 = true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity1.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity1.radius = radius;
                index = 0;
            }
            else if (!entityInCell.is_contarin_2)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_2= true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity2.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity2.radius = radius;
                index = 1;
            }
            else if (!entityInCell.is_contarin_3)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_3 = true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity3.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity3.radius = radius;
                index = 2;
            }
            else if (!entityInCell.is_contarin_4)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_4 = true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity4.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity4.radius = radius;
                index = 3;
            }
            else if (!entityInCell.is_contarin_5)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_5 = true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity5.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity5.radius = radius;
                index = 4;
            }
            else if (!entityInCell.is_contarin_6)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_6 = true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity6.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity6.radius = radius;
                index = 5;
            }
            

            return true;
        }
        
    }
}