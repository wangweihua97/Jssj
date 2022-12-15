using Game.Camera;
using Unity.Mathematics;

namespace Game.GlobalSetting
{
    public static class Setting
    {
        public readonly static int2 MapSize = new int2(240,240);
        public readonly static int ObstacleNumber = 240;
        public readonly static int RandomObstacleDeep = 10;
        public readonly static int2 ObstacleRange = new int2(40,60);

        public static bool IsOpenShadow
        {
            get
            {
                return _isOpenShadow;
            }
            set
            {
                _isOpenShadow = value;
                SetShadow(value);
            }
        }
        
        public static bool IsOpenRimLight
        {
            get
            {
                return _isOpenRimLight;
            }
            set
            {
                _isOpenRimLight = value;
                SetRimLight(value);
            }
        }
        
        public static bool IsOpenColorGlut
        {
            get
            {
                return _isOpenColorGlut;
            }
            set
            {
                _isOpenColorGlut = value;
                SetColorGlut(value);
            }
        }
        
        

        private static bool _isOpenShadow = false;
        private static bool _isOpenRimLight = false;
        private static bool _isOpenColorGlut = false;
        static void SetShadow(bool isOPen)
        {
            if (isOPen)
            {
                foreach (var gameObjectLod in Monsters.Instance.allMonsters)
                {
                    gameObjectLod.EnableKey("_IS_OPEN_SHADOW_ON");
                }
                foreach (var mat in Obstacles.Instance.Rocks)
                {
                    mat.EnableKey("_IS_OPEN_SHADOW_ON");
                }
                foreach (var mat in Obstacles.Instance.Trees)
                {
                    mat.EnableKey("_IS_OPEN_SHADOW_ON");
                }
                Planes.Instance.PlanMaterial.EnableKeyword("_IS_OPEN_SHADOW_ON");
                foreach (var mat in Towers.Instance.TowersGo)
                {
                    mat.EnableKey("_IS_OPEN_SHADOW_ON");
                }
            }
            else
            {
                foreach (var gameObjectLod in Monsters.Instance.allMonsters)
                {
                    gameObjectLod.DisEnableKey("_IS_OPEN_SHADOW_ON");
                }
                foreach (var mat in Obstacles.Instance.Rocks)
                {
                    mat.DisEnableKey("_IS_OPEN_SHADOW_ON");
                }
                foreach (var mat in Obstacles.Instance.Trees)
                {
                    mat.DisEnableKey("_IS_OPEN_SHADOW_ON");
                }
                Planes.Instance.PlanMaterial.DisableKeyword("_IS_OPEN_SHADOW_ON");
                foreach (var mat in Towers.Instance.TowersGo)
                {
                    mat.DisEnableKey("_IS_OPEN_SHADOW_ON");
                }
            }
        }
        
        
        static void SetRimLight(bool isOPen)
        {
            if (isOPen)
            {
                foreach (var gameObjectLod in Monsters.Instance.allMonsters)
                {
                    gameObjectLod.EnableKey("_IS_RimLight_ON");
                }
                foreach (var mat in Obstacles.Instance.Rocks)
                {
                    mat.EnableKey("_IS_RimLight_ON");
                }
                foreach (var mat in Obstacles.Instance.Trees)
                {
                    mat.EnableKey("_IS_RimLight_ON");
                }
                foreach (var mat in Towers.Instance.TowersGo)
                {
                    mat.EnableKey("_IS_RimLight_ON");
                }
            }
            else
            {
                foreach (var gameObjectLod in Monsters.Instance.allMonsters)
                {
                    gameObjectLod.DisEnableKey("_IS_RimLight_ON");
                }
                foreach (var mat in Obstacles.Instance.Rocks)
                {
                    mat.DisEnableKey("_IS_RimLight_ON");
                }
                foreach (var mat in Obstacles.Instance.Trees)
                {
                    mat.DisEnableKey("_IS_RimLight_ON");
                }
                foreach (var mat in Towers.Instance.TowersGo)
                {
                    mat.DisEnableKey("_IS_RimLight_ON");
                }
            }
        }

        static void SetColorGlut(bool isOPen)
        {
            MainCamera.Instance.CameraData.renderPostProcessing = isOPen;
        }
    }
}