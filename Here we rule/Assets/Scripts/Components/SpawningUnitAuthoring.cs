using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct SpawningUnit : IComponentData
{
    public Entity value;
}

public class SpawningUnitAuthoring : MonoBehaviour
{
    public GameObject unitToSpawn;

    public class Baker : Baker<SpawningUnitAuthoring>
    {
        public override void Bake(SpawningUnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SpawningUnit
            {
                value = GetEntity(authoring.unitToSpawn, TransformUsageFlags.Dynamic)
            });
        }
    }
}

