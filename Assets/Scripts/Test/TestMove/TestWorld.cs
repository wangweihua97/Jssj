using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Test.TestMove
{
    public class TestWorld : MonoBehaviour
    {
        public GameObject Go;
        private void Awake()
        {
            for (int i = 0; i < 10; i++)
            {
                float rx = Random.Range(-10.0f, 10.0f);
                float ry = Random.Range(-10.0f, 10.0f);
                var go = GameObject.Instantiate(Go);
                go.transform.position = new Vector3(rx ,0 ,ry);
            }
        }
    }
}