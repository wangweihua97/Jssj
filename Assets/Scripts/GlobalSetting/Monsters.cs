using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game.Prefabs;
using Game.VAT;
using UnityEngine;

namespace Game.GlobalSetting
{
    public class Monsters : MonoBehaviour
    {
        public static Monsters Instance;
        public List<GameObjectLod> allMonsters;
        public List<VAT_Info> allMonsterVAT_Info;
        public void Awake()
        {
            Instance = this;
            foreach (var monster in allMonsters)
            {
                monster.Init();
                /*monster.Lod0Material.SetMatrix("_Local2WorldMat" ,monster.Lod0Mat);
                monster.Lod1Material.SetMatrix("_Local2WorldMat" ,monster.Lod1Mat);
                monster.Lod2Material.SetMatrix("_Local2WorldMat" ,monster.Lod2Mat);*/
            }
            
        }
    }
}