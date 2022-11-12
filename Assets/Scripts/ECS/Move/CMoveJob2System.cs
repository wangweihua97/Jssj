﻿using System.Collections.Generic;
using Game.ECS;
using Game.Map;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class CMoveJob2System : SystemBase
    {
        EntityQuery m_Group;
        const float SpringForce = 10.0f;
        private const int MapNodeMaxContainEntitieCount = 12;
        private NativeArray<EntitieInfo> mapNodeEs;
        private NativeArray<MapNode2Struct> canWalks;
        protected override void OnCreate()
        { 
            m_Group = GetEntityQuery(ComponentType.ReadOnly<CRotation>(),
                ComponentType.ReadOnly<CPosition>(),
                ComponentType.ReadOnly<CMove>()
                );
        }

        public void Init()
        {
            int xCount = FlowFieldMap.Size.x;
            int yCount = FlowFieldMap.Size.y;
            mapNodeEs = 
                new NativeArray<EntitieInfo>(xCount * yCount * MapNodeMaxContainEntitieCount, Allocator.Persistent);
            canWalks
                = new NativeArray<MapNode2Struct>(xCount * yCount, Allocator.Persistent);
            
            MapCanWalkJob mapCanWalkJob = new MapCanWalkJob();
            mapCanWalkJob.xCount = xCount;
            mapCanWalkJob.canWalks = canWalks;
            JobHandle mapCanWalkJobHandle = mapCanWalkJob.Schedule(xCount * yCount, 1);  
            mapCanWalkJobHandle.Complete();
        }
        
        [BurstCompile]
        public struct MyJob2 : IJobParallelFor
        {
            public int xCount;
            public int yCount;
            public int zCount;
            public float deltaTime;
            public float SpringForce;
            [ReadOnly]
            public NativeArray<EntitieInfo> mapNodeEs;
            [ReadOnly]
            public NativeArray<MapNode2Struct> mapCanWalk;
            [ReadOnly]
            public NativeArray<EntitieInfo> es;
            public NativeArray<EntitieInfo> result;
            
            

            public void Execute(int i)
            {
                EntitieInfo source = es[i];
                int2 curXy = new int2((int)source.position.x ,(int)source.position.y);
                int2 xy = curXy;

                
                float2 posXY = source.position;

                float2 newPos;
                float2 v;
                
                
                
                if (!mapCanWalk[xy.y * xCount + xy.x].canWalk)
                {
                    float2 offset = math.abs(posXY - xy - new float2(0.5f,0.5f));
                    bool is_x_bigger = offset.x > offset.y;
                    if (is_x_bigger)
                    {
                        bool is_right = posXY.x > xy.x + 0.5f;
                        newPos = is_right ? new float2(xy.x + 1.01f,posXY.y) : new float2(xy.x - 0.01f,posXY.y);
                        v = is_right ? new float2(math.abs(source.v.x) ,source.v.y) :new float2(-math.abs(source.v.x) ,source.v.y) ;
                    }
                    else
                    {
                        bool is_up = posXY.y > xy.y + 0.5f ;
                        newPos = is_up? new float2(posXY.x ,xy.y + 1.01f) : new float2(posXY.x ,xy.y - 0.01f);
                        v = is_up ? new float2(source.v.x ,math.abs(source.v.y)) :new float2(source.v.x ,-math.abs(source.v.y));
                    }
                }
                else
                {
                    float2 f = float2.zero;
                    Check(xy + new int2(1,-1), source.id ,posXY,source.radius ,ref f);
                    Check(xy + new int2(1,0), source.id ,posXY,source.radius ,ref f);
                    Check(xy + new int2(1,1), source.id ,posXY,source.radius ,ref f);
                    Check(xy + new int2(0,-1), source.id ,posXY,source.radius ,ref f);
                    Check(xy + new int2(0,0), source.id ,posXY,source.radius ,ref f);
                    Check(xy + new int2(0,1), source.id ,posXY,source.radius ,ref f);
                    Check(xy + new int2(-1,-1), source.id ,posXY,source.radius ,ref f);
                    Check(xy + new int2(-1,0), source.id ,posXY,source.radius ,ref f);
                    Check(xy + new int2(-1,1), source.id ,posXY,source.radius ,ref f);

                    float2 totalF = f + source.f;
                    v = deltaTime * totalF * source.i_m + source.v;
                    newPos = posXY + 0.5f * (v+source.v)  * deltaTime;
                }
                //newPos = posXY + newDir * newLength;
                int2 newXy = new int2((int)newPos.x ,(int)newPos.y);
                if(newXy.x < 0 || newXy.x >= xCount || newXy.y < 0 || newXy.y>= yCount)
                    return;
                source.v = v;
                source.position = new float2(newPos.x ,newPos.y);
                
                result[i] = source;
            }
            
            void Check(int2 xy,uint id, float2 pos ,float radius,ref float2 f)
            {
                 if (xy.x < 0 || xy.x >= xCount || xy.y < 0 || xy.y >= yCount)
                 {
                     return;
                 }

                 int mapIndex = xy.y * xCount + xy.x;
                 MapNode2Struct mapNode2Struct = mapCanWalk[mapIndex];
                 
                 if(!CheckCollision(pos ,radius,xy + new float2(0.5f, 0.5f),1.5f))
                     return;
                    
                 if (!mapNode2Struct.canWalk)
                 {
                     if (CheckCollision(pos, radius, xy + new float2(0.5f, 0.5f), 1.0f))
                     {
                         f += CollideCircular(pos ,radius,xy + new float2(0.5f,0.5f),
                             1f);
                     }
                 }
                    
                 {
                        
                     for (int i = 0; i < mapNode2Struct.count; i++)
                     {
                         EntitieInfo entitieInfo = mapNodeEs[xy.y * (xCount * zCount) + xy.x * zCount + i];
                         if(id.Equals(entitieInfo.id))
                             continue;
                         if(CheckCollision(pos ,radius,entitieInfo.position,entitieInfo.radius))
                             f += CollideCircular(pos ,radius,entitieInfo.position,
                                 0.5f);
                                
                     }
                 }
            }
            
            bool CheckCollision(float2 selfPos ,float selfRadius ,float2 anotherPos ,float anotherRadius) 
            {
                bool collisionX = Mathf.Abs(anotherPos.x - selfPos.x) < selfRadius + anotherRadius;
                bool collisionY = Mathf.Abs(anotherPos.y - selfPos.y) < selfRadius + anotherRadius;
                return collisionX && collisionY;
            }  


            
            float2 CollideCircular(float2 pos,float selfRadius ,float2 anotherPos ,float anotherRadius)
            {
                float2 f = float2.zero;
                float2 dis = pos - anotherPos;
                float dis_len = math.max(0.01f ,math.length(dis));
                if(dis_len >= selfRadius + anotherRadius)
                    return f;
                f = SpringForce * (selfRadius + anotherRadius - dis_len) * dis / dis_len;
                return f;
            }
        }

        [BurstCompile]
        public struct MapClearJob : IJobParallelFor
        {
            public NativeArray<MapNode2Struct> canWalks;
            public void Execute(int index)
            {
                MapNode2Struct mapNode2Struct = canWalks[index];
                mapNode2Struct.count = 0;
                canWalks[index] = mapNode2Struct;
            }
        }
        
        public struct MapCanWalkJob: IJobParallelFor
        {
            public int xCount;
            public NativeArray<MapNode2Struct> canWalks;
            public void Execute(int index)
            {
                MapNode2Struct mapNode2Struct;
                mapNode2Struct.count = 0;
                int y = index / xCount;
                int x = index - y * xCount;
                mapNode2Struct.canWalk=
                    MapService.FlowFieldMap.map[y * xCount + x].CellType < 10;
                canWalks[index] = mapNode2Struct;
            }
        }

        protected override void  OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            int xCount = FlowFieldMap.Size.x;
            int yCount = FlowFieldMap.Size.y;
            int zCount = MapNodeMaxContainEntitieCount;
            
            
            int dataCount = m_Group.CalculateEntityCount();
            NativeArray<EntitieInfo> es
                = new NativeArray<EntitieInfo>(dataCount, Allocator.Persistent);
            NativeArray<EntitieInfo> result
                = new NativeArray<EntitieInfo>(dataCount, Allocator.Persistent);
            
            MapClearJob mapClearJob = new MapClearJob();
            mapClearJob.canWalks = canWalks;
            JobHandle mapClearHandle = mapClearJob.Schedule(xCount * yCount, 1);  
            mapClearHandle.Complete();
            
            
            Entities
                .WithStoreEntityQueryInField(ref m_Group)
                .WithoutBurst()
                .ForEach((int entityInQueryIndex, in CRotation cr ,in CPosition cp , in CMove cm) =>
                {
                    EntitieInfo entitieInfo;
                    entitieInfo.index = entityInQueryIndex;
                    entitieInfo.rotation = cr.rotation;
                    entitieInfo.position = cp.position;
                    entitieInfo.radius = cp.radius;
                    entitieInfo.id = cp.id;
                    entitieInfo.v = cm.v;
                    entitieInfo.f = cm.f;
                    entitieInfo.i_m = cm.i_m;
                    
                    int2 xy = new int2((int)cp.position.x ,(int)cp.position.y);
                    MapNode2Struct mapNode2Struct = canWalks[xy.y * xCount + xy.x];
                    mapNodeEs[xy.y * (xCount * zCount) + xy.x * zCount + mapNode2Struct.count] = entitieInfo;
                    if(mapNode2Struct.count < zCount)
                        mapNode2Struct.count += 1;
                    canWalks[xy.y * xCount + xy.x] = mapNode2Struct;

                    es[entityInQueryIndex] = entitieInfo;

                })
                .Run();
            
            MyJob2 jobData = new MyJob2();
            jobData.mapNodeEs = mapNodeEs;
            jobData.mapCanWalk = canWalks;
            jobData.es = es;
            jobData.xCount = xCount;
            jobData.yCount = yCount;
            jobData.zCount = zCount;
            jobData.deltaTime = deltaTime;
            jobData.SpringForce = SpringForce;
            jobData.result = result;
            //调度作业
            JobHandle handle = jobData.Schedule(dataCount, 1);  
            //等待作业的完成
            handle.Complete();
            Entities
                .WithName("RotationSpeedSystem_SpawnAndRemove")
                .WithBurst()
                .ForEach((int entityInQueryIndex,ref Translation translation,ref CPosition cPosition, ref CRotation cRotation ,ref CMove move) =>
                {
                    EntitieInfo entitieInfo = result[entityInQueryIndex];
                    move.v = entitieInfo.v;
                    translation.Value = new float3(entitieInfo.position.x ,0 ,entitieInfo.position.y);
                    cPosition.position = entitieInfo.position;
                }).ScheduleParallel();
            
            Dependency.Complete();
            es.Dispose();
            result.Dispose();
        }
        
        


        public void OnDestroy(ref SystemState state)
        {
        }
    }
}