using System;
using UnityEngine;

namespace Camera
{
    public class MainCamera : MonoBehaviour
    {
        public static MainCamera Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}