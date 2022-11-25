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
using UnityEngine.UIElements;
using Collider = Game.Map.Collider;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class SPhysicsSystem : SystemBase
    {
        EntityQuery m_Group;
        const float SpringForce = 10.0f;
        protected override void OnCreate()
        { 
            /*m_Group = GetEntityQuery(ComponentType.ReadOnly<CRotation>(),
                ComponentType.ReadOnly<CPosition>(),
                ComponentType.ReadOnly<CMove>()
                );*/
        }
        struct RotationSpeedJob : IJobChunk
        {
            public float DeltaTime;
            public ComponentTypeHandle<Translation> RPositionTypeHandle;
            public ComponentTypeHandle<CRotation> RotationTypeHandle;
            public ComponentTypeHandle<CPosition> PositionTypeHandle;
            [ReadOnly] public ComponentTypeHandle<CMove> MoveTypeHandle;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkRPosition = chunk.GetNativeArray(RPositionTypeHandle);
                var chunkRotations = chunk.GetNativeArray(RotationTypeHandle);
                var chunkpositions = chunk.GetNativeArray(PositionTypeHandle);
                var chunkMoves = chunk.GetNativeArray(MoveTypeHandle);
                for (var i = 0; i < chunk.Count; i++)
                {
                    var rotation = chunkRotations[i];
                    var positon = chunkpositions[i];
                    var move = chunkMoves[i];
                    chunkRPosition[i] = new Translation
                    {
                    };

                    chunkpositions[i] = new CPosition
                    {
                    };
                    chunkRotations[i] = new CRotation
                    {
                    };
                }
            }
        }

        protected override void  OnUpdate()
        {
            return;
            /*var rotationType = GetComponentTypeHandle<CRotation>();
            var positionType = GetComponentTypeHandle<CPosition>();
            var rPositionType = GetComponentTypeHandle<Translation>();
            var moveType = GetComponentTypeHandle<CMove>(true);

            var job = new RotationSpeedJob()
            {
                RPositionTypeHandle = rPositionType,
                RotationTypeHandle = rotationType,
                PositionTypeHandle = positionType,
                MoveTypeHandle = moveType,
                DeltaTime = Time.DeltaTime
            };
            
            Dependency = job.ScheduleSingle(m_Group, Dependency);*/
            var deltaTime = Time.DeltaTime;

            // The in keyword on the RotationSpeed_SpawnAndRemove component tells the job scheduler that this job will not write to rotSpeedSpawnAndRemove
            Entities
                .WithName("RotationSpeedSystem_SpawnAndRemove")
                .WithoutBurst()
                .ForEach((ref Translation translation,ref CPosition cPosition, ref CRotation cRotation ,ref CMove move) =>
                {
                    
                    float2 posXY = new float2(cPosition.position.x ,cPosition.position.y);
                    int2 xy = new int2((int)cPosition.position.x ,(int)cPosition.position.y);
                    float selfRadius = 0.5f;

                    float2 newPos;
                    float2 v;
                    
                    if (!MapService.FlowFieldMap.CanWalk(MapService.FlowFieldMap.map[xy.y * Setting.MapSize.x + xy.x].CellType))
                    {
                        float2 offset = math.abs(posXY - xy - new float2(0.5f,0.5f));
                        bool is_x_bigger = offset.x > offset.y;
                        if (is_x_bigger)
                        {
                            bool is_right = posXY.x > xy.x + 0.5f;
                            newPos = is_right ? new float2(xy.x + 1.01f,posXY.y) : new float2(xy.x - 0.01f,posXY.y);
                            v = is_right ? new float2(math.abs(move.v.x) ,move.v.y) :new float2(-math.abs(move.v.x) ,move.v.y) ;
                        }
                        else
                        {
                            bool is_up = posXY.y > xy.y + 0.5f ;
                            newPos = is_up? new float2(posXY.x ,xy.y + 1.01f) : new float2(posXY.x ,xy.y - 0.01f);
                            v = is_up ? new float2(move.v.x ,math.abs(move.v.y)) :new float2(move.v.x ,-math.abs(move.v.y));
                        }
                    }
                    else
                    {
                        var xys = GetAllNeedCheck(xy);
                    
                        Check(xys, cPosition.id ,posXY,out float2 f);

                        float2 totalF = f + move.f;
                        v = deltaTime * totalF * move.i_m + move.v;
                        newPos = posXY + 0.5f * (v+move.v)  * deltaTime;
                    }
                    //newPos = posXY + newDir * newLength;
                    int2 newXy = new int2((int)newPos.x ,(int)newPos.y);
                    if(newXy.x < 0 || newXy.x >= Setting.MapSize.x || newXy.y < 0 || newXy.y>= Setting.MapSize.y)
                        return;
                    if (newXy.Equals(xy))
                    {
                        Update(xy, cPosition.id, newPos, selfRadius);
                    }
                    else
                    {
                        Delete(xy, cPosition.id);
                        Insert(newXy, cPosition.id, newPos, selfRadius);
                    }
                    move.v = v;
                    translation.Value = new float3(newPos.x ,0 ,newPos.y);
                    cPosition.position = new float2(newPos.x ,newPos.y);
                }).Run();
        }

        void Update(int2 xy,uint id, float2 pos, float radius)
        {
            var colliders = MapService.FlowFieldMap.map[xy.y * Setting.MapSize.x + xy.x].Colliders;
            Collider collider = new Collider();
            collider.pos = pos;
            collider.radius = radius;
            collider.id = id;
            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i].id == id)
                {
                    colliders[i] = collider;
                    break;
                }
            }
        }
        

        void Insert(int2 xy,uint id, float2 pos, float radius)
        {
            Collider collider = new Collider();
            collider.pos = pos;
            collider.radius = radius;
            collider.id = id;
            MapService.FlowFieldMap.map[xy.y * Setting.MapSize.x + xy.x].Colliders.Add(collider);
        }
        

        void Delete(int2 xy,uint id)
        {
            var colliders = MapService.FlowFieldMap.map[xy.y * Setting.MapSize.x + xy.x].Colliders;
            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i].id == id)
                {
                    colliders.RemoveAt(i);
                    return;
                }
            }

            Debug.LogError("_____________________");
        }

        List<int2> GetAllNeedCheck(int2 xy)
        {
            List<int2> xys = new List<int2>();
            xys.Add(xy + new int2(1,-1));
            xys.Add(xy + new int2(1,0));
            xys.Add(xy + new int2(1,1));
            xys.Add(xy + new int2(0,-1));
            xys.Add(xy + new int2(0,0));
            xys.Add(xy + new int2(0,1));
            xys.Add(xy + new int2(-1,-1));
            xys.Add(xy + new int2(-1,0));
            xys.Add(xy + new int2(-1,1));
            return xys;
        }
        void Check(List<int2> xys,uint id, float2 pos ,out float2 f)
        {
             f = float2.zero; 
             int2 meXy = new int2((int)pos.x ,(int)pos.y);
            foreach (var xy in xys)
            {
                if (xy.x < 0 || xy.x >= Setting.MapSize.x || xy.y < 0 || xy.y >= Setting.MapSize.y)
                {
                    continue;
                }
                    
                var cell = MapService.FlowFieldMap.map[xy.y * Setting.MapSize.x + xy.x];
                
                if(!CheckCollision(pos ,0.5f,xy + new float2(0.5f, 0.5f),1.5f))
                    continue;
                
                if (!MapService.FlowFieldMap.CanWalk(cell.CellType))
                {
                    if (CheckCollision(pos, 0.5f, xy + new float2(0.5f, 0.5f), 1.0f))
                    {
                        f += CollideCircular(pos ,0.5f,xy + new float2(0.5f,0.5f),
                            1f);
                    }
                }
                
                {
                    
                    for (int i = 0; i < cell.Colliders.Count; i++)
                    {
                        if(id.Equals(cell.Colliders[i].id))
                            continue;
                        if(CheckCollision(pos ,0.5f,cell.Colliders[i].pos,0.5f))
                            f += CollideCircular(pos ,0.5f,cell.Colliders[i].pos,
                                0.5f);
                            
                    }
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
        
        

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}