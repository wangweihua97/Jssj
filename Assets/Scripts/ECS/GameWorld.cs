using System.Threading.Tasks;
using Core;
using Game.Map;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.ECS
{
    public class GameWorld
    {
        public GameObject Plane;
        public void Init()
        {
            GameStage.GameContext.GetService<MapService>().GenerateMap();
            for (int i = 0; i < FlowFieldMap.Size.x; i++)
            {
                for (int j = 0; j < FlowFieldMap.Size.y; j++)
                {
                    int2 pos = new int2(i ,j);
                    var  cell = MapService.FlowFieldMap.map[j * FlowFieldMap.Size.x + i];
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
            
            CMoveJob2System cMoveJob2System =
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<CMoveJob2System>();
            cMoveJob2System.Init();
        }
        
        public async void CreatMonster()
        {
            MonsterSpawnerSystem monsterSpawnerSystem =
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<MonsterSpawnerSystem>();

            await Task.Delay(100);
            for (int i = 0; i < 4000; i++)
            {
                int2 random_pos = new int2(UnityEngine.Random.Range(0,FlowFieldMap.Size.x),
                    Random.Range(0,FlowFieldMap.Size.y));
                if (MapService.FlowFieldMap.CanWalk(random_pos))
                {
                    monsterSpawnerSystem.CreatMonster(random_pos);
                }
            }
        }
    }
}