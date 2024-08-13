using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PeopleSpirit : IComponentData
{
    public float value;
}

public class PeopleSpiritAuthoring : MonoBehaviour
{
    public float defaultPeopleSpirit;

    public class Baker : Baker<PeopleSpiritAuthoring>
    {
        public override void Bake(PeopleSpiritAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new PeopleSpirit
            {
                value = authoring.defaultPeopleSpirit
            });
        }
    }
}
