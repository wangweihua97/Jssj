using System.Collections.Generic;
using Game.ECS;
using Game.GlobalSetting;
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
    public partial class CMoveJobSystem : SystemBase
    {
        EntityQuery m_Group;
        const float SpringForce = 100.0f;
        protected override void OnCreate()
        { 
            m_Group = GetEntityQuery(ComponentType.ReadOnly<CRotation>(),
                ComponentType.ReadOnly<CPosition>(),
                ComponentType.ReadOnly<CMove>()
                );
        }
        
        [BurstCompile]
        //public struct MyJob : IJobParallelFor
        public struct MyJob : IJobParallelFor
        {
            public int xCount;
            public int yCount;
            public float deltaTime;
            public float springForce;
            [ReadOnly]
            public NativeArray<MapNodeStruct> ms;
            [ReadOnly]
            public NativeArray<EntitieInfo> es;
            public NativeArray<EntitieInfo> result;
            
            

            public void Execute(int i)
            {
                EntitieInfo source = es[i];
                int2 curXy = new int2((int)source.position.x ,(int)source.position.y);
                MapNodeStruct mapNode =  ms[curXy.y * xCount + curXy.x];
                int2 xy = curXy;

                
                float2 posXY = source.position;

                float2 newPos;
                float2 v;
                
                if (!mapNode.canWalk)
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
                 MapNodeStruct mapNode =  ms[xy.y * xCount + xy.x];
                 
                 if(!CheckCollision(pos ,radius,xy + new float2(0.5f, 0.5f),1.5f))
                     return;
                    
                 if (!mapNode.canWalk)
                 {
                     if (CheckCollision(pos, radius, xy + new float2(0.5f, 0.5f), 1.0f))
                     {
                         f += CollideCircular(pos ,radius,xy + new float2(0.5f,0.5f),
                             1f);
                     }
                 }
                    
                 {
                        
                     for (int i = mapNode.startPos; i < mapNode.startPos + mapNode.count; i++)
                     {
                         EntitieInfo entitieInfo = es[i];
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
                float pressureLen = selfRadius + anotherRadius - dis_len;
                f = springForce * (0.1f * pressureLen + pressureLen * pressureLen)  * dis / dis_len;
                return f;
            }
        }

        protected override void  OnUpdate()
        {
            return;
            var deltaTime = Time.DeltaTime;
            int xCount = Setting.MapSize.x;
            int yCount = Setting.MapSize.y;
            List<EntitieInfo>[] es = new List<EntitieInfo>[xCount* yCount];
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    es[j * xCount + i] = new List<EntitieInfo>();
                }
            }

            int dataCount = m_Group.CalculateEntityCount();
            NativeArray<EntitieInfo> allEntitieInfo
                = new NativeArray<EntitieInfo>(dataCount, Allocator.Persistent);
            NativeArray<EntitieInfo> result
                = new NativeArray<EntitieInfo>(dataCount, Allocator.Persistent);
            NativeArray<int> entitieIndex
                = new NativeArray<int>(dataCount, Allocator.Persistent);
            NativeArray<MapNodeStruct> mapNode
                = new NativeArray<MapNodeStruct>(xCount* yCount, Allocator.Persistent);
            
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
                    entitieInfo.last_f = cm.last_f;
                    entitieInfo.i_m = cm.i_m;
                    entitieInfo.isAlive = false;
                    entitieInfo.isAtk = false;
                    int2 xy = new int2((int)cp.position.x ,(int)cp.position.y);
                    es[xy.y * xCount + xy.x].Add(entitieInfo);
                })
                .Run();
            

            int index = 0;
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    MapNodeStruct mapNodeStruct;
                    mapNodeStruct.startPos = index;
                    mapNodeStruct.count = es[j * xCount + i].Count;
                    mapNodeStruct.canWalk =
                        MapService.FlowFieldMap.map[j * xCount + i].CellType < 10;
                    mapNode[j * xCount + i] = mapNodeStruct;
                    for (int k = 0; k < es[j * xCount + i].Count; k++)
                    {
                        allEntitieInfo[index] = es[j * xCount + i][k];
                        if (es[j * xCount + i][k].index >= dataCount)
                        {
                            Debug.LogError("----------------");
                        }
                        
                        entitieIndex[es[j * xCount + i][k].index] = index;
                        index++;
                        if (index > dataCount)
                        {
                            Debug.LogError("----------------");
                        }
                    }
                }
            }
            
            MyJob jobData = new MyJob();
            jobData.ms = mapNode;
            jobData.es = allEntitieInfo;
            jobData.xCount = xCount;
            jobData.yCount = yCount;
            jobData.deltaTime = deltaTime;
            jobData.springForce = SpringForce;
            jobData.result = result;
            //调度作业
            JobHandle handle = jobData.Schedule(dataCount, 1);  
            //等待作业的完成
            handle.Complete();
            
            Entities
                .WithName("RotationSpeedSystem_SpawnAndRemove")
                .WithoutBurst()
                .ForEach((int entityInQueryIndex,ref Translation translation,ref CPosition cPosition, ref CRotation cRotation ,ref CMove move) =>
                {
                    EntitieInfo entitieInfo = result[entitieIndex[entityInQueryIndex]];
                    move.v = entitieInfo.v;
                    translation.Value = new float3(entitieInfo.position.x ,0 ,entitieInfo.position.y);
                    cPosition.position = entitieInfo.position;
                }).Run();
            
            Dependency.Complete();
            es = null;
            allEntitieInfo.Dispose();
            entitieIndex.Dispose();
            mapNode.Dispose();
            result.Dispose();
        }
        
        


        public void OnDestroy(ref SystemState state)
        {
        }
    }
}