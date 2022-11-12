using System;
using Game.ECS;
using Game.Map;
using UnityEngine;

namespace Core
{
    public class GameStage : MonoBehaviour
    {
        public static Context GameContext;
        public static GameWorld GameWorld;
        public GameObject Plane;

        private void Awake()
        {
            GameContext = new Context();
            GameContext.AddService<MapService>();
            
            GameWorld = new GameWorld();
            GameWorld.Plane = Plane;
            GameWorld.Init();
            GameWorld.CreatMonster();
        }

        private void Start()
        {
            
        }
    }
}