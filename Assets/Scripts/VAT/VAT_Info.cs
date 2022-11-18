using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace Game.VAT
{
    [CreateAssetMenu(fileName ="VAT_Info",menuName ="ScriptableObject/NewTab",order = 1 )]
    public class VAT_Info : ScriptableObject
    {
        public List<string> AnimNames;
        
        public List<Vector2> AnimInfos;

        public int VAT_Size;

        public int RunAnimIndex;
        public int AtkAnimIndex;
        public int DeathAnimIndex;
        
    }
    
    //++++++++++++++++++++++++++++++++++++
//++++++++++++++++++++++++++++编辑器代码
#if UNITY_EDITOR
    [CustomEditor(typeof(VAT_Info))]
    public class VertsAnimationMonoBehaviourEditor : Editor
    {
        VAT_Info vertsAnimation;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("载入动画json信息", GUILayout.Width(200f)))
            {
                vertsAnimation = (VAT_Info) target;
                if (vertsAnimation.AnimInfos != null)
                {
                    vertsAnimation.AnimInfos.Clear();
                }
                else
                {
                    vertsAnimation.AnimInfos = new List<Vector2>(); 
                }

                if (vertsAnimation.AnimNames != null)
                {
                    vertsAnimation.AnimNames.Clear();
                }
                else
                {
                    vertsAnimation.AnimNames = new List<string>();
                }
                string jsonPath = EditorUtility.OpenFilePanel("载入动画json信息", Application.dataPath, "txt");
                string jsonStr= File.ReadAllText(jsonPath);
                JsonData jsonObject = JsonMapper.ToObject(jsonStr);
                vertsAnimation.VAT_Size = (int)jsonObject["size"];
                JsonData info = jsonObject["info"];
                foreach(string key in info.Keys)
                {
                    Vector2 startIndex_count = Vector2.zero;
                    startIndex_count.x = (int) info[key][0] + 0.5f;
                    startIndex_count.y = (int) info[key][1] - 1.0f;
                    vertsAnimation.AnimNames.Add(key);
                    vertsAnimation.AnimInfos.Add(startIndex_count);
                }
                EditorUtility.SetDirty(vertsAnimation);
                
            }
        }
    }
#endif
//------------------------------------
//------------------------------------
}