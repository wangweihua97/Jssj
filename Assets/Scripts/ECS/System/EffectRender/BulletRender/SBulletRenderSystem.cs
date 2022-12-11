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
    public partial class SBulletRenderSystem: SystemBase ,ICustomSystem
    {

        private const int BULLETS_MAX_SHOW_AMOUNT = 2000;
        private static int BULLETS_AMOUNT;
        
        private Mesh[] BulletsMesh;
        private Material[] BulletsMat;

        private NativeArray<Matrix4x4> BulletsWorldMat;
        private NativeArray<int> BulletsCount;
        private NativeArray<Matrix4x4> BulletsSelfMat;
        
        private Matrix4x4[] temp_mat;
        private int2 mapSize;
        
        EntityQuery m_Group;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Group = GetEntityQuery( typeof(CAttackRay));
        }

        public void Init()
        {
            InitMembers();
            InitmapTowerInfos();
            mapSize = Setting.MapSize;
        }

        void InitMembers()
        {
            Bullets bullets = Bullets.Instance;
            BULLETS_AMOUNT = bullets.BulletsGo.Count;
            BulletsMesh = new Mesh[BULLETS_AMOUNT];
            BulletsMat = new Material[BULLETS_AMOUNT];
            BulletsSelfMat = new NativeArray<Matrix4x4>(BULLETS_AMOUNT ,Allocator.Persistent);
            for (int i = 0; i < BULLETS_AMOUNT; i++)
            {
                BulletsMesh[i] = bullets.BulletsMesh[i];
                BulletsSelfMat[i] = bullets.BulletsSelfMat[i];
                BulletsMat[i] = bullets.BulletsMaterial[i];
            }
            
            BulletsCount = new NativeArray<int>(BULLETS_AMOUNT ,Allocator.Persistent);
            BulletsWorldMat = new NativeArray<Matrix4x4>(BULLETS_AMOUNT *BULLETS_MAX_SHOW_AMOUNT , Allocator.Persistent);
            temp_mat = new Matrix4x4[1000];
        }

        void InitmapTowerInfos()
        {
            int2 mapSize =  Setting.MapSize;
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
            
            NativeArray<Matrix4x4> bulletsWorldMat = this.BulletsWorldMat;
            NativeArray<Matrix4x4> bulletsSelfMat = this.BulletsSelfMat;

            int bulletsAmount= BULLETS_AMOUNT;
            NativeArray<int> bulletsCount = this.BulletsCount;
            Entities
                .WithStoreEntityQueryInField(ref m_Group)
                .ForEach((ref CAttackRay cAttackRay) =>
                {
                    int2 xy = new int2((int)cAttackRay.pos.x ,(int)cAttackRay.pos.y);
                    if(xy.x < indexMinX || xy.x > indexMaxX || xy.y < indexMiny || xy.y > indexMaxy)
                        return;
                    int count = bulletsCount[0];
                    if( count >= BULLETS_MAX_SHOW_AMOUNT)
                        return;
                    for (int i = 0; i < bulletsAmount ;i++)
                    {
                        bulletsWorldMat[i * BULLETS_MAX_SHOW_AMOUNT + count] = Matrix4x4.Translate(new Vector3(cAttackRay.pos.x, 1.0f, cAttackRay.pos.y))
                                                                               *Matrix4x4.Rotate(Quaternion.Euler(0, cAttackRay.rotation,0)) * 
                                                                               bulletsSelfMat[i];
                    }
                    bulletsCount[0] = count + 1;
                }).Schedule();
            Dependency.Complete();

            for (int i = 0; i < bulletsAmount ; i++)
            {
                int count = bulletsCount[0];
            
                int start = 0;
                while (count > 0)
                {
                    int cur_count = count > 1000 ? 1000 : count;
                    NativeArray<Matrix4x4>.Copy(bulletsWorldMat, i * BULLETS_MAX_SHOW_AMOUNT + start,temp_mat,0, cur_count);
                    MaterialPropertyBlock properties = new MaterialPropertyBlock();
                    Graphics.DrawMeshInstanced(BulletsMesh[i] ,0,BulletsMat[i],temp_mat,cur_count,properties,Setting.IsOpenShadow ?ShadowCastingMode.On : ShadowCastingMode.Off);
                    start += 1000;
                    count -= 1000;
                }
            }

            bulletsCount[0] = 0;
        }

        

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}