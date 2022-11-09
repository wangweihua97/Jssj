using System;
using Entitas;
using Game.Map;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
namespace TestEcs
{
    public class World : MonoBehaviour
    {
        public GameObject Plane;
        private void Start()
        {
            
            /*FlowFieldMap fieldMap = new FlowFieldMap();
            fieldMap.RefreshFlowField(new int2(100,100));
            ;*/
            FlowFieldMap fieldMap = AutoGenerate.Generate();
            fieldMap.RefreshFlowField(fieldMap.center);
            for (int i = 0; i < FlowFieldMap.Size.x; i++)
            {
                for (int j = 0; j < FlowFieldMap.Size.y; j++)
                {
                    int2 pos = new int2(i ,j);
                    var  cell = fieldMap.map[j * FlowFieldMap.Size.x + i];
                    GameObject p = GameObject.Instantiate(Plane);
                    p.transform.position = new Vector3(pos.x ,0,pos.y);
                    Plane plane = p.GetComponent<Plane>();
                    if (cell.Dir.Equals(half2.zero))
                    {
                        plane.tf.gameObject.SetActive(false);
                    }
                    else
                    {
                        //plane.SetText(cell.Dir.x + "\n" + cell.Dir.y);
                        plane.tf.localEulerAngles = new Vector3(0,-Mathf.Rad2Deg * math.atan2(cell.Dir.y,cell.Dir.x),0);
                    }
                }
            }
            
        }
    }
}