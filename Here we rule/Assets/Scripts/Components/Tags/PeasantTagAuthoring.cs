using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct PeasantTag : IComponentData
{
}

public class PeasantTagAuthoring : MonoBehaviour
{
    public class Baker : Baker<PeasantTagAuthoring>
    {
        public override void Bake(PeasantTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PeasantTag());
        }
    }
}

