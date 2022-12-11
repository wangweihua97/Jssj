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
using UnityEngine.UIElements;
using NotImplementedException = System.NotImplementedException;

namespace Game.ECS
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SGroundRenderSystem  : SystemBase ,ICustomSystem
    {
        private const int PLAN_SIZE = 10;
        private Mesh planMesh;
        private Material planMat;
        private int2 mapSize;

        private NativeArray<Matrix4x4> planWorldMat;
        private Matrix4x4[] temp_mat;
        private PlanWorldMatJob planWorldMatJob;
        private Matrix4x4 selfMat;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            mapSize = Setting.MapSize;
            planWorldMat = new NativeArray<Matrix4x4>(100,Allocator.Persistent);
            temp_mat = new Matrix4x4[1000];
            planWorldMatJob = new PlanWorldMatJob();
        }

        public void Init()
        {
            Planes planes = Planes.Instance;
            planMesh = planes.PlanMesh;
            planMat = planes.PlanMaterial;
            selfMat = planes.selfMats;
        }
        
        [BurstCompile]
        public struct PlanWorldMatJob: IJobParallelFor
        {
            public int2 startPos;
            public int xCount;
            public int yCount;
            public Matrix4x4 selfMat;
            public NativeArray<Matrix4x4> planWorldMat;
            public void Execute(int index)
            {
                int y = index / xCount;
                int x = index - y * xCount;
                planWorldMat[index] = Matrix4x4.Translate(new Vector3((startPos.x + x+0.5f)*PLAN_SIZE,0 ,(startPos.y + y+0.5f)*PLAN_SIZE)) * selfMat;
            }
        }
        
        protected override void OnUpdate()
        {
            float minx = MainCamera.Instance.min_x;
            float maxx = MainCamera.Instance.max_x;
            float miny = MainCamera.Instance.min_y;
            float maxy = MainCamera.Instance.max_y;

            int indexMinX = (int)math.floor(minx / PLAN_SIZE);
            int indexMiny = (int)math.floor(miny / PLAN_SIZE);
            int indexMaxX = (int)math.floor(maxx / PLAN_SIZE);
            int indexMaxy = (int)math.floor(maxy / PLAN_SIZE);

            int xCount = indexMaxX - indexMinX + 1;
            int yCount = indexMaxy - indexMiny + 1;
            int totalCount = xCount * yCount;
            if (totalCount > planWorldMat.Length)
                ResizeCapacity(totalCount);
            
            planWorldMatJob.startPos = new int2(indexMinX ,indexMiny);
            planWorldMatJob.xCount = xCount;
            planWorldMatJob.yCount = yCount;
            planWorldMatJob.planWorldMat = planWorldMat;
            planWorldMatJob.selfMat = selfMat;
            
            JobHandle jobHandle =planWorldMatJob.Schedule(totalCount ,1);
            jobHandle.Complete();

            int count = totalCount;
            
            int start = 0;
            while (count > 0)
            {
                int cur_count = count > 1000 ? 1000 : count;
                NativeArray<Matrix4x4>.Copy(planWorldMat, start,temp_mat,0, count);
                MaterialPropertyBlock properties = new MaterialPropertyBlock();
                Graphics.DrawMeshInstanced(planMesh ,0,planMat,temp_mat,cur_count,properties,ShadowCastingMode.Off);
                start += 1000;
                count -= 1000;
            }

        }
        
        void ResizeCapacity(int needCount)
        {
            planWorldMat.Dispose();
            planWorldMat = new NativeArray<Matrix4x4>(needCount,Allocator.Persistent);
        }

        

        protected override void OnDestroy()
        {
            base.OnDestroy();
            planWorldMat.Dispose();
        }
    }
}