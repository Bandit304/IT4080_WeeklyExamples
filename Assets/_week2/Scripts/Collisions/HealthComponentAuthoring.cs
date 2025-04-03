using IT4080C;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[GhostComponent]
public struct HealthComponent : IComponentData
{
    [GhostField] public float CurrentHealth;
    [GhostField] public float MaxHealth;
    [GhostField] public float ownerNetworkID;
    [GhostField] public int kills;
    [GhostField] public int deaths;
}

[DisallowMultipleComponent]
public class HealthComponentAuthoring : MonoBehaviour
{

    class Baker : Baker<HealthComponentAuthoring>
    {
        public override void Bake(HealthComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<HealthComponent>(entity, new HealthComponent
            {
                CurrentHealth = 100f,
                MaxHealth = 100f,
                ownerNetworkID = 999f,
                kills = 0,
                deaths = 0
            });


        }

    }


}