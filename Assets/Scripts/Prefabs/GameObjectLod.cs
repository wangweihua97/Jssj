using System;
using Game.Camera;
using UnityEngine;

namespace Game.Prefabs
{
    [Serializable]
    public class GameObjectLod
    {
        public GameObject Lod0GO;
        public Material Lod0Material;
        
        [HideInInspector]
        public Mesh Lod0Mesh;
        [HideInInspector]
        public Matrix4x4 Lod0Mat;
        
        public GameObject Lod1GO;
        public Material Lod1Material;
        
        [HideInInspector]
        public Mesh Lod1Mesh;
        [HideInInspector]
        public Matrix4x4 Lod1Mat;
        
        public GameObject Lod2GO;
        public Material Lod2Material;
        
        [HideInInspector]
        public Mesh Lod2Mesh;
        [HideInInspector]
        public Matrix4x4 Lod2Mat;

        public Mesh GetMesh()
        {
            if (MainCamera.Instance.CurLod == 0)
            {
                return Lod0Mesh;
            }
            else if (MainCamera.Instance.CurLod == 1)
            {
                return Lod1Mesh;
            }
            else
            {
                return Lod2Mesh;
            }
        }

        public void EnableKey(string key)
        {
            Lod0Material.EnableKeyword(key);
            Lod1Material.EnableKeyword(key);
            Lod2Material.EnableKeyword(key);
        }
        
        public void DisEnableKey(string key)
        {
            Lod0Material.DisableKeyword(key);
            Lod1Material.DisableKeyword(key);
            Lod2Material.DisableKeyword(key);
        }

        public Material GetMaterial()
        {
            if (MainCamera.Instance.CurLod == 0)
            {
                return Lod0Material;
            }
            else if (MainCamera.Instance.CurLod == 1)
            {
                return Lod1Material;
            }
            else
            {
                return Lod2Material;
            }
        }

        public Matrix4x4 GetMatrix()
        {
            if (MainCamera.Instance.CurLod == 0)
            {
                return Lod0Mat;
            }
            else if (MainCamera.Instance.CurLod == 1)
            {
                return Lod1Mat;
            }
            else
            {
                return Lod2Mat;
            }
        }

        public void Init()
        {
            Lod0Mesh = CombineSubMesh(Lod0GO ,Lod0GO.GetComponent<MeshFilter>().sharedMesh);
            Lod0Mat = GetSelfMat(Lod0GO.transform);
            
            Lod1Mesh = CombineSubMesh(Lod1GO ,Lod1GO.GetComponent<MeshFilter>().sharedMesh);
            Lod1Mat = GetSelfMat(Lod1GO.transform);
            
            Lod2Mesh = CombineSubMesh(Lod2GO ,Lod2GO.GetComponent<MeshFilter>().sharedMesh);
            Lod2Mat = GetSelfMat(Lod2GO.transform);
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