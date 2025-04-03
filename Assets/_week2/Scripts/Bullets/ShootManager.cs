using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using IT4080C;

namespace _week2.Scripts.Bullets
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    partial struct ShootManager : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletSpawner>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();

            var prefab = SystemAPI.GetSingleton<BulletSpawner>().Bullet;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            // NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();

            foreach ((
                var playerInput,
                var localTransform,
                var ghostOwner,
                var poweredUpComp)
                in SystemAPI.Query<
                RefRO<CubeInput>,
                RefRO<LocalTransform>,
                RefRO<GhostOwner>,
                RefRO<PoweredUpComponent>>().WithAll<Simulate>())
            {
                // if (networkTime.IsFirstTimeFullyPredictingTick)
                // {
                    if (playerInput.ValueRO.shoot.IsSet)
                    {
                        // Debug.LogWarning("Shoot Input");
                        Entity bulletEntity = ecb.Instantiate(prefab);
                        LocalTransform bulletLocalTransform = SystemAPI.GetComponent<LocalTransform>(prefab);

                        int bulletOffset = 3;
                        var forwardDir = math.mul(localTransform.ValueRO.Rotation, Vector3.forward);

                        bulletLocalTransform.Position = localTransform.ValueRO.Position + forwardDir * bulletOffset;
                        bulletLocalTransform.Rotation = localTransform.ValueRO.Rotation;
                        // var bulletLocalTransform = LocalTransform.FromPositionRotation(
                        //     localTransform.ValueRO.Position,
                        //     localTransform.ValueRO.Rotation
                        // );
 
                        ecb.SetComponent(
                            bulletEntity,
                            new Bullet
                            {
                                hasHit = 0,
                                hittable = false,
                                ownerNetworkID = ghostOwner.ValueRO.NetworkId,
                                timer = 5f,
                                damageMult = poweredUpComp.ValueRO.poweredUpMultiplier
                            }
                        );
                        ecb.SetComponent(bulletEntity, bulletLocalTransform);
                        ecb.SetComponent(bulletEntity, new GhostOwner { NetworkId = ghostOwner.ValueRO.NetworkId });
                    }
                // }
            }
            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
