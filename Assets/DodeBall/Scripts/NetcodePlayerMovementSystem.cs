using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct NetcodePlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach((RefRW<NetcodePlayerInput> netcodePlayerInput, RefRW<LocalTransform> LocalTransform) in SystemAPI.Query<RefRW<NetcodePlayerInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            float moveSpeed = 10f;
            float3 moveVector = new float3(netcodePlayerInput.ValueRO.inputVector.x,0,netcodePlayerInput.ValueRO.inputVector.y);
            LocalTransform.ValueRW.Position += moveVector * moveSpeed * SystemAPI.Time.DeltaTime;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
