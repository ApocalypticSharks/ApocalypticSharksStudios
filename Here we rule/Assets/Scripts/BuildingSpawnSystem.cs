using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public partial class BuildingSpawnSystem : SystemBase
{
    protected override void OnCreate()
    {
    }
    protected override void OnUpdate()
    {
    }
    public void BuildEntity(Vector3 position, Quaternion rotation)
    {
        BuildingToSpawn entityToBuild = SystemAPI.GetSingleton<BuildingToSpawn>();
        Entity spawned = EntityManager.Instantiate(entityToBuild.value);
        EntityManager.SetComponentData(spawned, new LocalTransform
        {
            Position = position,
            Rotation = rotation,
            Scale = 1f
        });
    }
}
