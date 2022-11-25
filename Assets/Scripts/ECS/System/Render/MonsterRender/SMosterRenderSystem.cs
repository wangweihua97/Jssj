using System;
using System.Linq;
using Game.Camera;
using Game.GlobalSetting;
using Game.VAT;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Game.ECS
{
    
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SMosterRenderSystem  : SystemBase ,ICustomSystem
    {
        
        
        private const float VAT_FRAMERATE = 60.0f;
        private const int MAX_MONSTER_SHOW_COUNT = 10000;
        private int ShaderPlayPosId = Shader.PropertyToID("_PlayPos");
        
        
        EntityQuery m_Group;
        public int MonsterTypeCount;
        public NativeArray<MonsterAnimInfo> MonsterAnimInfos;


        public NativeArray<int> MonsterCount;
        public NativeArray<float> AnimPlayPoses;
        public NativeArray<Matrix4x4> Mats;
        public NativeArray<TempAnimInfo> TempAnimInfos;
        
        
        private float[] temp_as = new float[1000];
        private Matrix4x4[] temp_ma = new Matrix4x4[1000];
        private float[] max_as = new float[1024];
        private Matrix4x4[] max_ma = new Matrix4x4[1024];
        
        CalculateMatAndPlayTimeJob calculateMatAndPlayTimeJob;
        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(typeof(CMonsterAnim) ,ComponentType.ReadOnly<CRotation>(), ComponentType.ReadOnly<CPosition>() 
                ,ComponentType.ReadOnly<CMonsterState>());
            
        }

        public void Init()
        {
            InitMonsterAnimInfos();
            InitMembers();
            calculateMatAndPlayTimeJob = new CalculateMatAndPlayTimeJob();
        }
        
        

        void InitMonsterAnimInfos()
        {
            Monsters monsters = Monsters.Instance;
            MonsterTypeCount = monsters.allMonsters.Count;
            MonsterAnimInfos = 
                new NativeArray<MonsterAnimInfo>( MonsterTypeCount, Allocator.Persistent);
            

            for (int i = 0; i < monsters.allMonsterVAT_Info.Count; i++)
            {
                var vatInfo = monsters.allMonsterVAT_Info[i];
                MonsterAnimInfo monsterAnimInfo;
                monsterAnimInfo.i_vat_size = 1.0f / vatInfo.VAT_Size;
                monsterAnimInfo.atk_anim_pos = vatInfo.AnimInfos[vatInfo.AtkAnimIndex].x / vatInfo.VAT_Size;
                monsterAnimInfo.atk_anim_time = vatInfo.AnimInfos[vatInfo.AtkAnimIndex].y / vatInfo.VAT_Size;
                
                monsterAnimInfo.run_anim_pos = vatInfo.AnimInfos[vatInfo.RunAnimIndex].x / vatInfo.VAT_Size;
                monsterAnimInfo.run_anim_time = vatInfo.AnimInfos[vatInfo.RunAnimIndex].y / vatInfo.VAT_Size;
                
                monsterAnimInfo.death_anim_pos = vatInfo.AnimInfos[vatInfo.DeathAnimIndex].x / vatInfo.VAT_Size;
                monsterAnimInfo.death_anim_time = vatInfo.AnimInfos[vatInfo.DeathAnimIndex].y / vatInfo.VAT_Size;
                monsterAnimInfo.self_mat = monsters.selfMats[i];
                MonsterAnimInfos[i] = monsterAnimInfo;
            }
        }

        void InitMembers()
        {
            MonsterCount = new NativeArray<int>(MonsterTypeCount , Allocator.Persistent);
            
            for (int i = 0; i < MonsterTypeCount; i++)
            {
                MonsterCount[i] = 0;
            }
            AnimPlayPoses = new NativeArray<float>(MonsterTypeCount * MAX_MONSTER_SHOW_COUNT , Allocator.Persistent);
            Mats = new NativeArray<Matrix4x4>(MonsterTypeCount * MAX_MONSTER_SHOW_COUNT , Allocator.Persistent);
            TempAnimInfos = new NativeArray<TempAnimInfo>(MonsterTypeCount * MAX_MONSTER_SHOW_COUNT , Allocator.Persistent);
        }
        
        public struct TempAnimInfo
        {
            public int type;
            public float posx;
            public float posy;
            public float rotation;
            public float cur_play_time;
        }
    
        [BurstCompile]
        public struct CalculateMatAndPlayTimeJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<MonsterAnimInfo> MonsterAnimInfos;
            [ReadOnly]
            public NativeArray<TempAnimInfo> TempAnimInfos;
            [ReadOnly]
            public NativeArray<int> MonsterCount;
            [WriteOnly]
            public NativeArray<float> AnimPlayPoses;
            [WriteOnly]
            public NativeArray<Matrix4x4> Mats;
            public void Execute(int index)
            {
                int type = index / MAX_MONSTER_SHOW_COUNT;
                int cur_index = index - type * MAX_MONSTER_SHOW_COUNT; 
                int count = MonsterCount[type];
                if(cur_index >= count)
                    return;
                MonsterAnimInfo monsterAnimInfo = MonsterAnimInfos[type];
                TempAnimInfo tempAnimInfo = TempAnimInfos[index];
                
                
                Matrix4x4 m = Matrix4x4.Translate(new Vector3(tempAnimInfo.posx, 0.0f, tempAnimInfo.posy))
                              * Matrix4x4.Rotate(Quaternion.Euler(0.0f,tempAnimInfo.rotation,0.0f)) * monsterAnimInfo.self_mat;;

                AnimPlayPoses[index] = tempAnimInfo.cur_play_time ;
                Mats[index] = m;
            }
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("CalculateRenderdara");

            float minx = MainCamera.Instance.min_x - 0.5f;
            float maxx = MainCamera.Instance.max_x + 0.5f;
            float miny = MainCamera.Instance.min_y - 0.5f;
            float maxy = MainCamera.Instance.max_y + 0.5f;
            
            float deltaTime = Time.DeltaTime;
            var monsterAnimInfos = MonsterAnimInfos;
            var monsterCount = MonsterCount;
            var animPlayPoses = AnimPlayPoses;
            var mats = Mats;
            var tempAnimInfos = TempAnimInfos;
            /*Entities
                .WithStoreEntityQueryInField(ref m_Group)
                .ForEach((ref CMonsterAnim cma ,in CRotation cr ,in CPosition cp , in CMonsterState cms) =>
                {
                    if(cp.position.x < minx || cp.position.x > maxx 
                        ||cp.position.y < miny || cp.position.y > maxy )
                        return;
                    int type = cms.MonsterType;
                    MonsterAnimInfo monsterAnimInfo = monsterAnimInfos[type];
                    Matrix4x4 m = Matrix4x4.Translate(new Vector3(cp.position.x, 0.0f, cp.position.y))
                                  * Matrix4x4.Rotate(Quaternion.Euler(0.0f,cr.rotation,0.0f)) * monsterAnimInfo.self_mat;;
                    float time = math.fmod(cma.cur_playTime + deltaTime * monsterAnimInfo.i_vat_size * VAT_FRAMERATE ,monsterAnimInfo.run_anim_time);
                    float animPlayPos = monsterAnimInfo.run_anim_pos + time;
                    cma.cur_playTime = time;

                    int count = monsterCount[type];
                    int index = type * MAX_MONSTER_SHOW_COUNT + count;
                    animPlayPoses[index] = animPlayPos;
                    mats[index] = m;
                    monsterCount[type] = count + 1;

                })
                .Schedule();
            Dependency.Complete();*/
            Entities
                .WithStoreEntityQueryInField(ref m_Group)
                .ForEach((ref CMonsterAnim cma ,in CRotation cr ,in CPosition cp , in CMonsterState cms) =>
                {
                    //#if !CLOSE_CUll
                    if(cp.position.x < minx || cp.position.x > maxx 
                                            ||cp.position.y < miny || cp.position.y > maxy )
                        return;
                    //#endif
                    int type = cms.MonsterType;
                    MonsterAnimInfo monsterAnimInfo = monsterAnimInfos[type];
                    TempAnimInfo tempAnimInfo;
                    float playTime = 0;
                    if (cms.CurState == 0)
                    {
                        playTime = math.fmod(cma.cur_playTime + deltaTime* monsterAnimInfo.i_vat_size * VAT_FRAMERATE,monsterAnimInfo.run_anim_time);
                        tempAnimInfo.cur_play_time = monsterAnimInfo.run_anim_pos+playTime; 
                    }
                    else
                    {
                        playTime = math.min(cma.cur_playTime + deltaTime* monsterAnimInfo.i_vat_size * VAT_FRAMERATE,monsterAnimInfo.death_anim_time);
                        tempAnimInfo.cur_play_time = monsterAnimInfo.death_anim_pos+playTime; 
                    }
                    
                    
                    tempAnimInfo.type = type;
                    tempAnimInfo.posx = cp.position.x;
                    tempAnimInfo.posy = cp.position.y;
                    tempAnimInfo.rotation = cr.rotation;
                    
                    cma.cur_playTime = playTime;
                    
                    int count = monsterCount[type];
                    int index = type * MAX_MONSTER_SHOW_COUNT + count;
                    tempAnimInfos[index] = tempAnimInfo;
                    monsterCount[type] = count + 1;

                })
                .Schedule();
            Dependency.Complete();
            
            calculateMatAndPlayTimeJob.Mats = mats;
            calculateMatAndPlayTimeJob.MonsterCount = monsterCount;
            calculateMatAndPlayTimeJob.AnimPlayPoses = animPlayPoses;
            calculateMatAndPlayTimeJob.TempAnimInfos = tempAnimInfos;
            calculateMatAndPlayTimeJob.MonsterAnimInfos = monsterAnimInfos;
            
            JobHandle updateHandle =calculateMatAndPlayTimeJob.Schedule(MonsterTypeCount * MAX_MONSTER_SHOW_COUNT ,1);
            updateHandle.Complete();
            
            Profiler.EndSample();

            Monsters monsters = Monsters.Instance;
            
            for (int i = 0; i < MonsterTypeCount; i++)
            {
                int count = MonsterCount[i];
                
                if (max_as.Length < count)
                {
                    ResizeCapacity(count);
                }

                NativeArray<float>.Copy(animPlayPoses,i * MAX_MONSTER_SHOW_COUNT, max_as,0, count);
                NativeArray<Matrix4x4>.Copy(mats, i * MAX_MONSTER_SHOW_COUNT,max_ma,0, count);
                int start = 0;
                while (count > 0)
                {
                    int cur_count = count > 1000 ? 1000 : count;
                    Array.Copy(max_as, start, temp_as, 0, cur_count);
                    Array.Copy(max_ma, start, temp_ma, 0, cur_count);
                    MaterialPropertyBlock properties = new MaterialPropertyBlock();
                    properties.SetFloatArray(ShaderPlayPosId ,temp_as);
                    Graphics.DrawMeshInstanced(monsters.allMeshs[i] ,0,monsters.allMats[i],temp_ma,cur_count,properties,ShadowCastingMode.Off);

                    start += 1000;
                    count -= 1000;
                }
                
            }
            for (int i = 0; i < MonsterTypeCount; i++)
            {
                MonsterCount[i] = 0;
            }
            
        }

        void ResizeCapacity(int needCount)
        {
            max_as = new float[2 * max_as.Length];
            max_ma = new Matrix4x4[2 * max_ma.Length];

            if (max_as.Length < needCount)
                ResizeCapacity(needCount);
        }
        
        
        public void OnDestroy(ref SystemState state)
        {
        }
        
        
    }
    
    
}