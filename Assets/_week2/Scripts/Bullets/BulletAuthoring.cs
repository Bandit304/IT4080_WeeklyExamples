using Unity.Entities;
using UnityEngine;
    
namespace _week2.Scripts.Bullets
{
    /// <summary>
    /// Flag component to mark an entity as a Bullet.
    /// </summary>
    public struct Bullet : IComponentData
    {
        public float timer;
    }

    /// <summary>
    /// The authoring component for the Bullet.
    /// </summary>
    [DisallowMultipleComponent]
    public class BulletAuthoring : MonoBehaviour
    {
        class Baker : Baker<BulletAuthoring>
        {
            public override void Bake(BulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Bullet>(entity, new Bullet
                {
                    timer = 5f
                });
            }
        }
    }
}
