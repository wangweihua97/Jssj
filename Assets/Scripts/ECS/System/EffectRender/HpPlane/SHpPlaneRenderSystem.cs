using Game.Camera;
using Game.GlobalSetting;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.ECS
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SHpPlaneRenderSystem: SystemBase ,ICustomSystem
    {

        private const int HP_PLANE_MAX_SHOW_AMOUNT = 1000;
        
        private Mesh HpPlaneMesh;
        private Material MonsterHpPlaneMat;
        private Material TowerHpPlaneMat;

        private NativeArray<Matrix4x4> MonsterHpPlaneWorldMat;
        private NativeArray<float> MonsterHpPlanePercent;
        
        private NativeArray<Matrix4x4> TowerHpPlaneWorldMat;
        private NativeArray<float> TowerHpPlanePercent;
        
        private Matrix4x4[] temp_mat;
        private float[] temp_showRatio;
        private int2 mapSize;
        private float m_scale = 0.5f;

        private NativeArray<int> MonsterAndTowerCount;
        private int ShaderId = UnityEngine.Shader.PropertyToID("_ShowRatio");
        
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        public void Init()
        {
            InitMembers();
            InitmapTowerInfos();
            mapSize = Setting.MapSize;
        }

        void InitMembers()
        {
            HpPlane hpPlane = HpPlane.Instance;
            HpPlaneMesh = hpPlane.HpPlaneMesh;
            MonsterHpPlaneMat = hpPlane.MonsterHpPlaneMaterial;
            TowerHpPlaneMat = hpPlane.TowerHpPlaneMaterial;

            MonsterHpPlaneWorldMat = new NativeArray<Matrix4x4>(HP_PLANE_MAX_SHOW_AMOUNT , Allocator.Persistent);
            MonsterHpPlanePercent = new NativeArray<float>(HP_PLANE_MAX_SHOW_AMOUNT , Allocator.Persistent);
            
            TowerHpPlaneWorldMat = new NativeArray<Matrix4x4>(HP_PLANE_MAX_SHOW_AMOUNT , Allocator.Persistent);
            TowerHpPlanePercent = new NativeArray<float>(HP_PLANE_MAX_SHOW_AMOUNT , Allocator.Persistent);
            MonsterAndTowerCount = new NativeArray<int>(2 , Allocator.Persistent);
            
            temp_mat = new Matrix4x4[1000];
            temp_showRatio = new float[1000];
        }

        void InitmapTowerInfos()
        {
            int2 mapSize =  Setting.MapSize;
        }

        protected override void OnUpdate()
        {
            float minx = MainCamera.Instance.min_x - 0.5f;
            float maxx = MainCamera.Instance.max_x + 0.5f;
            float miny = MainCamera.Instance.min_y - 0.5f;
            float maxy = MainCamera.Instance.max_y + 0.5f;
            
            NativeArray<Matrix4x4> monsterHpPlaneWorldMat = MonsterHpPlaneWorldMat;
            NativeArray<float> monsterHpPlanePercent = MonsterHpPlanePercent;
            float scale = m_scale;
            MonsterAndTowerCount[0] = 0;
            MonsterAndTowerCount[1] = 0;
            
            NativeArray<int> monsterAndTowerCount = MonsterAndTowerCount;
            Entities
                .WithBurst()
                .WithName("MonsterHpPlane")
                .ForEach((in CPosition cPosition ,in CHpPlane cHpPlane) =>
                {
                    if(monsterAndTowerCount[0] > HP_PLANE_MAX_SHOW_AMOUNT)
                        return;
                    
                    if(cHpPlane.cur_show_time >= cHpPlane.max_show_time)
                        return;
                    
                    if(cPosition.position.x < minx || cPosition.position.x > maxx 
                                                   ||cPosition.position.y < miny || cPosition.position.y > maxy )
                        return;

                    monsterHpPlaneWorldMat[monsterAndTowerCount[0]] =
                        Matrix4x4.Translate(new Vector3(cPosition.position.x, 2.0f, cPosition.position.y)) 
                        * Matrix4x4.Scale(new Vector3(scale,scale,scale));
                    monsterHpPlanePercent[monsterAndTowerCount[0]] = cHpPlane.percent;

                    monsterAndTowerCount[0]++;
                }).Schedule();

            NativeArray<Matrix4x4> towerHpPlaneWorldMat = TowerHpPlaneWorldMat;
            NativeArray<float> towerHpPlanePercent = TowerHpPlanePercent;
            Entities
                .WithBurst()
                .WithName("TowerHpPlane")
                .ForEach((in CTowerTransform cTowerTransform ,in CHpPlane cHpPlane) =>
                {
                    if(monsterAndTowerCount[1] > HP_PLANE_MAX_SHOW_AMOUNT)
                        return;
                    
                    if(cHpPlane.cur_show_time >= cHpPlane.max_show_time)
                        return;
                    
                    if(cTowerTransform.xy.x < minx || cTowerTransform.xy.x > maxx 
                                                         ||cTowerTransform.xy.y < miny || cTowerTransform.xy.y > maxy )
                        return;

                    towerHpPlaneWorldMat[monsterAndTowerCount[1]] =
                        Matrix4x4.Translate(new Vector3(cTowerTransform.xy.x + 0.5f, 2.0f, cTowerTransform.xy.y + 0.5f)) 
                        * Matrix4x4.Scale(new Vector3(scale,scale,scale));
                    towerHpPlanePercent[monsterAndTowerCount[1]] = cHpPlane.percent;

                    monsterAndTowerCount[1]++;
                }).Schedule();
            
            Dependency.Complete();

            int start = 0;
            int monsterHpPlaneAmount = MonsterAndTowerCount[0];
            while (monsterHpPlaneAmount > 0)
            {
                int cur_count = monsterHpPlaneAmount > 1000 ? 1000 : monsterHpPlaneAmount;
                NativeArray<Matrix4x4>.Copy(MonsterHpPlaneWorldMat,  start,temp_mat,0, cur_count);
                NativeArray<float>.Copy(monsterHpPlanePercent,  start,temp_showRatio,0, cur_count);
                MaterialPropertyBlock properties = new MaterialPropertyBlock();
                properties.SetFloatArray(ShaderId,temp_showRatio);
                Graphics.DrawMeshInstanced(HpPlaneMesh ,0,MonsterHpPlaneMat,temp_mat,cur_count,properties,ShadowCastingMode.Off);
                start += 1000;
                monsterHpPlaneAmount -= 1000;
            }
            
            start = 0;
            int towerHpPlaneAmount = MonsterAndTowerCount[1];
            while (towerHpPlaneAmount > 0)
            {
                int cur_count = towerHpPlaneAmount > 1000 ? 1000 : towerHpPlaneAmount;
                NativeArray<Matrix4x4>.Copy(towerHpPlaneWorldMat,  start,temp_mat,0, cur_count);
                NativeArray<float>.Copy(towerHpPlanePercent,  start,temp_showRatio,0, cur_count);
                MaterialPropertyBlock properties = new MaterialPropertyBlock();
                properties.SetFloatArray(ShaderId,temp_showRatio);
                Graphics.DrawMeshInstanced(HpPlaneMesh ,0,TowerHpPlaneMat,temp_mat,cur_count,properties,ShadowCastingMode.Off);
                start += 1000;
                towerHpPlaneAmount -= 1000;
            }
        }

        

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}