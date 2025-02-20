using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
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
        CollisionSimulationJob simJob = new CollisionSimulationJob();
        state.Dependency = simJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    public partial struct CollisionSimulationJob : ICollisionEventsJob
    {
        public void Execute(CollisionEvent collisionEvent)
        {
            Debug.LogWarning("Collision!!1!");
        }
    }
}
