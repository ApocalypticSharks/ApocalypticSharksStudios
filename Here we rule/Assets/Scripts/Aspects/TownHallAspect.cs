using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

readonly partial struct TownHallAspect : IAspect
{
    public readonly Entity Self;
    readonly RefRW<LocalTransform> transform;
    readonly RefRW<Timer> timer;
    readonly RefRW<PeopleSpirit> peopleSpirit;
    readonly RefRW<SpawningUnit> spawningUnit;

    public void SpawnUnit(ref SystemState state)
    {
        Entity spawnedEntity = state.EntityManager.Instantiate(spawningUnit.ValueRO.value);
        Vector2 spawnArea = UnityEngine.Random.insideUnitCircle * 10;
        state.EntityManager.SetComponentData(spawnedEntity, new LocalTransform
            {
                Scale = 1f,
                Position = transform.ValueRO.Position + new float3(spawnArea.x,0,spawnArea.y),
                Rotation = transform.ValueRO.Rotation
            });
    }

    public void TimerTick(ref SystemState state, float deltaTime)
    {
        timer.ValueRW.value = timer.ValueRO.value + deltaTime * peopleSpirit.ValueRO.value;
        if (timer.ValueRO.value >= timer.ValueRO.maxValue)
        {
            SpawnUnit(ref state);
            timer.ValueRW.value = 0;
        }
    }
}
