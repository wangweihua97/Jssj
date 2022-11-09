using System.Collections.Generic;
using Game.ECS;
using Game.Map;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class CMoveSystem : SystemBase
    {
        EntityQuery m_Group;
        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(ComponentType.ReadWrite<CRotation>(),
                ComponentType.ReadWrite<CPosition>(),
                ComponentType.ReadOnly<CMove>(),
                ComponentType.ReadOnly<Translation>()
                );
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
                        Value = positon.position + move.moveDir * move.moveSpeed * DeltaTime
                    };

                    chunkpositions[i] = new CPosition
                    {
                        position = positon.position + move.moveDir * move.moveSpeed * DeltaTime
                    };
                    chunkRotations[i] = new CRotation
                    {
                        rotation = new float3(0,-Mathf.Rad2Deg * math.atan2(move.moveDir.z ,move.moveDir.x),0)
                    };
                }
            }
        }

        protected override void  OnUpdate()
        {
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
                    
                    float2 posXY = new float2(cPosition.position.x ,cPosition.position.z);
                    int2 xy = new int2((int)cPosition.position.x ,(int)cPosition.position.z);
                    float2 dir = new float2(move.moveDir.x ,move.moveDir.z);
                    float length = deltaTime * move.moveSpeed;
                    float selfRadius = 0.5f;
                    var xys = GetAllNeedCheck(xy, posXY, dir, length, selfRadius);
                    
                    Check(xys, posXY, dir, length, out float2 newPos, out float2 newDir, out float newLength,
                        cPosition.indexInCell);

                    //newPos = posXY + newDir * newLength;
                    int2 newXy = new int2((int)newPos.x ,(int)newPos.y);
                    if(newXy.x < 0 || newXy.x >= FlowFieldMap.Size.x || newXy.y < 0 || newXy.y>= FlowFieldMap.Size.y)
                        return;
                    if (newXy.Equals(xy))
                    {
                        Insert(xy, cPosition.indexInCell, newPos, selfRadius);
                    }
                    else
                    {
                        if(!Insert(newXy, newPos, selfRadius ,out int newIndex))
                            return;
                        Delete(xy, cPosition.indexInCell);
                        cPosition.indexInCell = newIndex;
                    }

                    move.moveDir = new float3(newDir.x ,0,newDir.y);
                    translation.Value = new float3(newPos.x ,0 ,newPos.y);
                    cPosition.position = new float3(newPos.x ,0 ,newPos.y);


                }).Run();
        }

        void Insert(int2 xy,int index, float2 pos, float radius)
        {
            if (index == 0)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_1 = true;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity1.pos = pos;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity1.radius = radius;
            }
            else if (index == 1)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_2= true;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity2.pos = pos;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity2.radius = radius;
            }
            else if (index == 2)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_3 = true;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity3.pos = pos;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity3.radius = radius;
            }
            else if (index == 3)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_4 = true;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity4.pos = pos;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity4.radius = radius;
            }
            else if (index == 4)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_5 = true;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity5.pos = pos;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity5.radius = radius;
            }
            else if (index == 5)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_6 = true;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity6.pos = pos;
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.entity6.radius = radius;
            }
        }

        bool Insert(int2 newXy, float2 pos ,float radius ,out int newIndex)
        {
            var cell = MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x];
                
            var entityInCell = cell.EntityInCell;
            bool isFull = entityInCell.is_contarin_1 && entityInCell.is_contarin_2 && entityInCell.is_contarin_3 &&
                          entityInCell.is_contarin_4 && entityInCell.is_contarin_5 && entityInCell.is_contarin_6;
            newIndex = -1;
            if (isFull)
            {
                Debug.LogError("无法插入");
                return false;
            }
            
            else if (!MapService.FlowFieldMap.CanWalk(cell.CellType))
            {
                Debug.LogWarning("进入无法行走的位置");
            }

            if (!entityInCell.is_contarin_1)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_1 = true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity1.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity1.radius = radius;
                newIndex = 0;
            }
            else if (!entityInCell.is_contarin_2)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_2= true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity2.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity2.radius = radius;
                newIndex = 1;
            }
            else if (!entityInCell.is_contarin_3)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_3 = true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity3.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity3.radius = radius;
                newIndex = 2;
            }
            else if (!entityInCell.is_contarin_4)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_4 = true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity4.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity4.radius = radius;
                newIndex = 3;
            }
            else if (!entityInCell.is_contarin_5)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_5 = true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity5.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity5.radius = radius;
                newIndex = 4;
            }
            else if (!entityInCell.is_contarin_6)
            {
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.is_contarin_6 = true;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity6.pos = pos;
                MapService.FlowFieldMap.map[newXy.y * FlowFieldMap.Size.x + newXy.x].EntityInCell.entity6.radius = radius;
                newIndex = 5;
            }
            

            return true;
        }

        void Delete(int2 xy,int index)
        {
            if (index == 0)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_1 = false;
            }
            else if (index == 1)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_2 = false;
            }
            else if (index == 2)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_3 = false;
            }
            else if (index == 3)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_4 = false;
            }
            else if (index == 4)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_5 = false;
            }
            else if (index == 5)
            {
                MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].EntityInCell.is_contarin_6 = false;
            }
        }

        List<int2> GetAllNeedCheck(int2 xy ,float2 pos, float2 dir, float length ,float selfRadius)
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
            /*bool add_x = pos.x % 1.0 + selfRadius > selfRadius;
            bool red_x = pos.x % 1.0 - selfRadius < -selfRadius;
            
            bool add_y = pos.y % 1.0 + selfRadius > selfRadius;
            bool red_y = pos.y % 1.0 - selfRadius < -selfRadius;
            
            if(add_x)
                xys.Add(xy + new int2(1,0));
            else if(red_x)
                xys.Add(xy + new int2(-1,0));
            
            if(add_y)
                xys.Add(xy + new int2(0,1));
            else if(red_y)
                xys.Add(xy + new int2(0,-1));
            
            if(add_x && add_y)
                xys.Add(xy + new int2(1,1));
            if(red_x && red_y)
                xys.Add(xy + new int2(-1,-1));*/
            
            float2 newPos = pos;
            int2 newXy = xy;
            while (length > 0)
            {
                float2 oldPos = newPos;
                int2 oldXy = newXy;
                if (length > 1)
                {
                    newPos = pos + dir;
                    length -= 1;
                }
                else
                {
                    newPos = pos + dir * length;
                    length = 0;
                }
                newXy = new int2((int)newPos.x ,(int)newPos.y);
                if(oldXy.Equals(newXy))
                    break;
                List<int2> newXys = GetAllNeedCheck(newXy, newPos, dir, length, selfRadius);
                foreach (var n_xy in newXys)
                {
                    if(!xys.Contains(n_xy))
                        xys.Add(n_xy);
                }
            }
            
            /*float2 newPos = pos;
            int2 newXy = xy;
            while (length > 0)
            {
                float2 oldPos = newPos;
                int2 oldXy = newXy;
                if (length > 1)
                {
                    newPos = pos + dir;
                    length -= 1;
                }
                else
                {
                    newPos = pos + dir * length;
                    length = 0;
                }
                newXy = new int2((int)newPos.x ,(int)newPos.y);

                !newXy.Equals(oldXy);
                bool add_self = oldPos.x + selfRadius
            }*/
            
            return xys;
        }

        /*bool NotIn(int2 xy, float2 pos ,float Radius)
        {
            int2 newXy = new int2((int)pos.x ,(int)pos.y);
            if(newXy.Equals(xy))
                return true;
        }*/

        void Check(List<int2> xys, float2 pos, float2 dir, float length ,out float2 newPos ,out float2 newDir,out float newLength ,int index)
        {
             newPos = pos + dir * length;
             newDir = dir;
             newLength = length;
             int2 meXy = new int2((int)pos.x ,(int)pos.y);
             /*float minT = length;
             float2 minDir = dir;
             float2 minPos = pos + minDir * length;*/
            foreach (var xy in xys)
            {
                if (xy.x < 0 || xy.x >= FlowFieldMap.Size.x || xy.y < 0 || xy.y >= FlowFieldMap.Size.y)
                {
                    continue;
                }
                    
                var cell = MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x];
                
                var entityInCell = cell.EntityInCell;
                bool isFull = entityInCell.is_contarin_1 && entityInCell.is_contarin_2 && entityInCell.is_contarin_3 &&
                              entityInCell.is_contarin_4 && entityInCell.is_contarin_5 && entityInCell.is_contarin_6;
                if (!meXy.Equals(xy) && (isFull || !MapService.FlowFieldMap.CanWalk(cell.CellType)))
                {
                    /*if (index == -1)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            Map.Collider Get_Collider = this.Get_Collider(ref entityInCell,i);
                            float2 dis = (pos - Get_Collider.pos);
                            if (dis.x * dis.x + dis.y * dis.y < 0.001f)
                            {
                                index = i;
                            }

                    }*/
                    if (CheckCollision(newPos, 0.5f, xy + new float2(0.5f, 0.5f), 1.0f))
                    {
                        CollideCircular(0.5f,xy + new float2(0.5f,0.5f),
                            1f,dir,pos, ref newPos, ref newDir, ref  newLength);
                        int2 newXy = new int2((int)newPos.x ,(int)newPos.y);
                        if (newXy.Equals(xy))
                        {
                            float2 dif = newPos - meXy;
                            dif.x = dif.x < 0 ? 0 : dif.x;
                            dif.y = dif.y < 0 ? 0 : dif.y;
                            
                            dif.x = dif.x >= 1 ? 0.999f : dif.x;
                            dif.y = dif.y >= 1 ? 0.999f : dif.y;
                            newPos = meXy + dif;
                        }
                    }
                    
                    /*if (newt < minT)
                    {
                        minT = newt;
                        minDir = newDir;
                        minPos = newPos;
                    }*/
                }
                
                {
                    
                    for (int i = 0; i < 6; i++)
                    {
                        if (i == index && meXy.Equals(xy))
                        {
                            continue;
                        }
                            
                        if (HasCollider(ref entityInCell,i))
                        {
                            Map.Collider collider = Get_Collider(ref entityInCell,i);
                            /*if (index == -1)
                            {
                                float2 dis = (pos - Get_Collider.pos);
                                if ( dis.x * dis.x + dis.y * dis.y < 0.001f)
                                {
                                    index = i;
                                }
                            }*/
                            if(CheckCollision(newPos ,0.5f,collider.pos,0.5f))
                                CollideCircular(0.5f,collider.pos,
                                0.5f, dir, pos,ref newPos, ref newDir, ref  newLength);
                            /*if (newt < minT)
                            {
                                minT = newt;
                                minDir = newDir;
                                minPos = newPos;
                            }*/
                            
                        }
                    }
                }
                
            }
            /*newDir = minDir;
            newLength = minT;
            newPos = minPos;*/
        }

        bool HasCollider(ref EntityInCell entityInCell, int index)
        {
            if (index == 0)
            {
                return entityInCell.is_contarin_1;
            }
            else if (index == 1)
            {
                return entityInCell.is_contarin_2;
            }
            else if (index == 2)
            {
                return entityInCell.is_contarin_3;
            }
            else if (index == 3)
            {
                return entityInCell.is_contarin_4;
            }
            else if (index == 4)
            {
                return entityInCell.is_contarin_5;
            }
            else if (index == 5)
            {
                return entityInCell.is_contarin_6;
            }

            return false;
        }
        
        bool CheckCollision(float2 selfPos ,float selfRadius ,float2 anotherPos ,float anotherRadius) 
        {
            bool collisionX = selfPos.x + selfRadius >= anotherPos.x &&
                              anotherPos.x + anotherRadius >= selfPos.x;
            bool collisionY = selfPos.y + selfRadius >= anotherPos.y &&
                              anotherPos.y + anotherRadius >= selfPos.y;
            return collisionX && collisionY;
        }  


        /*void CollideCircular(float selfRadius ,float2 anotherPos ,float anotherRadius,
                             float2 pos ,ref float2 newDir ,ref float newLength)
        {
            float2 oldPos = pos;
            float2 oldDir = newDir;
            float oldLength = newLength;
            //float a = dir.x * dir.x + dir.y * dir.y;
            float m = anotherPos.x - oldPos.x;
            float n = anotherPos.y - oldPos.y;
            float b = -2 * (oldDir.x * m + oldDir.y * n);
            float c = m * m + n * n - (selfRadius + anotherRadius) * (selfRadius + anotherRadius);
            //float w = b * b - 4 * a * c;
            float w = b * b - 4 * c;
            if(w < 0)
                return;
            //float t = 0.5f * (-b - math.sqrt(w)) / a;
            float t = 0.5f * (-b - math.sqrt(w));
            
            if(t<0)
                t = 0.5f * (-b + math.sqrt(w));
                //t = 0.5f * (-b + math.sqrt(w)) / a;
            if(t<0 || t >  oldLength)
                return;
            float2 newPos = oldPos + newDir * t;
            if(math.dot(newPos - anotherPos ,oldDir) > 0)
                return;

            t = math.max(0.0f, t - 0.0001f);

            newPos = oldPos + newDir * t;

            float2 leave = newDir * (newLength - t);
            float2 newN = math.normalize(anotherPos - newPos);
            float2 newT = new float2(newN.y, -newN.x);
            newPos += newT * math.dot(leave, newT);
            newLength = math.length(newPos - oldPos);
            newDir = (newPos - oldPos)/newLength;
        }*/
        
        void CollideCircular(float selfRadius ,float2 anotherPos ,float anotherRadius,float2 oldDir,float2 pos,
            ref float2 newPos ,ref float2 newDir ,ref float newLength)
        {
            float2 oldPos = pos;
            float oldLength = newLength;
            //float a = dir.x * dir.x + dir.y * dir.y;
            float m = anotherPos.x - oldPos.x;
            float n = anotherPos.y - oldPos.y;
            float b = -2 * (oldDir.x * m + oldDir.y * n);
            float c = m * m + n * n - (selfRadius + anotherRadius) * (selfRadius + anotherRadius);
            //float w = b * b - 4 * a * c;
            float w = b * b - 4 * c;
            if(w < 0)
                return;
            //float t = 0.5f * (-b - math.sqrt(w)) / a;
            float t = 0.5f * (-b - math.sqrt(w));
            
            /*if(t<-0.1f)
                t = 0.5f * (-b + math.sqrt(w));*/
            
            //t = 0.5f * (-b + math.sqrt(w)) / a;
            if(t >  oldLength || t < -0.001f)
                return; 
            /*if(math.dot(oldPos - anotherPos ,oldDir) > 0)
                return;*/

            if(t > 0)
                 t = math.max(0.0f, t - 0.0001f);
            newLength = t;

            newPos = oldPos + oldDir * t;
            
            float2 newN = math.normalize(anotherPos - newPos);
            float2 newT = new float2(newN.y, -newN.x);
            newT = math.dot(newT, oldDir) > 0 ? newT : -newT;
            newDir = newT;
            /*float2 leave = newDir * (newLength - t);
            float2 newN = math.normalize(anotherPos - newPos);
            float2 newT = new float2(newN.y, -newN.x);
            newPos += newT * math.dot(leave, newT);
            newLength = math.length(newPos - oldPos);
            newDir = (newPos - oldPos)/newLength;*/
        }

        void CollideSquare(float selfRadius ,float2 anotherPos ,float anotherRadius,float2 oldDir,
            ref float2 newPos ,ref float2 newDir ,ref float newLength ,out float newt)
        {
            float2 oldPos = newPos;
            float oldLength = newLength;
            newt = newLength;
            //float a = dir.x * dir.x + dir.y * dir.y;
            float m = anotherPos.x - oldPos.x;
            float n = anotherPos.y - oldPos.y;
            float b = -2 * (oldDir.x * m + oldDir.y * n);
            float c = m * m + n * n - (selfRadius + anotherRadius) * (selfRadius + anotherRadius);
            //float w = b * b - 4 * a * c;
            float w = b * b - 4 * c;
            if(w < 0)
                return;
            //float t = 0.5f * (-b - math.sqrt(w)) / a;
            float t = 0.5f * (-b - math.sqrt(w));
            
            if(t<0)
                t = 0.5f * (-b + math.sqrt(w));
            //t = 0.5f * (-b + math.sqrt(w)) / a;
            if(t<0 || t >  oldLength)
                return; 
            if(math.dot(oldPos - anotherPos ,oldDir) > 0)
                return;

            t = math.max(0.0f, t - 0.001f);
            newt = t;

            newPos = oldPos + oldDir * t;
            
            float2 newN = math.normalize(anotherPos - newPos);
            float2 newT = new float2(newN.y, -newN.x);
            newT = math.dot(newT, oldDir) > 0 ? newT : -newT;
            newDir = newT;
            /*float2 leave = newDir * (newLength - t);
            float2 newN = math.normalize(anotherPos - newPos);
            float2 newT = new float2(newN.y, -newN.x);
            newPos += newT * math.dot(leave, newT);
            newLength = math.length(newPos - oldPos);
            newDir = (newPos - oldPos)/newLength;*/
        }

        bool Get_is_contarin(ref EntityInCell entityInCell,int index)
        {
            if (index == 0)
            {
                return entityInCell.is_contarin_1;
            }
            if (index == 1)
            {
                return entityInCell.is_contarin_2;
            }
            if (index == 2)
            {
                return entityInCell.is_contarin_3;
            }
            if (index == 3)
            {
                return entityInCell.is_contarin_4;
            }
            if (index == 4)
            {
                return entityInCell.is_contarin_5;
            }
            if (index == 5)
            {
                return entityInCell.is_contarin_6;
            }

            return false;
        }
        
        Map.Collider Get_Collider(ref EntityInCell entityInCell,int index)
        {
            if (index == 0)
            {
                return entityInCell.entity1;
            }
            if (index == 1)
            {
                return entityInCell.entity2;
            }
            if (index == 2)
            {
                return entityInCell.entity3;
            }
            if (index == 3)
            {
                return entityInCell.entity4;
            }
            if (index == 4)
            {
                return entityInCell.entity5;
            }
            if (index == 5)
            {
                return entityInCell.entity6;
            }

            Debug.LogError("不存在实体");
            return new Map.Collider();
        }
        

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}