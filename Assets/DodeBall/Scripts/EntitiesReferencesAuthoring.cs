using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject playerPrefabGameObject;

    public class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                playerPrefabEntity = GetEntity(authoring.playerPrefabGameObject, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity playerPrefabEntity;
}
