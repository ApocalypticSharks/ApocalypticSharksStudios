using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct Health : IComponentData
{
    public float value;
    public float maxValue;
}

public class HealthAuthoring : MonoBehaviour
{
    public float value;
    public float maxValue;

    public class Baker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Health
            {
                value = authoring.maxValue,
                maxValue = authoring.maxValue
            });
        }
    }
}

