using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class MainCamera : MonoBehaviour
    {
        public static MainCamera Instance;
        public float Width;
        public float Height;
        public UnityEngine.Camera Camera;
        public float2 Center;

        public float min_x;
        public float min_y;
        public float max_x;
        public float max_y;

        private Vector3 m_left_top;
        private Vector3 m_left_buttom;
        private Vector3 m_right_top;
        private Vector3 m_right_buttom;

        private float2 m_left_top_offset;
        private float2 m_left_buttom_offset;
        private float2 m_right_top_offset;
        private float2 m_right_buttom_offset;
        
        private float2 m_center_offset;

        private Vector3 m_last_angle;
        private float m_last_height;
        private float m_last_posY;
        private void Awake()
        {
            Instance = this;
            Camera = GetComponent<UnityEngine.Camera>();
            
            ChangeSize();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float2 GetOffset(ref Vector3 dir ,Vector3 pos ,ref float2 selfPos)
        {
            float h = pos.y;
            float x = -dir.x / dir.y * h;
            float z = -dir.z / dir.y * h;
            return new float2(pos.x + x ,pos.z + z) - selfPos;
        }

        private void Update()
        {
            if (Camera.orthographicSize != m_last_height || !transform.localEulerAngles.Equals(m_last_angle) || m_last_posY != transform.position.y)
            {
                ChangeSize();
            }
            ChangeCenter();
        }

        void ChangeSize()
        {
            Height = Camera.orthographicSize;
            Width = Height * Camera.aspect;
            
            m_last_height = Height;
            m_last_angle = transform.localEulerAngles;
            m_last_posY = transform.position.y;

            float2 selfPos = new float2(transform.position.x, transform.position.z);
            
            Vector3 dir = transform.forward;
            m_center_offset = GetOffset(ref dir,transform.position, ref selfPos);
            
            m_left_top = new Vector3( -Width , Height,0);
            m_left_top = transform.localToWorldMatrix * new Vector4(m_left_top.x ,m_left_top.y ,m_left_top.z ,1);
            m_left_top_offset = GetOffset(ref dir,m_left_top, ref selfPos);
            
            m_left_buttom = new Vector3( -Width , -Height,0);
            m_left_buttom = transform.localToWorldMatrix * new Vector4(m_left_buttom.x ,m_left_buttom.y ,m_left_buttom.z ,1);
            m_left_buttom_offset = GetOffset(ref dir,m_left_buttom, ref selfPos);
            
            m_right_top = new Vector3( Width , Height,0);
            m_right_top = transform.localToWorldMatrix * new Vector4(m_right_top.x ,m_right_top.y ,m_right_top.z ,1);
            m_right_top_offset = GetOffset(ref dir,m_right_top, ref selfPos);
            
            m_right_buttom = new Vector3( Width , -Height,0);
            m_right_buttom = transform.localToWorldMatrix * new Vector4(m_right_buttom.x ,m_right_buttom.y ,m_right_buttom.z ,1);
            m_right_buttom_offset = GetOffset(ref dir,m_right_buttom, ref selfPos);
            
        }

        void ChangeCenter()
        {
            float2 selfPos = new float2(transform.position.x, transform.position.z);
            Center = selfPos + m_center_offset;
            float2 left_top = selfPos + m_left_top_offset;
            float2 left_buttom = selfPos + m_left_buttom_offset;
            float2 right_top = selfPos + m_right_top_offset;
            float2 right_buttom = selfPos + m_right_buttom_offset;

            min_x = math.min(left_top.x , left_buttom.x);
            max_x = math.max(right_top.x , right_buttom.x);
            min_y = math.min(left_buttom.y , right_buttom.y);
            max_y = math.max(left_top.y , right_top.y);
        }
    }
}