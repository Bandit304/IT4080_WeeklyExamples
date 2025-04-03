using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace _week2.Scripts
{
    /// <summary>
    /// Input for the cube to move in the example setup.
    /// </summary>
    public struct CubeInput : IInputComponentData
    {
        /// <summary>
        /// Horizontal movement (X axis).
        /// </summary>
        public int Horizontal;
        
        /// <summary>
        /// Vertical movement (Z axis).
        /// </summary>
        public int Vertical;

        public float RotationX;
        public float RotationY;

        public InputEvent shoot;
    }

    /// <summary>
    /// The authoring component for the CubeInput.
    /// </summary>
    [DisallowMultipleComponent]
    public class CubeInputAuthoring : MonoBehaviour
    {
        class Baking : Baker<CubeInputAuthoring>
        {
            public override void Bake(CubeInputAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<CubeInput>(entity);
            }
        }
    }

    /// <summary>
    /// System in charge of setting the cube input data based on the pressed keys.
    /// Input: whether keys are pressed, <see cref="CubeInput"/> component data.
    /// Output: modified <see cref="CubeInput"/> component data for which the local client is the owner.
    /// </summary>
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial struct SampleCubeInput : ISystem
    {
        /// <summary>
        /// Sets the cube input data based on the pressed keys.
        /// </summary>
        /// <param name="state">Raw entity system state, unused here.</param>
        public void OnUpdate(ref SystemState state)
        {
#if ENABLE_INPUT_SYSTEM
            var left = Keyboard.current.aKey.isPressed;
            var right = Keyboard.current.dKey.isPressed;
            var down = Keyboard.current.sKey.isPressed;
            var up = Keyboard.current.wKey.isPressed;
            var shootKey = Mouse.current.leftButton.wasPressedThisFrame;
            // var mouseX = Mouse.current.delta.x.value;
            // var mouseY = Mouse.current.delta.y.value;
#else
            var left = UnityEngine.Input.GetKey(KeyCode.A);
            var right = UnityEngine.Input.GetKey(KeyCode.D);
            var down = UnityEngine.Input.GetKey(KeyCode.S);
            var up = UnityEngine.Input.GetKey(KeyCode.W);
            var shootKey = UnityEngine.Input.GetKeyDown("Fire1");
            // var mouseX = UnityEngine.Input.GetAxis("Mouse X");
            // var mouseY = UnityEngine.Input.GetAxis("Mouse Y");
#endif
            var mouseX = UnityEngine.Input.GetAxis("Mouse X");
            var mouseY = UnityEngine.Input.GetAxis("Mouse Y");
            
            foreach (var playerInput in SystemAPI.Query<RefRW<CubeInput>>().WithAll<GhostOwnerIsLocal>())
            {
                playerInput.ValueRW = default;
                if (left)
                    playerInput.ValueRW.Horizontal -= 1;
                if (right)
                    playerInput.ValueRW.Horizontal += 1;
                if (down)
                    playerInput.ValueRW.Vertical -= 1;
                if (up)
                    playerInput.ValueRW.Vertical += 1;
                playerInput.ValueRW.RotationX += mouseX;
                playerInput.ValueRW.RotationY += mouseY;

                if (shootKey)
                    playerInput.ValueRW.shoot.Set();
                else
                    playerInput.ValueRW.shoot = default;
            }
        }
    }
    
    /// <summary>
    /// System in charge of moving the cube based on the input for entities with the Simulate flag.
    /// Input: <see cref="CubeInput"/> component data, <see cref="LocalTransform"/> component data.
    /// Output: modified <see cref="LocalTransform"/> component data.
    /// </summary>
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [BurstCompile]
    public partial struct CubeMovementSystem : ISystem
    {
        /// <summary>
        /// Modifies the local transforms of the cubes based on the input.
        /// </summary>
        /// <param name="state">Raw entity system state, unused here.</param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var speed = SystemAPI.Time.DeltaTime * 4;
            var rSpeed = SystemAPI.Time.DeltaTime * 200;
            var backCamOffset = 2;
            var rightCamOffset = 1;
            var upCamOffset = 0.5f;

            foreach (var (input, trans) in
                     SystemAPI.Query<RefRO<CubeInput>, RefRW<LocalTransform>>()
                         .WithAll<Simulate>())
            {
                trans.ValueRW.Rotation *= Quaternion.AngleAxis(input.ValueRO.RotationX * rSpeed, Vector3.up);

                // var moveInput = new float2(input.ValueRO.Horizontal, input.ValueRO.Vertical);
                // moveInput = math.normalizesafe(moveInput) * speed;
                // trans.ValueRW.Position += new float3(moveInput.x, 0, moveInput.y);

                var forwardDir = math.mul(trans.ValueRO.Rotation, Vector3.forward);
                var rightDir = math.mul(trans.ValueRO.Rotation, Vector3.right);

                Vector3 moveVector = input.ValueRO.Vertical * forwardDir + input.ValueRO.Horizontal * rightDir;
                trans.ValueRW.Position += new float3(moveVector * speed);
            }

            foreach (
                var (trans, input) in 
                SystemAPI.Query<RefRW<LocalTransform>, RefRO<CubeInput>>()
                    .WithAll<GhostOwner, GhostOwnerIsLocal>()
                    .WithAll<Simulate>()
            )
            {
                if (!state.World.IsServer())
                {
                    // Entity mainEntityCameraEntity = SystemAPI.GetSingletonEntity<MainCamera>();
                    // MainCamera mainCamera = SystemAPI.GetSingleton<MainCamera>();

                    var backDir = math.mul(trans.ValueRO.Rotation, Vector3.back);
                    var rightDir = math.mul(trans.ValueRO.Rotation, Vector3.right);
                    var upDir = math.mul(trans.ValueRO.Rotation, Vector3.up);

                    float3 camPos = trans.ValueRO.Position +
                        backDir * backCamOffset +
                        rightDir * rightCamOffset +
                        upDir * upCamOffset;

                    MainGameObjectCamera.Instance.transform.SetPositionAndRotation(camPos, trans.ValueRO.Rotation);
                    MainGameObjectCamera.Instance.fieldOfView = 75;
                }
            }

            //check if dead and respawn
            foreach ((RefRW<HealthComponent> playerHealth,
                      RefRW<LocalTransform> playerPosition)
                        in
                    SystemAPI.Query<
                        RefRW<HealthComponent>,
                        RefRW<LocalTransform>>()
                    .WithAll<HealthComponent>()
                    .WithAll<Simulate>())
            {
                if (playerHealth.ValueRO.CurrentHealth <= 0)
                {
                    List<LocalTransform> localTrans = new List<LocalTransform>();
                    foreach (var spawnTrans in
                    SystemAPI.Query<RefRW<LocalTransform>>()
                    .WithAll<SpawnPoint>()
                    .WithAll<Simulate>())
                    {
                        Debug.Log("SPpoint found " + spawnTrans.ValueRW.Position.x);
                        localTrans.Add(spawnTrans.ValueRW);
                    }
                    //adjust y offset
                    LocalTransform tempTransform = localTrans[UnityEngine.Random.Range(0, localTrans.Count - 1)];
                    tempTransform.Position.y += 2f;
                    tempTransform.Position.z += -2.456f;

                    playerPosition.ValueRW.Position = tempTransform.Position;
                    playerHealth.ValueRW.CurrentHealth = playerHealth.ValueRO.MaxHealth;
                }
            }
        }
    }
}
