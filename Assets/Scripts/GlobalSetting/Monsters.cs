using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game.VAT;
using UnityEngine;

namespace Game.GlobalSetting
{
    public class Monsters : MonoBehaviour
    {
        public static Monsters Instance;
        public List<GameObject> allMonsters;
        public List<VAT_Info> allMonsterVAT_Info;
        [HideInInspector]
        public List<Mesh> allMeshs;
        [HideInInspector]
        public List<Matrix4x4> selfMats;
        public List<Material> allMats;
        public void Awake()
        {
            Instance = this;
            allMeshs = new List<Mesh>();
            selfMats = new List<Matrix4x4>();
            foreach (var monster in allMonsters)
            {
                selfMats.Add(Matrix4x4.Rotate(monster.transform.localRotation) * Matrix4x4.Scale(monster.transform.localScale));
                
                Mesh mesh = monster.GetComponent<MeshFilter>().sharedMesh;
                CombineInstance[] combines = new CombineInstance[mesh.subMeshCount];
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    combines[i].mesh= mesh;
                    combines[i].transform = monster.transform.localToWorldMatrix;
                    combines[i].subMeshIndex = i;
                }
                Mesh new_mesh = new Mesh();
                new_mesh.CombineMeshes(combines, true,true);
                allMeshs.Add(new_mesh);
            }
            
        }
    }
}