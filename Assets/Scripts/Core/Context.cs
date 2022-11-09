using System;
using System.Collections.Generic;

namespace Core
{
    public enum ContextUpdateMoment
    {
        None = 0,
        Update = 1,
        Render = 2,
        LateUpdate = 3,
        FixedUpdate = 4,
        
    }
    public class Context
    {
        private Dictionary<Type, Service> m_services;

        private List<(Service ,int)>[] m_updateQueue;

        public Context()
        {
            m_services = new Dictionary<Type, Service>();
            InitUpdateQueue();
        }

        public T AddService<T>(int priority = 1) where T:Service ,new()
        {
            T t = new T();
            t.Init(this);
            m_services.Add(typeof(T) ,t);
            if(t.ContextUpdateMoment != ContextUpdateMoment.None)
                InsertUpdateQueue(ContextUpdateMomentToIndex(t.ContextUpdateMoment), priority, t);
            return t;
        }
        
        public void StartService<T>()
        {
            m_services[typeof(T)].Start();
        }
        
        public T GetService<T>() where T : Service
        {
            return m_services[typeof(T)] as T;
        }
        
        public void StopService<T>()
        {
            m_services[typeof(T)].Stop();
        }

        public void Update(ContextUpdateMoment updateMoment)
        {
            foreach (var service in m_updateQueue[ContextUpdateMomentToIndex(updateMoment)])
            {
                if(service.Item1.ServiceState == ServiceState.Running)
                    service.Item1.Update();
            }
        }

        void InitUpdateQueue()
        {
            var queue = Enum.GetValues(typeof(ContextUpdateMoment));
            m_updateQueue = new List<(Service ,int)>[queue.Length];
            for (int i = 0; i < queue.Length; i++)
            {
                m_updateQueue[i] = new List<(Service ,int)>();
            }
        }

        int ContextUpdateMomentToIndex(ContextUpdateMoment updateMoment)
        {
            return (int) updateMoment - 1;
        }

        void InsertUpdateQueue(int queueIndex, int priority, Service service)
        {
            int insertPos = m_updateQueue[queueIndex].Count;
            for (int i = 0; i < m_updateQueue[queueIndex].Count; i++)
            {
                if (priority > m_updateQueue[queueIndex][i].Item2)
                {
                    insertPos = i;
                    break;
                }
            }
            m_updateQueue[queueIndex].Insert(insertPos ,(service ,priority));
        }
    }
}