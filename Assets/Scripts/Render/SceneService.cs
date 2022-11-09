using Core;

namespace Game.Render
{
    public class SceneService : Service
    {
        public override ContextUpdateMoment ContextUpdateMoment => ContextUpdateMoment.Render;
        public override void Init(Context context)
        {
            base.Init(context);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }
    }
}