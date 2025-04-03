using Unity.Entities;
using UnityEngine;

namespace IT4080C
{
    public struct HPBox : IComponentData
    {
        public float timer;
        public byte hasBeenPickedUp;
        public bool touchable;
        public bool destroy;
    }

    [DisallowMultipleComponent]
    public class HPBoxAuthoring : MonoBehaviour
    {


        class SpawnerBaker : Baker<HPBoxAuthoring>
        {
            public override void Bake(HPBoxAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<HPBox>(entity, new HPBox
                {
                    timer = 5f,
                    hasBeenPickedUp = 0,
                    touchable = false
                });
            }
        }
    }
}
