using Game.ECS;
using Game.GlobalSetting;
using Unity.Mathematics;
using UnityEngine;

public class CreatTowerView : MonoBehaviour
{
    void Update () {
        if (Input.GetMouseButton(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var rayPos = ray.origin;
            var rayDir = ray.direction;
            float h = rayPos.y;
            float x = -rayDir.x / rayDir.y * h;
            float z = -rayDir.z / rayDir.y * h;
            float2 hitPos = new float2(rayPos.x + x ,rayPos.z + z);
            int2 hitXY = new int2((int) hitPos.x, (int) hitPos.y);
            if (SPhysicsJob2System.MapBlock[hitXY.y * Setting.MapSize.x + hitXY.x].canWalk)
            {
                STowerSpawnerSystem.AddList.Add((hitXY , 0));
            }
        }
    }

}