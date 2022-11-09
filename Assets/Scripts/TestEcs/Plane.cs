using TMPro;
using UnityEngine;

namespace TestEcs
{
    public class Plane : MonoBehaviour
    {
        public TextMeshPro TextMeshPro;
        public Transform tf;

        public void SetText(string text)
        {
            TextMeshPro.SetText(text);
        }
    }
}