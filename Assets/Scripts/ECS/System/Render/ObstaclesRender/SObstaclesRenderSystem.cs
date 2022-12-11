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
    public partial class SObstaclesRenderSystem : SystemBase ,ICustomSystem
    {
        private const int OBSTACLE_MAX_SHOW_AMOUNT = 2000;
        private const int TREE_AMOUNT = (int)ObstacleCount.Tree;
        private const int ROCK_AMOUNT = (int)ObstacleCount.Rock;
        
        private Mesh[] obstaclesMesh;
        private Material[] obstaclesMat;

        private NativeArray<Matrix4x4> obstaclesWorldMat;
        private NativeArray<int> obstaclesCount;
        private NativeArray<Matrix4x4> obstaclesSelfMat;
        
        private Matrix4x4[] temp_mat;

        private NativeArray<MapObstacleInfo> mapObstacleInfos;

        private ObstaclesWorldMatJob _matJob;
        private int2 mapSize;
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        public void Init()
        {
            InitMembers();
            InitMapObstacleInfos();
            _matJob = new ObstaclesWorldMatJob();
            mapSize = Setting.MapSize;
        }

        void InitMembers()
        {
            Obstacles obstacles = Obstacles.Instance;
            int totalObstaclesAmount = TREE_AMOUNT + ROCK_AMOUNT;
            obstaclesMesh = new Mesh[totalObstaclesAmount];
            obstaclesMat = new Material[totalObstaclesAmount];
            obstaclesSelfMat = new NativeArray<Matrix4x4>(totalObstaclesAmount ,Allocator.Persistent);
            for (int i = 0; i < TREE_AMOUNT; i++)
            {
                obstaclesMesh[i] = obstacles.TreesMesh[i];
                obstaclesMat[i] = obstacles.TreesMaterial[i];
                obstaclesSelfMat[i] = obstacles.TreesSelfMat[i];
            }
            
            for (int i = 0; i < ROCK_AMOUNT; i++)
            {
                obstaclesMesh[i + TREE_AMOUNT] = obstacles.RocksMesh[i];
                obstaclesMat[i + TREE_AMOUNT] = obstacles.RocksMaterial[i];
                obstaclesSelfMat[i + TREE_AMOUNT] = obstacles.RocksSelfMat[i];
            }
            
            obstaclesCount = new NativeArray<int>(totalObstaclesAmount ,Allocator.Persistent);
            obstaclesWorldMat = new NativeArray<Matrix4x4>(totalObstaclesAmount *OBSTACLE_MAX_SHOW_AMOUNT , Allocator.Persistent);
            temp_mat = new Matrix4x4[1000];
        }

        void InitMapObstacleInfos()
        {
            int2 mapSize =  Setting.MapSize;
            mapObstacleInfos = new NativeArray<MapObstacleInfo>(mapSize.x * mapSize.y ,Allocator.Persistent);

            for (int i = 0; i < mapSize.x; i++)
            {
                for (int j = 0; j < mapSize.y; j++)
                {
                    MapObstacleInfo mapObstacleInfo;
                    int cellType = (int)MapService.FlowFieldMap.map[j * mapSize.x + i].CellType;
                    
                    if (cellType >= (int) CellType.Tree_1 && cellType < (int) CellType.Tree_1 + TREE_AMOUNT)
                    {
                        mapObstacleInfo.obstacleIndex =  cellType - (int) CellType.Tree_1;
                    }
                    else if (cellType >= (int) CellType.Rock_1 && cellType < (int) CellType.Rock_1 + ROCK_AMOUNT)
                    {
                        mapObstacleInfo.obstacleIndex =  TREE_AMOUNT + cellType - (int) CellType.Rock_1;
                    }
                    else
                    {
                        mapObstacleInfo.obstacleIndex = -1;
                    }

                    mapObstacleInfos[j * mapSize.x + i] = mapObstacleInfo;
                }
            }
        }
        
        [BurstCompile]
        public struct ObstaclesWorldMatJob: IJob
        {
            public int2 startPos;
            public int2 xyCount;
            public int xSize;
            public int ySize;
            [ReadOnly] 
            public NativeArray<MapObstacleInfo> mapObstacleInfos;
            [ReadOnly]
            public NativeArray<Matrix4x4> obstaclesSelfMat;
            [WriteOnly]
            public NativeArray<Matrix4x4> ObstaclesWorldMat;

            public NativeArray<int> obstaclesCount;
            public void Execute()
            {
                for (int j = startPos.y; j < startPos.y + xyCount.y; j++)
                {
                    for (int i = startPos.x; i < startPos.x + xyCount.x; i++)
                    {
                        if(i < 0 || i >= xSize || j < 0 || j> ySize)
                            continue;
                        MapObstacleInfo obstacleInfo = mapObstacleInfos[j * xSize + i];
                        int obstacleType = obstacleInfo.obstacleIndex;
                        if( obstacleType < 0)
                            continue;
                        int count = obstaclesCount[obstacleType];
                        if( count >= OBSTACLE_MAX_SHOW_AMOUNT)
                            continue;
                        
                        ObstaclesWorldMat[obstacleType * OBSTACLE_MAX_SHOW_AMOUNT + count] = Matrix4x4.Translate(new Vector3(i + 0.5f, 0, j + 0.5f)) *
                                                                                             obstaclesSelfMat[obstacleType];
                        obstaclesCount[obstacleType] = count + 1;
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

            int2 xyCount = new int2(indexMaxX - indexMinX + 1 ,indexMaxy - indexMiny + 1);
            _matJob.startPos = new int2(indexMinX ,indexMiny);
            _matJob.xyCount = xyCount;
            _matJob.xSize = mapSize.x;
            _matJob.ySize = mapSize.y;
            _matJob.obstaclesCount = obstaclesCount;
            _matJob.mapObstacleInfos = mapObstacleInfos;
            _matJob.obstaclesSelfMat = obstaclesSelfMat;
            _matJob.ObstaclesWorldMat = obstaclesWorldMat;
            
            JobHandle jobHandle =_matJob.Schedule();
            jobHandle.Complete();

            for (int i = 0; i < TREE_AMOUNT + ROCK_AMOUNT; i++)
            {
                int count = obstaclesCount[i];
            
                int start = 0;
                while (count > 0)
                {
                    int cur_count = count > 1000 ? 1000 : count;
                    NativeArray<Matrix4x4>.Copy(obstaclesWorldMat, i * OBSTACLE_MAX_SHOW_AMOUNT + start,temp_mat,0, cur_count);
                    MaterialPropertyBlock properties = new MaterialPropertyBlock();
                    Graphics.DrawMeshInstanced(obstaclesMesh[i] ,0,obstaclesMat[i],temp_mat,cur_count,properties,Setting.IsOpenShadow ?ShadowCastingMode.On : ShadowCastingMode.Off);
                    start += 1000;
                    count -= 1000;
                }
            }

            for (int i = 0; i < TREE_AMOUNT + ROCK_AMOUNT; i++)
            {
                obstaclesCount[i] = 0;
            }
        }

        

        protected override void OnDestroy()
        {
            base.OnDestroy();
            obstaclesWorldMat.Dispose();
            obstaclesCount.Dispose();
            obstaclesSelfMat.Dispose();
            mapObstacleInfos.Dispose();
        }
    }
}