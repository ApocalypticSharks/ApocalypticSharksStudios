using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct WorkingPlace : IComponentData
{
    public Entity value;
}

public class WorkingPlaceAuthoring : MonoBehaviour
{
    public GameObject defaultWorkingPlace;
    public class Baker : Baker<WorkingPlaceAuthoring>
    {
        public override void Bake(WorkingPlaceAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new WorkingPlace
            {
                value = GetEntity(authoring.defaultWorkingPlace, TransformUsageFlags.Dynamic)
            });
        }
    }
}

