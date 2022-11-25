using Game.Camera;
using Game.GlobalSetting;
using Game.Map;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.ECS
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class STowersRenderSystem: SystemBase ,ICustomSystem
    {
        private const int TOWERS_MAX_SHOW_AMOUNT = 2000;
        private const int TOWERS_AMOUNT = (int)TowerCount.Default;
        
        private Mesh[] towersMesh;
        private Material[] towersMat;

        private NativeArray<Matrix4x4> towersWorldMat;
        private NativeArray<int> towersCount;
        private NativeArray<Matrix4x4> towersSelfMat;
        
        private Matrix4x4[] temp_mat;
        private int2 mapSize;
        
        EntityQuery m_Group;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Group = GetEntityQuery( ComponentType.ReadOnly<CTowerTransform>() ,ComponentType.ReadOnly<CTowerInfo>());
        }

        public void Init()
        {
            InitMembers();
            InitmapTowerInfos();
            mapSize = Setting.MapSize;
        }

        void InitMembers()
        {
            Towers towers = Towers.Instance;
            int totalObstaclesAmount = TOWERS_AMOUNT;
            towersMesh = new Mesh[totalObstaclesAmount];
            towersMat = new Material[totalObstaclesAmount];
            towersSelfMat = new NativeArray<Matrix4x4>(totalObstaclesAmount ,Allocator.Persistent);
            for (int i = 0; i < TOWERS_AMOUNT; i++)
            {
                towersMesh[i] = towers.TowersMesh[i];
                towersMat[i] = towers.TowersMaterial[i];
                towersSelfMat[i] = towers.TowersSelfMat[i];
            }
            
            towersCount = new NativeArray<int>(totalObstaclesAmount ,Allocator.Persistent);
            towersWorldMat = new NativeArray<Matrix4x4>(totalObstaclesAmount *TOWERS_MAX_SHOW_AMOUNT , Allocator.Persistent);
            temp_mat = new Matrix4x4[1000];
        }

        void InitmapTowerInfos()
        {
            int2 mapSize =  Setting.MapSize;
        }
        
        [BurstCompile]
        public struct TowersWorldMatJob: IJob
        {
            public int2 startPos;
            public int2 xyCount;
            public int xSize;
            public int ySize;
            [ReadOnly] 
            public NativeArray<MapTowerInfo> mapTowerInfos;
            [ReadOnly]
            public NativeArray<Matrix4x4> obstaclesSelfMat;
            [WriteOnly]
            public NativeArray<Matrix4x4> towersWorldMat;

            public NativeArray<int> towersCount;
            public void Execute()
            {
                for (int j = startPos.y; j < startPos.y + xyCount.y; j++)
                {
                    for (int i = startPos.x; i < startPos.x + xyCount.x; i++)
                    {
                        MapTowerInfo obstacleInfo = mapTowerInfos[j * xSize + i];
                        int towerType = obstacleInfo.TowerIndex;
                        if( towerType < 0)
                            continue;
                        int count = towersCount[towerType];
                        if( count >= TOWERS_MAX_SHOW_AMOUNT)
                            continue;
                        
                        towersWorldMat[towerType * TOWERS_MAX_SHOW_AMOUNT + count] = Matrix4x4.Translate(new Vector3(i + 0.5f, 0, j + 0.5f)) *
                                                                                     obstaclesSelfMat[towerType];
                        towersCount[towerType] = count + 1;
                    }
                }
                
            }
        }

        protected override void OnUpdate()
        {
            float minx = MainCamera.Instance.min_x;
            float maxx = MainCamera.Instance.max_x;
            float miny = MainCamera.Instance.min_y;
            float maxy = MainCamera.Instance.max_y;

            int indexMinX = (int)math.floor(minx);
            int indexMiny = (int)math.floor(miny);
            int indexMaxX = (int)math.floor(maxx);
            int indexMaxy = (int)math.floor(maxy);
            
            NativeArray<Matrix4x4> towersSelfMat = this.towersSelfMat;
            NativeArray<Matrix4x4> towersWorldMat = this.towersWorldMat;
            NativeArray<int> towersCount = this.towersCount;
            Entities
                .WithStoreEntityQueryInField(ref m_Group)
                .ForEach((in CTowerTransform cTowerTransform ,in CTowerInfo cTowerInfo) =>
                {
                    int2 xy = new int2(cTowerTransform.xy.x ,cTowerTransform.xy.y);
                    if(xy.x < indexMinX || xy.x > indexMaxX || xy.y < indexMiny || xy.y > indexMaxy)
                        return;
                    int count = towersCount[cTowerInfo.type];
                    if( count >= TOWERS_MAX_SHOW_AMOUNT)
                        return;
                    towersWorldMat[cTowerInfo.type * TOWERS_MAX_SHOW_AMOUNT + count] = Matrix4x4.Translate(new Vector3(xy.x + 0.5f, 0, xy.y + 0.5f))
                                                                                       *Matrix4x4.Rotate(Quaternion.Euler(0, cTowerTransform.rotation,0)) * 
                                                                                       towersSelfMat[cTowerInfo.type];
                    towersCount[cTowerInfo.type] = count + 1;
                    
                }).Schedule();
            Dependency.Complete();

            for (int i = 0; i < TOWERS_AMOUNT ; i++)
            {
                int count = towersCount[i];
            
                int start = 0;
                while (count > 0)
                {
                    int cur_count = count > 1000 ? 1000 : count;
                    NativeArray<Matrix4x4>.Copy(towersWorldMat, i * TOWERS_MAX_SHOW_AMOUNT + start,temp_mat,0, cur_count);
                    MaterialPropertyBlock properties = new MaterialPropertyBlock();
                    Graphics.DrawMeshInstanced(towersMesh[i] ,0,towersMat[i],temp_mat,cur_count,properties,ShadowCastingMode.Off);
                    start += 1000;
                    count -= 1000;
                }
            }

            for (int i = 0; i < TOWERS_AMOUNT ; i++)
            {
                towersCount[i] = 0;
            }
        }

        

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}