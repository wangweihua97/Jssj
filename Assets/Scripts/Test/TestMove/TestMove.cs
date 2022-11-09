using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Test.TestMove
{
    public class TestMove : MonoBehaviour
    {
        public static List<TestMove> lists;
        public float Speed = 2.0f;
        public float radius = 0.5f;
        public float2 Dir;
        public float2 Pos;
        private void Awake()
        {
            if(lists == null)
                lists = new List<TestMove>();
            lists.Add(this);
            Pos = new float2(transform.position.x ,transform.position.z);
        }

        private void Update()
        {
            float2 pos = new float2(transform.position.x ,transform.position.z);
            float2 newDir = math.normalize(new float2(-pos.x ,-pos.y));
            newDir = math.normalize(0.8f * Dir + 0.2f * newDir);
            float2 oldDir = newDir;
            float newLength = Time.deltaTime * Speed;
            float2 newPos = pos + newDir * newLength;
            foreach (var move in lists)
            {
                if(move == this)
                    continue;
                CollideCircular(radius ,move.Pos ,move.radius ,oldDir ,pos ,
                    ref newPos,ref newDir,ref newLength);
            }
            
            Dir = newDir;
            Pos = newPos;
            transform.position = new Vector3(Pos.x ,0,Pos.y);
        }
        
        void CollideCircular(float selfRadius ,float2 anotherPos ,float anotherRadius,float2 oldDir,float2 pos,
            ref float2 newPos ,ref float2 newDir ,ref float newLength)
        {
            float2 oldPos = pos;
            float oldLength = newLength;
            //float a = dir.x * dir.x + dir.y * dir.y;
            float m = anotherPos.x - oldPos.x;
            float n = anotherPos.y - oldPos.y;
            float b = -2 * (oldDir.x * m + oldDir.y * n);
            float c = m * m + n * n - (selfRadius + anotherRadius) * (selfRadius + anotherRadius);
            //float w = b * b - 4 * a * c;
            float w = b * b - 4 * c;
            if(w < 0)
                return;
            //float t = 0.5f * (-b - math.sqrt(w)) / a;
            float t = 0.5f * (-b - math.sqrt(w));
            
            if(t<-0.1f)
                t = 0.5f * (-b + math.sqrt(w));
            
            //t = 0.5f * (-b + math.sqrt(w)) / a;
            if(t >  oldLength || t < -0.001f)
                return; 
            if(math.dot(oldPos - anotherPos ,oldDir) > 0)
                return;

            if(t > 0)
                t = math.max(0.0f, t - 0.0001f);
            newLength = t;

            newPos = oldPos + oldDir * t;
            
            float2 newN = math.normalize(anotherPos - newPos);
            float2 newT = new float2(newN.y, -newN.x);
            newT = math.dot(newT, oldDir) > 0 ? newT : -newT;
            newDir = newT;
            /*float2 leave = newDir * (newLength - t);
            float2 newN = math.normalize(anotherPos - newPos);
            float2 newT = new float2(newN.y, -newN.x);
            newPos += newT * math.dot(leave, newT);
            newLength = math.length(newPos - oldPos);
            newDir = (newPos - oldPos)/newLength;*/
        }
    }
}