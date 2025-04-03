using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct TestNetCodeEntitiesServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<SimpleRpc> simpleRpc, RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest, Entity entity) in SystemAPI.Query<RefRO<SimpleRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())                   
        {
            // Log the received value

            // Handle the RPC logic based on the received value
            // Add your server-side logic here

            Debug.Log($"Server received SimpleRpc with value: {simpleRpc.ValueRO.value} :: {receiveRpcCommandRequest.ValueRO.SourceConnection}");
            // Remove the request component to indicate that we've handled this RPC
            entityCommandBuffer.DestroyEntity(entity);            
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
