using System;
using UnityEngine;

namespace Game.Camera
{
    public class CameraMove : MonoBehaviour
    {
        public float MoveSpeed = 0.01f;
        
        private Touch oldTouch1; 

        private Touch oldTouch2;

        private Vector3 oldMousePosition;

        private void Awake()
        {
#if UNITY_ANDROID && ! UNITY_EDITOR
            Application.targetFrameRate=60;  
#endif
        }

        void Update()
        {
            if (Input.touchCount <= 0)
            {
                return;
            }
            
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                var deltaposition = Input.GetTouch(0).deltaPosition;
                float move = MainCamera.Instance.Camera.orthographicSize * MoveSpeed * Time.deltaTime;
                transform.localPosition += new Vector3(-deltaposition.x * move, 0, -deltaposition.y * move);
            }

            Touch newTouch1 = Input.GetTouch(0);
            Touch newTouch2 = Input.GetTouch(1);

            if (newTouch2.phase == TouchPhase.Began)
            {
                oldTouch2 = newTouch2;
                oldTouch1 = newTouch1;
                return;
            }

            float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
            float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);

            float offset = newDistance - oldDistance;

            float height = MainCamera.Instance.Camera.orthographicSize - offset * 0.01f;


            if (height > 1.0f && height < 20.0f)
            {
                MainCamera.Instance.Camera.orthographicSize = height;
            }
            
            oldTouch1 = newTouch1;
            oldTouch2 = newTouch2;
        }
    }
    
}