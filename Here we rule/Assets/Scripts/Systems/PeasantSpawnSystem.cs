using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct PeasantSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        //state.RequireForUpdate<TownHallAspect>();
    }
    public void OnUpdate(ref SystemState state)
    {
        foreach (var townHall in SystemAPI.Query<TownHallAspect>())
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            townHall.TimerTick(ref state, deltaTime);
        }
    }
}
