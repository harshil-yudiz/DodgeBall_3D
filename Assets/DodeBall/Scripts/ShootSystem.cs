using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct ShootSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        foreach (RefRW<NetcodePlayerInput> netcodePlayerInput in SystemAPI.Query<RefRW<NetcodePlayerInput>>().WithAll<Simulate>())
        {
            if (networkTime.IsFirstPredictionTick) 
            { 
                if (netcodePlayerInput.ValueRO.shoot.IsSet)
                {
                    Debug.Log("Shoot is True");
                }
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
