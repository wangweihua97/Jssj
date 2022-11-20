using Unity.Entities;

namespace Game.ECS
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SObstaclesRenderSystem : SystemBase
    {
        private const int TREE_AMOUNT = 2;
        private const int ROCK_AMOUNT = 3;
        
        
        
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        public void Init()
        {
            
        }

        protected override void OnUpdate()
        {
            ;
        }

        

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}