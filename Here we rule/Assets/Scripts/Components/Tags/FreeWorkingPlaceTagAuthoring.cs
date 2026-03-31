using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct FreeWorkingPlace : IComponentData
{
}

public class FreeWorkingPlaceTagAuthoring : MonoBehaviour
{
    public class Baker : Baker<FreeWorkingPlaceTagAuthoring>
    {
        public override void Bake(FreeWorkingPlaceTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new FreeWorkingPlace());
        }
    }
}

