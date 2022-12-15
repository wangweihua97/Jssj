using System.Collections.Generic;
using Game.Prefabs;
using UnityEngine;

namespace Game.GlobalSetting
{
    public class Towers: MonoBehaviour
    {
        public static Towers Instance;
        public List<GameObjectLod> TowersGo;
        public List<GameObjectLod> TowersBaseGo;
        

        private void Awake()
        {
            Instance = this;
            
            foreach (var tree in TowersGo)
            {
                tree.Init();
            }
            
            foreach (var tree in TowersBaseGo)
            {
                tree.Init();
            }
        }
    }
}