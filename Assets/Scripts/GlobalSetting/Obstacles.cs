using System;
using System.Collections.Generic;
using Game.Prefabs;
using UnityEngine;

namespace Game.GlobalSetting
{
    public class Obstacles : MonoBehaviour
    {
        public static Obstacles Instance;
        public List<GameObjectLod> Trees;
        
        public List<GameObjectLod> Rocks;
        

        private void Awake()
        {
            Instance = this;
            
            foreach (var tree in Trees)
            {
                tree.Init();
            }
            
            foreach (var rock in Rocks)
            {
                rock.Init();
            }
        }
    }
}