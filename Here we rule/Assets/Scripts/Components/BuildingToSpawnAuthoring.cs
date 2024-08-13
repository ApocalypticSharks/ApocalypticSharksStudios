using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct BuildingToSpawn : IComponentData
{
    public Entity value;
}

public class BuildingToSpawnAuthoring : MonoBehaviour
{
    public GameObject buildingToPalce;

    public class Baker : Baker<BuildingToSpawnAuthoring>
    {
        public override void Bake(BuildingToSpawnAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new BuildingToSpawn { 
                value = GetEntity(authoring.buildingToPalce, TransformUsageFlags.Dynamic)
            });
        }
    }
}
