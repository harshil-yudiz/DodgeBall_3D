using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkId>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach ((RefRO<ReceiveRpcCommandRequest> ReceiveRpcCommandRequest, Entity entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequestRpc>().WithEntityAccess())
        {

            entityCommandBuffer.AddComponent<NetworkStreamInGame>(ReceiveRpcCommandRequest.ValueRO.SourceConnection);
            Debug.Log("Client Connected to Server!");


            Entity playerEntity = entityCommandBuffer.Instantiate(entitiesReferences.playerPrefabEntity);
            entityCommandBuffer.SetComponent(playerEntity, LocalTransform.FromPosition(new float3(UnityEngine.Random.Range(-10f, 10f), 0, 0)));
            
            NetworkId networkId = SystemAPI.GetComponent<NetworkId>(ReceiveRpcCommandRequest.ValueRO.SourceConnection);
            entityCommandBuffer.AddComponent(playerEntity, new GhostOwner
            {
                NetworkId = networkId.Value,
            });

            entityCommandBuffer.AppendToBuffer(ReceiveRpcCommandRequest.ValueRO.SourceConnection, new LinkedEntityGroup
            {
                Value = playerEntity,
            });

            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);        
    }    
}
