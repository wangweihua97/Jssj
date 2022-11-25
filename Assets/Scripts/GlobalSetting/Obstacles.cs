using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GlobalSetting
{
    public class Obstacles : MonoBehaviour
    {
        public static Obstacles Instance;
        public List<GameObject> Trees;
        public List<Material> TreesMaterial;
        [HideInInspector]
        public List<Mesh> TreesMesh;
        [HideInInspector]
        public List<Matrix4x4> TreesSelfMat;
        
        public List<GameObject> Rocks;
        public List<Material> RocksMaterial;
        [HideInInspector]
        public List<Mesh> RocksMesh;
        [HideInInspector]
        public List<Matrix4x4> RocksSelfMat;
        

        private void Awake()
        {
            Instance = this;
            
            TreesMesh = new List<Mesh>();
            TreesSelfMat = new List<Matrix4x4>();
            foreach (var tree in Trees)
            {
                Mesh mesh = tree.GetComponent<MeshFilter>().sharedMesh;
                TreesMesh.Add(CombineSubMesh(tree ,mesh));
                TreesSelfMat.Add(GetSelfMat(tree.transform));
            }
            
            RocksMesh = new List<Mesh>();
            RocksSelfMat = new List<Matrix4x4>();
            foreach (var rock in Rocks)
            {
                Mesh mesh = rock.GetComponent<MeshFilter>().sharedMesh;
                RocksMesh.Add(CombineSubMesh(rock ,mesh));
                RocksSelfMat.Add(GetSelfMat(rock.transform));
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