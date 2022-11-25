using Game.GlobalSetting;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(SPhysicsJob2System))]
    public partial class CRayHitSystem  : SystemBase ,ICustomSystem
    {
        EntityQuery m_Group;
        EntityQuery m_RayGroup;
        public static NativeArray<MonsterBeHit> MonsterBeHits;
        private HitJob m_hitJob;
        private ClearJob m_clearJob;
        protected override void OnCreate()
        { 
            m_Group = GetEntityQuery(
                ComponentType.ReadOnly<CMonsterInfo>(),
                ComponentType.ReadOnly<CMonsterState>()
            );
            m_RayGroup = GetEntityQuery(
                typeof(CAttackRay)
            );
        }

        public void Init()
        {
            MonsterBeHits = new NativeArray<MonsterBeHit>(10000 ,Allocator.Persistent);
            m_hitJob = new HitJob();
            m_clearJob = new ClearJob();
            m_clearJob.monsterBeHits = MonsterBeHits;
            JobHandle handle = m_clearJob.Schedule(MonsterBeHits.Length, 1);  
            handle.Complete();
        }
        
        [BurstCompile]
        struct ClearJob: IJobParallelFor
        {
            public NativeArray<MonsterBeHit> monsterBeHits;
            public void Execute(int index)
            {
                MonsterBeHit monsterBeHit = monsterBeHits[index];
                monsterBeHit.damage = 0;
                monsterBeHits[index] = monsterBeHit;
            }
        }
        
        [BurstCompile]
        public partial struct HitJob : IJobEntity
        {
            public int2 mapSize;
            public int zCount;
            public float deltaTime;
            [ReadOnly]
            public NativeArray<EntitieInfo> mapNodeEs;
            [ReadOnly]
            public NativeArray<MapNode2Struct> mapBlock;
            public void Execute(ref CAttackRay cAttackRay)
            {
                CAttackRay attackRay = cAttackRay;
                float2 start = attackRay.pos;
                float rayLength = math.min(deltaTime * attackRay.speed, attackRay.length);
                attackRay.length -= rayLength;
                attackRay.pos += rayLength * attackRay.dir;
                float2 end = start + rayLength * attackRay.dir;
                float2 rayCenter = 0.5f * (start + end);
                
                float2 rayCenterWH = 0.5f * math.abs(rayLength * attackRay.dir);
                
                bool isAlongXAxis = math.abs(attackRay.dir.x) > math.abs(attackRay.dir.y);

                float minHitLength = 9999.9f;
                int hitIndex = -1;
                if (isAlongXAxis)
                {
                    float tan = attackRay.dir.y / math.abs(attackRay.dir.x);
                    float dec = start.x - (int)start.x;
                    float dec_offset = 0.5f - dec;
                    int x_dir = attackRay.dir.x > 0 ? 1 : -1;
                    float y = start.y + dec_offset * x_dir * tan;

                    int x_step_len = (int)math.ceil(math.abs(rayLength * attackRay.dir.x)) + 1;
                    int x_start = 0;
                    
                    
                    while (x_start < x_step_len)
                    {
                        
                        int2 curXY = new int2((int)start.x + x_start * x_dir,(int)y);
                        if (Check(curXY, rayCenter, rayCenterWH, start, end, rayLength,
                            out float rayHitLength ,out int rayHitMonsterIndex))
                        {
                            if (rayHitLength < minHitLength)
                            {
                                minHitLength = rayHitLength;
                                hitIndex = rayHitMonsterIndex;
                            }
                                
                        }

                        float y_offset = y - (int)y;
                        if (y_offset > 0.5f)
                            curXY = curXY + new int2(0, 1);
                        else
                            curXY = curXY + new int2(0, -1);
                        
                        if (Check(curXY, rayCenter, rayCenterWH, start, end, rayLength,
                            out float rayHitLength2 ,out int rayHitMonsterIndex2))
                        {
                            if (rayHitLength2 < minHitLength)
                            {
                                hitIndex = rayHitMonsterIndex2;
                                minHitLength = rayHitLength2;
                            }
                                
                        }
                        
                        x_start++;
                        y += tan;

                    }
                }
                else
                {
                    float tan = attackRay.dir.x / math.abs(attackRay.dir.y);
                    float dec = start.y - (int)start.y;
                    float dec_offset = 0.5f - dec;
                    int y_dir = attackRay.dir.y > 0 ? 1 : -1;
                    float x = start.x + dec_offset * y_dir * tan;

                    int y_step_len = (int)math.ceil(math.abs(rayLength * attackRay.dir.y)) + 1;
                    int y_start = 0;
                    
                    while (y_start < y_step_len)
                    {
                        
                        int2 curXY = new int2((int)start.x + y_start * y_dir,(int)x);
                        if (Check(curXY, rayCenter, rayCenterWH, start, end, rayLength,
                            out float rayHitLength ,out int rayHitMonsterIndex))
                        {
                            if (rayHitLength < minHitLength)
                            {
                                minHitLength = rayHitLength;
                                hitIndex = rayHitMonsterIndex;
                            }  
                        }

                        float x_offset = x - (int)x;
                        if (x_offset > 0.5f)
                            curXY = curXY + new int2(1, 0);
                        else
                            curXY = curXY + new int2(-1, 0);
                        
                        if (Check(curXY, rayCenter, rayCenterWH, start, end, rayLength,
                            out float rayHitLength2 ,out int rayHitMonsterIndex2))
                        {
                            if (rayHitLength2 < minHitLength)
                            {
                                hitIndex = rayHitMonsterIndex2;
                                minHitLength = rayHitLength2;
                            }
                        }
                        
                        y_start++;
                        x += tan;
                    }
                }

                if (hitIndex != -1)
                {
                    attackRay.length = 0;
                    attackRay.hitPos = start + minHitLength * attackRay.dir;
                    attackRay.monsterIndex = hitIndex;
                }

                cAttackRay = attackRay;
            }

            bool Check(int2 xy, float2 rayCenter ,float2 rayCenterWH ,float2 rayStart,float2 rayDir ,float rayLength ,out float rayHitLength ,out int index)
            {
                rayHitLength = 9999.9f;
                index = -1;
                if (xy.x < 0 || xy.x >= mapSize.x || xy.y < 0 || xy.y >= mapSize.y)
                {
                    return false;
                }

                int mapIndex = xy.y * mapSize.x + xy.x;
                MapNode2Struct mapNode2Struct = mapBlock[mapIndex];
                bool ifHit = false;
                for (int i = 0; i < mapNode2Struct.count; i++)
                {
                    EntitieInfo entitieInfo = mapNodeEs[xy.y * (mapSize.x * zCount) + xy.x * zCount + i];
                    if(!entitieInfo.isAlive)
                        continue;
                    if (RayHitCircular(rayCenter ,rayCenterWH ,rayStart ,rayDir ,rayLength ,entitieInfo.position,entitieInfo.radius ,out float curHitLen))
                    {
                        ifHit = true;
                        index = entitieInfo.index;
                        if (curHitLen < rayHitLength)
                        {
                            rayHitLength = curHitLen;
                        }
                    }
                }
                return ifHit;
            }

            bool RayHitCircular(float2 rayCenter ,float2 rayCenterWH ,float2 rayStart,float2 rayDir ,float rayLength ,float2 circularPos ,float circularRadius ,out float rayHitLength)
            {
                rayHitLength = 0;
                if (!CheckCollision(rayCenter, rayCenterWH, circularPos, circularRadius))
                    return false;
                float m = rayStart.x - circularPos.x;
                float n = rayStart.y - circularPos.y;
                float b = 2 * (rayDir.x * m + rayDir.y * n);
                float c = m * m + n * n - circularRadius * circularRadius;
                float d = b * b - 4 * c;
                if (d < 0)
                    return false;
                d = math.sqrt(d);
                float t = 0.5f * (-b - d);
                if(t < 0)
                    t = 0.5f * (-b + d);
                if (t < 0 || t < rayLength)
                    return false;
                rayHitLength = t;
                return true;
            }
            
            bool CheckCollision(float2 selfPos ,float2 selfWH ,float2 anotherPos ,float anotherRadius) 
            {
                bool collisionX = Mathf.Abs(anotherPos.x - selfPos.x) < selfWH.x + anotherRadius;
                bool collisionY = Mathf.Abs(anotherPos.y - selfPos.y) < selfWH.y + anotherRadius;
                return collisionX && collisionY;
            } 
        }

        protected override void OnUpdate()
        {
            NativeArray<MonsterBeHit> monsterBeHits = MonsterBeHits;
            m_clearJob.monsterBeHits = monsterBeHits;
            JobHandle handle = m_clearJob.Schedule(monsterBeHits.Length, 1);  
            handle.Complete();
            
            int dataCount = m_Group.CalculateEntityCount();
            if (dataCount > MonsterBeHits.Length)
            {
                ResizeCapacity(dataCount);
            }
            
            int2 mapSize = Setting.MapSize;
            float deltaTime = Time.DeltaTime;
            NativeArray<EntitieInfo> mapNodeEs = SPhysicsJob2System.MapNodeEs;
            NativeArray<MapNode2Struct> mapBlock = SPhysicsJob2System.MapBlock;
            int zCount = SPhysicsJob2System.MapNodeMaxContainEntitieCount;

            m_hitJob.deltaTime = deltaTime;
            m_hitJob.mapSize = mapSize;
            m_hitJob.mapBlock = mapBlock;
            m_hitJob.mapNodeEs = mapNodeEs;
            m_hitJob.zCount = zCount;
            Dependency = m_hitJob.ScheduleParallel(m_RayGroup, Dependency);
            Dependency.Complete();

            Entities
                .ForEach((in CAttackRay attackRay) =>
                {
                    if (attackRay.monsterIndex >= 0)
                    {
                        MonsterBeHit monsterBeHit;
                        monsterBeHit.damage = attackRay.damage;
                        monsterBeHit.hitPos = attackRay.hitPos;
                        monsterBeHits[attackRay.monsterIndex] = monsterBeHit;
                    }
                }).Schedule();

        }
        
        void ResizeCapacity(int needCount)
        {
            MonsterBeHits.Dispose();
            MonsterBeHits = new NativeArray<MonsterBeHit>(needCount, Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}