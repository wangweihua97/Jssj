using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Core;
using Game.GlobalSetting;
using Game.Map;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.ECS
{
    public class GameWorld
    {
        public static GameWorld Instance;
        public GameObject Plane;
        public List<object> Systems;
        public void Init()
        {
            Instance = this;
            Systems = new List<object>();
            GameStage.GameContext.GetService<MapService>().GenerateMap();
            for (int i = 0; i < Setting.MapSize.x; i++)
            {
                for (int j = 0; j < Setting.MapSize.y; j++)
                {
                    int2 pos = new int2(i ,j);
                    var  cell = MapService.FlowFieldMap.map[j * Setting.MapSize.x + i];
                    /*GameObject p = GameObject.Instantiate(Plane);
                    p.transform.position = new Vector3(pos.x + 0.5f ,0,pos.y+ 0.5f);
                    TestEcs.Plane plane = p.GetComponent<TestEcs.Plane>();
                    if (cell.Dir.Equals(half2.zero))
                    {
                        plane.tf.gameObject.SetActive(false);
                    }
                    else
                    {
                        //plane.SetText(cell.Dir.x + "\n" + cell.Dir.y);
                        plane.tf.localEulerAngles = new Vector3(0,-Mathf.Rad2Deg * math.atan2(cell.Dir.y,cell.Dir.x),0);
                    }*/
                }
            }

            foreach (var type in  Assembly.GetAssembly(typeof(GameWorld)).GetTypes())
            {
                if (type.IsSubclassOf(typeof(SystemBase)) && typeof(ICustomSystem).IsAssignableFrom(type))
                {
                    MethodInfo GetComponent_Method=this.GetType().GetMethod("AddSystem" ,new Type[]{});
                    MethodInfo MakeGeneric_GetComponent_Method = GetComponent_Method.MakeGenericMethod(type);
                    MakeGeneric_GetComponent_Method.Invoke(this, null);
                }
            }

            foreach (var system in Systems)
            {
                (system as ICustomSystem).Init();
            }
        }

        public void AddSystem<T>() where T:SystemBase ,ICustomSystem
        {
            Systems.Add(World.DefaultGameObjectInjectionWorld.GetExistingSystem<T>());
        }
        
        public async void CreatMonster()
        {
            SMonsterSpawnerSystem monsterSpawnerSystem =
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<SMonsterSpawnerSystem>();
            
            monsterSpawnerSystem.Creat_A_Large_Number_Monsters();
        }
    }
}