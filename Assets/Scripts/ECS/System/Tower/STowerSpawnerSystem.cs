using System.Collections.Generic;
using Game.GlobalSetting;
using Game.Map;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [AlwaysUpdateSystem]
    public partial class STowerSpawnerSystem : SystemBase
    {
        BeginInitializationEntityCommandBufferSystem m_TowerCommandBufferSystem;
        
        public static List<(int2, int)> AddList;
        private static uint TowerId = 1000;

        protected override void OnCreate()
        {
            m_TowerCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            
            AddList = new List<(int2, int)>();
        }

        protected override void OnUpdate()
        {
            if (AddList.Count > 0)
            {
                foreach (var xy_type in AddList)
                {
                    CreatTower(xy_type.Item1, xy_type.Item2);
                }
                AddList.Clear();
            }
            
            var commandBuffer = m_TowerCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            int2 mapSize = Setting.MapSize;

            var mapNodeInfos= SPathfindingJobSystem.MapNodeInfos;
            var mapBlock = SPhysicsJob2System.MapBlock;
            Entities
                .WithBurst()
                .ForEach((Entity entity, int entityInQueryIndex,in CTowerInfo cTowerInfo,in CTowerState cTowerState
                    ,in CTowerTransform cTowerTransform) =>
                {
                    if (cTowerState.curState == 2)
                    {
                        commandBuffer.DestroyEntity(entityInQueryIndex ,entity);
                        MapNodeInfo mapNodeDir = mapNodeInfos[cTowerTransform.xy.y * Setting.MapSize.x + cTowerTransform.xy.x];
                        mapNodeDir.canWalk = true;
                        mapNodeDir.isTowar = false;
                        mapNodeInfos[cTowerTransform.xy.y * Setting.MapSize.x + cTowerTransform.xy.x] = mapNodeDir;
            
                        MapNode2Struct mapNode2Struct =mapBlock[cTowerTransform.xy.y * Setting.MapSize.x + cTowerTransform.xy.x];
                        mapNode2Struct.canWalk = true;
                        mapBlock[cTowerTransform.xy.y * Setting.MapSize.x + cTowerTransform.xy.x] = mapNode2Struct;
                    }
                }).Schedule();
            m_TowerCommandBufferSystem.AddJobHandleForProducer(Dependency);
            Dependency.Complete();
        }

        public void CreatTower(int2 xy ,int type)
        {
            var EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var instance = EntityManager.CreateEntity();
            TowerId++;
            EntityManager.AddComponentData(instance, new CTowerInfo()
            {
                type = type,
                atkDamage = 30,
                atkInterval = 0.2f,
                atkRange = 8.0f,
                maxHP = 200,
                atkPosOffset = 0.3f,
                bulletSpeed = 30.0f
            });
            float random_IdleTime = UnityEngine.Random.Range(0.0f, 1.0f);
            EntityManager.AddComponentData(instance, new CTowerState()
            {
                curState = 0,
                curAtkInterval = 0,
                curIdleTime = random_IdleTime,
                curHP = 200
            });
            float random_rotation = UnityEngine.Random.Range(0.0f, 1.0f);
            EntityManager.AddComponentData(instance, new CTowerTransform() {
                rotation = random_rotation,
                xy = xy
            });
            
            EntityManager.AddComponentData(instance, new CHpPlane()
            {
                max_show_time = 1.0f ,cur_show_time = 1.0f,percent = 1.0f
            });
            Insert(xy ,type);
        }
        
        void Insert(int2 xy,int type)
        {
            MapNodeInfo mapNodeDir = SPathfindingJobSystem.MapNodeInfos[xy.y * Setting.MapSize.x + xy.x];
            mapNodeDir.canWalk = false;
            mapNodeDir.isTowar = true;
            SPathfindingJobSystem.MapNodeInfos[xy.y * Setting.MapSize.x + xy.x] = mapNodeDir;
            
            MapNode2Struct mapNode2Struct = SPhysicsJob2System.MapBlock[xy.y * Setting.MapSize.x + xy.x];
            mapNode2Struct.canWalk = false;
            SPhysicsJob2System.MapBlock[xy.y * Setting.MapSize.x + xy.x] = mapNode2Struct;
        }
    }
}