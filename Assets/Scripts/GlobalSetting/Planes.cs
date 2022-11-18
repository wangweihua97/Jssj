using System;
using UnityEngine;

namespace Game.GlobalSetting
{
    public class Planes : MonoBehaviour
    {
        public static Planes Instance;
        public GameObject Plan;
        [HideInInspector]
        public Mesh PlanMesh;
        [HideInInspector]
        public Matrix4x4 selfMats;
        public Material PlanMaterial;

        private void Awake()
        {
            Instance = this;
            selfMats = Matrix4x4.Rotate(Plan.transform.localRotation) * Matrix4x4.Scale(Plan.transform.localScale);
            PlanMesh = Plan.GetComponent<MeshFilter>().sharedMesh;
        }
    }
}