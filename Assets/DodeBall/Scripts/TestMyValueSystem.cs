using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct TestMyValueSystem : ISystem
{    
    public void OnUpdate(ref SystemState state)
    {
        foreach((RefRW<MyValue> myValue, Entity entity) in SystemAPI.Query<RefRW<MyValue>>().WithEntityAccess())
        {
            //myValue.ValueRW.value++;
            Debug.Log("MyValue: " + myValue.ValueRW.value + "::" + entity + "::" + state.World);
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct TestMyValueServerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<MyValue> myValue in SystemAPI.Query<RefRW<MyValue>>())
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                myValue.ValueRW.value++;
                Debug.Log("MyValue: " + myValue.ValueRW.value);
            }
        }
    }   
}
