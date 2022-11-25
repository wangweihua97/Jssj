using System.Collections.Generic;
using UnityEngine;

namespace Game.GlobalSetting
{
    public class Towers: MonoBehaviour
    {
        public static Towers Instance;
        public List<GameObject> TowersGo;
        public List<Material> TowersMaterial;
        [HideInInspector]
        public List<Mesh> TowersMesh;
        [HideInInspector]
        public List<Matrix4x4> TowersSelfMat;
        

        private void Awake()
        {
            Instance = this;
            
            TowersMesh = new List<Mesh>();
            TowersSelfMat = new List<Matrix4x4>();
            foreach (var tree in TowersGo)
            {
                Mesh mesh = tree.GetComponent<MeshFilter>().sharedMesh;
                TowersMesh.Add(CombineSubMesh(tree ,mesh));
                TowersSelfMat.Add(GetSelfMat(tree.transform));
            }
        }
        
        Mesh CombineSubMesh(GameObject go ,Mesh mesh)
        {
            CombineInstance[] combines = new CombineInstance[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                combines[i].mesh= mesh;
                combines[i].transform = go.transform.localToWorldMatrix;
                combines[i].subMeshIndex = i;
            }
            Mesh new_mesh = new Mesh();
            new_mesh.CombineMeshes(combines, true,true);
            return new_mesh;
        }

        Matrix4x4 GetSelfMat(Transform tf)
        {
            return Matrix4x4.Rotate(tf.localRotation) * Matrix4x4.Scale(tf.localScale);
        }
    }
}