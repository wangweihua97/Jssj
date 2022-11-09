namespace Core
{
    public enum ServiceState
    {
        None,
        Running,
        Stop
    }
    public class Service
    {
        public virtual ContextUpdateMoment ContextUpdateMoment
        {
            get
            {
                return ContextUpdateMoment.None;
            }
        }

        public ServiceState ServiceState
        {
            get
            {
                return m_ServiceState;
            }
        }
        protected Context m_context;
        protected ServiceState m_ServiceState = ServiceState.None;
        
        public virtual void Init(Context context)
        {
            m_context = context;
        }

        public virtual void Update()
        {
            ;
        }

        public virtual void Start()
        {
            m_ServiceState = ServiceState.Running;
        }
        
        public virtual void Stop()
        {
            m_ServiceState = ServiceState.Stop;
        }
    }
}