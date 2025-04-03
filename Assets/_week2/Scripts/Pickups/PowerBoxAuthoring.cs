using Unity.Entities;
using UnityEngine;

namespace IT4080C
{
    public struct PowerBox : IComponentData
    {
        public float timer;
        public byte hasBeenPickedUp;
        public bool touchable;
        public bool destroy;
        public float damageMult;
    }

    [DisallowMultipleComponent]
    public class PowerBoxAuthoring : MonoBehaviour
    {


        class SpawnerBaker : Baker<PowerBoxAuthoring>
        {
            public override void Bake(PowerBoxAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PowerBox>(entity, new PowerBox
                {
                    timer = 5f,
                    hasBeenPickedUp = 0,
                    damageMult = 5,
                    destroy = false
                });
            }
        }
    }
}