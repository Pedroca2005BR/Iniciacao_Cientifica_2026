using System.Collections.Generic;
using UnityEngine;

namespace Pedroca2005BR.EquipmentSystem
{
    [CreateAssetMenu(fileName = "SpawnObjects", menuName = "Scriptable Objects/Special Effects/Spawn Objects")]
    public class SpawnObjects : SpecialEffectsBase
    {
        [Header("Spawn Settings")]
        public GameObject objectToSpawn;
        public Vector3 spawnPoint;

        GameObject spawnedObject;

        public override void ActivateEffect()
        {
            spawnedObject = Instantiate(objectToSpawn, spawnPoint, Quaternion.identity);
        }

        public override void DeactivateEffect()
        {
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
            }
        }
    }
}