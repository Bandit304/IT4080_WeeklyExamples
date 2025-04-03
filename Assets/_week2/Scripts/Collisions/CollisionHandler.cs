using IT4080C;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEditor;
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
            PlayerHealthLookup = SystemAPI.GetComponentLookup<HealthComponent>(),
            BulletLookup = SystemAPI.GetComponentLookup<Bullet>(),
            HPBoxLookup = SystemAPI.GetComponentLookup<HPBox>(),
            PowerBoxLookup = SystemAPI.GetComponentLookup<PowerBox>(),
            PoweredUpComponentLookup = SystemAPI.GetComponentLookup<PoweredUpComponent>()
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
    public ComponentLookup<HealthComponent> PlayerHealthLookup;
    public ComponentLookup<Bullet> BulletLookup;
    public ComponentLookup<HPBox> HPBoxLookup;
    public ComponentLookup<PowerBox> PowerBoxLookup;
    public ComponentLookup<PoweredUpComponent> PoweredUpComponentLookup;
    public void Execute(CollisionEvent collisionEvent)
    {
        if(PlayerHealthLookup.TryGetComponent(collisionEvent.EntityB, out HealthComponent health))
        {
            if (BulletLookup.TryGetComponent(collisionEvent.EntityA, out Bullet bullet))
            {
                if (bullet.hasHit != 1 && bullet.hittable)
                {
                    if (bullet.ownerNetworkID != health.ownerNetworkID)
                    {
                        health.CurrentHealth -= 50f * bullet.damageMult;
                        PlayerHealthLookup[collisionEvent.EntityB] = health;
                        Debug.Log("I am #: " + health.ownerNetworkID);
                        Debug.Log("Owww My health is: " + health.CurrentHealth);
                        Debug.Log("I was hit by: " + bullet.ownerNetworkID);
                        Debug.Log("DMG Mult: " + bullet.damageMult);

                        if (health.CurrentHealth <= 0)
                        {
                            Debug.Log("You dead?");
                            bullet.killed = true;
                        }

                        bullet.hasHit = 1;
                        bullet.timer = 0;
                        bullet.hittable = false;
                        bullet.hitPlayerNetworkID = health.ownerNetworkID;
                        BulletLookup[collisionEvent.EntityA] = bullet;
                    }
                }
            }
            else if(HPBoxLookup.TryGetComponent(collisionEvent.EntityA, out HPBox hpBox))
            {
                float healthPickUpValue = 25f;
                //doo health math
                if (hpBox.hasBeenPickedUp != 1)
                {
                    if (health.CurrentHealth + healthPickUpValue >= health.MaxHealth)
                    {
                        Debug.LogWarning("Set to Max HP");
                        health.CurrentHealth = health.MaxHealth;
                    }
                    else if (health.CurrentHealth + healthPickUpValue < health.MaxHealth)
                    {
                        Debug.LogWarning("Set to Curr HP +"+healthPickUpValue);
                        health.CurrentHealth += healthPickUpValue;
                    }
                    else if (health.CurrentHealth <= 0)
                    {
                        Debug.Log("You dead");
                        //do death scene
                    }
                    PlayerHealthLookup[collisionEvent.EntityB] = health;
                    //doo stuff to HPBox
                    hpBox.hasBeenPickedUp = 1;
                    hpBox.destroy = true;
                    //add me
                    HPBoxLookup[collisionEvent.EntityA] = hpBox; 
                }
            }
            else if (PowerBoxLookup.TryGetComponent(collisionEvent.EntityA, out PowerBox powerBox) && PoweredUpComponentLookup.TryGetComponent(collisionEvent.EntityB, out PoweredUpComponent powerUp))
            {
                if (powerBox.hasBeenPickedUp != 1)
                {
                    Debug.LogWarning("Unlimited POWER!!");
                    //add mult to player damage
                    powerUp.poweredUpMultiplier = 5f;
                    PoweredUpComponentLookup[collisionEvent.EntityB] = powerUp; 

                    //destroy power cube on pickup
                    powerBox.hasBeenPickedUp = 1;
                    powerBox.destroy = true;
                    //add me
                    PowerBoxLookup[collisionEvent.EntityA] = powerBox;
                    
                }
              
            }

        }
       
    }
}