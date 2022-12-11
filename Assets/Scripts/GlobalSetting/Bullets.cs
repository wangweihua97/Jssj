using System.Collections.Generic;
using UnityEngine;

namespace Game.GlobalSetting
{
    public class Bullets : MonoBehaviour
    {
        public static Bullets Instance;
        public List<GameObject> BulletsGo;
        
        public List<Material> BulletsMaterial;
        
        [HideInInspector]
        public List<Mesh> BulletsMesh;
        [HideInInspector]
        public List<Matrix4x4> BulletsSelfMat;

        private void Awake()
        {
            Instance = this;
            
            BulletsMesh = new List<Mesh>();
            BulletsSelfMat = new List<Matrix4x4>();
            foreach (var tree in BulletsGo)
            {
                Mesh mesh = tree.GetComponent<MeshFilter>().sharedMesh;
                BulletsMesh.Add(CombineSubMesh(tree ,mesh));
                BulletsSelfMat.Add(GetSelfMat(tree.transform));
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
            return Matrix4x4.Translate(tf.localPosition) * Matrix4x4.Rotate(tf.localRotation) * Matrix4x4.Scale(tf.localScale);
        }
    }
}