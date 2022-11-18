using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Camera
{
    public class MainCamera : MonoBehaviour
    {
        public static MainCamera Instance;
        public float Width;
        public float Height;
        public UnityEngine.Camera Camera;
        public float2 Center;
        

        private void Awake()
        {
            Instance = this;
            Camera = GetComponent<UnityEngine.Camera>();
            float m_h = Camera.orthographicSize;
            float m_w = m_h * Camera.aspect;
            
            Width = m_w;
            Height = m_h / Mathf.Abs(math.cos(90.0f - transform.localEulerAngles.x));
        }
        
        

        private void Update()
        {
            Vector3 dir = transform.forward;
            float h = transform.position.y;
            float x = -dir.x / dir.y * h;
            float z = -dir.z / dir.y * h;
            
            Center = new float2(transform.position.x + x ,transform.position.z + z);
            
        }
    }
}