using Game.Map;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class CPositionSystem : SystemBase
    {
        protected override void OnCreate()
        {
        }

        protected override void OnUpdate()
        {
            Entities
                .WithName("CPositionSystem")
                .ForEach((ref CMove move ,in CPosition position) =>
                {
                    int2 xy = new int2((int)position.position.x ,(int)position.position.z);
                    if (MapService.FlowFieldMap.CanWalk(xy))
                    {
                        float2 dir = MapService.FlowFieldMap.map[xy.y * FlowFieldMap.Size.x + xy.x].Dir;
                        move.moveDir =math.normalize(0.8f * move.moveDir + 0.2f * new float3(dir.x ,0 ,dir.y));
                    }
                    else
                    {
                        float2 dir;
                        float x = position.position.x % 1.0f;
                        x = x < 0 ? x + 1.0f : x;
                        float y = position.position.y % 1.0f;
                        y = y < 0 ? y + 1.0f : y;
                        if (x <= 0.5f)
                        {
                            if (y <= 0.5f)
                            {
                                dir = new float2(-0.7071f,-0.7071f);
                            }
                            else
                            {
                                dir = new float2(-0.7071f,0.7071f);
                            }
                        }
                        else
                        {
                            if (y <= 0.5f)
                            {
                                dir = new float2(0.7071f,-0.7071f);
                            }
                            else
                            {
                                dir = new float2(0.7071f,0.7071f);
                            }
                        }
                        move.moveDir = new float3(dir.x ,0 ,dir.y);
                    }
                    
                }).ScheduleParallel();
        }
    }
}