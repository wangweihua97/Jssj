using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Game.ECS
{
    public class MonsterEntity: MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public static Entity Prefab;
        public GameObject prefabGameObject;

        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            Entity prefabEntity = conversionSystem.GetPrimaryEntity(prefabGameObject);
            Prefab = prefabEntity;
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(prefabGameObject);
        }
    }
}