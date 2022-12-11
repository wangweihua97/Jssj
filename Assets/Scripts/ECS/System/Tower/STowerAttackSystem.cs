using Game.GlobalSetting;
using Game.Map;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SPhysicsJob2System))]
    public partial class STowerAttackSystem : SystemBase ,ICustomSystem
    {
        private const int MAX_TOWER_AMOUNT = 2000;
        private const float IDLE_TIME = 1.0f;
        public static NativeArray<TowarAttackResult> TowarAttackResults;
        public static int TowarAttackResultsCount;
        EntityQuery m_Group;
        private const float ROTATE_SPEED = 100.0f;
        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(typeof(CTowerState),typeof(CTowerTransform), ComponentType.ReadOnly<CTowerInfo>());
        }

        public void Init()
        {
            TowarAttackResultsCount = 0;
            TowarAttackResults = 
                new NativeArray<TowarAttackResult>(MAX_TOWER_AMOUNT, Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            int2 mapSize = Setting.MapSize;
            float deltaTime = Time.DeltaTime;
            NativeArray<EntitieInfo> mapNodeEs = SPhysicsJob2System.MapNodeEs;
            NativeArray<MapNode2Struct> mapBlock = SPhysicsJob2System.MapBlock;
            int zCount = SPhysicsJob2System.MapNodeMaxContainEntitieCount;
            NativeArray<TowarAttackResult> towarAttackResults = TowarAttackResults;
            
            NativeArray<int> towarAttackResultsCount = new NativeArray<int>(1 ,Allocator.Persistent);
            towarAttackResultsCount[0] = 0;
            Entities
                .WithStoreEntityQueryInField(ref m_Group)
                .ForEach((int entityInQueryIndex, ref CTowerState cTowerState, ref CTowerTransform cTowerTransform,
                    in CTowerInfo cTowerInfo) =>
                {
                    if (cTowerState.curAtkInterval > 0.001f)
                    {
                        cTowerState.curAtkInterval -= deltaTime;
                        return;
                    }

                    if (cTowerState.curIdleTime > 0.001f)
                    {
                        cTowerState.curIdleTime -= deltaTime;
                        cTowerTransform.rotation =
                            math.fmod(cTowerTransform.rotation + ROTATE_SPEED * deltaTime, 360.0f);
                        return;
                    }

                    int range = (int) cTowerInfo.atkRange;
                    float2 atkPos = new float2(0, 0);
                    bool hasAtkAim = false;
                    for (int i = 0; i < range; i++)
                    {
                        int2 left = cTowerTransform.xy + new int2(-i, 0);
                        for (int j = left.y - i; j < left.y + i; j++)
                        {
                            int2 xy = new int2(left.x, j);
                            if (xy.x < 0 || xy.x >= mapSize.x || xy.y < 0 || xy.y >= mapSize.y)
                                continue;
                            int2 dif = xy - cTowerTransform.xy;
                            if (dif.x * dif.x + dif.y * dif.y > cTowerInfo.atkRange * cTowerInfo.atkRange)
                                continue;
                            for (int k = 0; k < mapBlock[xy.y * mapSize.x + xy.x].count; k++)
                            {
                                if (mapNodeEs[xy.y * (mapSize.x * zCount) + xy.x * zCount + k].isAlive)
                                {
                                    atkPos = mapNodeEs[xy.y * (mapSize.x * zCount) + xy.x * zCount + k].position;
                                    hasAtkAim = true;
                                    break;
                                }
                            }

                            if (hasAtkAim)
                                break;
                        }

                        if (hasAtkAim)
                            break;

                        int2 top = cTowerTransform.xy + new int2(0, i);
                        for (int j = top.x - i; j < top.x + i; j++)
                        {
                            int2 xy = new int2(j, top.y);
                            if (xy.x < 0 || xy.x >= mapSize.x || xy.y < 0 || xy.y >= mapSize.y)
                                continue;
                            int2 dif = xy - cTowerTransform.xy;
                            if (dif.x * dif.x + dif.y * dif.y > cTowerInfo.atkRange * cTowerInfo.atkRange)
                                continue;
                            for (int k = 0; k < mapBlock[xy.y * mapSize.x + xy.x].count; k++)
                            {
                                if (mapNodeEs[xy.y * (mapSize.x * zCount) + xy.x * zCount + k].isAlive)
                                {
                                    atkPos = mapNodeEs[xy.y * (mapSize.x * zCount) + xy.x * zCount + k].position;
                                    hasAtkAim = true;
                                    break;
                                }
                            }

                            if (hasAtkAim)
                                break;
                        }

                        if (hasAtkAim)
                            break;

                        int2 right = cTowerTransform.xy + new int2(i, 0);
                        for (int j = right.y - i; j < right.y + i; j++)
                        {
                            int2 xy = new int2(right.x, j);
                            if (xy.x < 0 || xy.x >= mapSize.x || xy.y < 0 || xy.y >= mapSize.y)
                                continue;
                            int2 dif = xy - cTowerTransform.xy;
                            if (dif.x * dif.x + dif.y * dif.y > cTowerInfo.atkRange * cTowerInfo.atkRange)
                                continue;
                            for (int k = 0; k < mapBlock[xy.y * mapSize.x + xy.x].count; k++)
                            {
                                if (mapNodeEs[xy.y * (mapSize.x * zCount) + xy.x * zCount + k].isAlive)
                                {
                                    atkPos = mapNodeEs[xy.y * (mapSize.x * zCount) + xy.x * zCount + k].position;
                                    hasAtkAim = true;
                                    break;
                                }
                            }

                            if (hasAtkAim)
                                break;
                        }

                        if (hasAtkAim)
                            break;

                        int2 buttom = cTowerTransform.xy + new int2(0, -i);
                        for (int j = buttom.x - i; j < buttom.x + i; j++)
                        {
                            int2 xy = new int2(j, buttom.y);
                            if (xy.x < 0 || xy.x >= mapSize.x || xy.y < 0 || xy.y >= mapSize.y)
                                continue;
                            int2 dif = xy - cTowerTransform.xy;
                            if (dif.x * dif.x + dif.y * dif.y > cTowerInfo.atkRange * cTowerInfo.atkRange)
                                continue;
                            for (int k = 0; k < mapBlock[xy.y * mapSize.x + xy.x].count; k++)
                            {
                                if (mapNodeEs[xy.y * (mapSize.x * zCount) + xy.x * zCount + k].isAlive)
                                {
                                    atkPos = mapNodeEs[xy.y * (mapSize.x * zCount) + xy.x * zCount + k].position;
                                    hasAtkAim = true;
                                    break;
                                }
                            }

                            if (hasAtkAim)
                                break;
                        }

                        if (hasAtkAim)
                            break;
                    }

                    if (!hasAtkAim)
                    {
                        cTowerState.curIdleTime = IDLE_TIME;
                        cTowerTransform.rotation =
                            math.fmod(cTowerTransform.rotation + ROTATE_SPEED * deltaTime, 360.0f);
                        return;
                    }

                    cTowerState.curAtkInterval = cTowerInfo.atkInterval;

                    float2 dir = atkPos - new float2(cTowerTransform.xy.x + 0.5f, cTowerTransform.xy.y + 0.5f);
                    cTowerTransform.rotation = -math.atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    TowarAttackResult towarAttackResult;
                    towarAttackResult.damage = cTowerInfo.atkDamage;
                    towarAttackResult.bulletType = cTowerInfo.type;
                    towarAttackResult.dir = math.normalize(dir);
                    towarAttackResult.rotation = -math.atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    towarAttackResult.length = cTowerInfo.atkRange;
                    towarAttackResult.pos = new float2(cTowerTransform.xy.x + 0.5f, cTowerTransform.xy.y + 0.5f)
                                            + towarAttackResult.dir * cTowerInfo.atkPosOffset;
                    towarAttackResult.speed = cTowerInfo.bulletSpeed;

                    towarAttackResults[towarAttackResultsCount[0]] = towarAttackResult;
                    towarAttackResultsCount[0] = towarAttackResultsCount[0] + 1;

                })
                .Schedule();
            Dependency.Complete();

            TowarAttackResultsCount = towarAttackResultsCount[0];
            towarAttackResultsCount.Dispose();
        }
        
        public void OnDestroy(ref SystemState state)
        {
            TowarAttackResults.Dispose();
        }
    }
}