using System.Collections.Generic;
using Game.GlobalSetting;
using Game.Map;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
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
                atkInterval = 0.3f,
                atkRange = 4.0f,
                maxHP = 1000,
                atkPosOffset = 0.3f,
                bulletSpeed = 10.0f
            });
            float random_IdleTime = UnityEngine.Random.Range(0.0f, 1.0f);
            EntityManager.AddComponentData(instance, new CTowerState()
            {
                curAtkInterval = 0,
                curIdleTime = random_IdleTime,
                curHP = 1000
            });
            float random_rotation = UnityEngine.Random.Range(0.0f, 1.0f);
            EntityManager.AddComponentData(instance, new CTowerTransform() {
                rotation = random_rotation,
                xy = xy
            });
            Insert(xy ,type);
        }
        
        void Insert(int2 xy,int type)
        {
            MapNodeDir mapNodeDir = SPathfindingJobSystem.MapDirs[xy.y * Setting.MapSize.x + xy.x];
            mapNodeDir.canWalk = false;
            mapNodeDir.isTowar = true;
            SPathfindingJobSystem.MapDirs[xy.y * Setting.MapSize.x + xy.x] = mapNodeDir;
            
            MapNode2Struct mapNode2Struct = SPhysicsJob2System.MapBlock[xy.y * Setting.MapSize.x + xy.x];
            mapNode2Struct.canWalk = false;
            SPhysicsJob2System.MapBlock[xy.y * Setting.MapSize.x + xy.x] = mapNode2Struct;
        }
    }
}