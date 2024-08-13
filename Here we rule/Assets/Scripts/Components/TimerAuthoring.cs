using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct Timer : IComponentData
{
    public float value;
    public float maxValue;
}

public class TimerAuthoring : MonoBehaviour
{
    public float value;
    public float maxValue;

    public class Baker : Baker<TimerAuthoring>
    {
        public override void Bake(TimerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Timer
            {
                value = authoring.value,
                maxValue = authoring.maxValue
            });
        }
    }
}

