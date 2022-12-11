using UnityEngine;

namespace Game.GlobalSetting
{
    public class HpPlane : MonoBehaviour
    {
        public static HpPlane Instance;
        public Mesh HpPlaneMesh;
        public Material MonsterHpPlaneMaterial;
        
        public Material TowerHpPlaneMaterial;

        private void Awake()
        {
            Instance = this;
        }
    }
}