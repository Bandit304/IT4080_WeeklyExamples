using Unity.Entities;
using UnityEngine;

namespace IT4080C
{
    /// <summary>
    /// Flag component to mark an entity as a Bullet.
    /// </summary>
    public struct Health : IComponentData
    {
        public float currentHealth;
    }

    /// <summary>
    /// The authoring component for the Bullet.
    /// </summary>
    [DisallowMultipleComponent]
    public class HealthAuthoring : MonoBehaviour
    {
        class Baker : Baker<HealthAuthoring>
        {
            public override void Bake(HealthAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Health>(entity, new Health
                {
                    currentHealth = 100f
                });
            }
        }
    }
}
