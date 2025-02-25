using IT4080C;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;


partial struct CollisionHandler : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        CollisionSimulationJob simulationJob = new CollisionSimulationJob
        {
            PlayerHealthLookup = SystemAPI.GetComponentLookup<Health>(),

        };
        state.Dependency = simulationJob.Schedule(
            SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

[WithAll(typeof(Simulate))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSimulationGroup))]
[BurstCompile]
public partial struct CollisionSimulationJob : ICollisionEventsJob
{
    public ComponentLookup<Health> PlayerHealthLookup;

    public void Execute(CollisionEvent collisionEvent)
    {
        if(PlayerHealthLookup.TryGetComponent(collisionEvent.EntityB, out Health health))
        {
            health.currentHealth -= 1f;
            PlayerHealthLookup[collisionEvent.EntityB] = health;
            Debug.Log("Owww My health is: "+health.currentHealth);
        }
    }
}
