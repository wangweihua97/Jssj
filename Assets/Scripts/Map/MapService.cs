using Core;

namespace Game.Map
{
    public class MapService : Service
    {
        public static FlowFieldMap FlowFieldMap;
        
        public override void Init(Context context)
        {
            base.Init(context);
        }

        public void GenerateMap()
        {
            FlowFieldMap = AutoGenerate.Generate();
            FlowFieldMap.RefreshFlowField(FlowFieldMap.center);
        }
    }
}