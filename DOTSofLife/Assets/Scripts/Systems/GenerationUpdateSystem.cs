using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

public class GenerationUpdateSystem : SystemBase
{
    private EntityCommandBufferSystem entityCommandBufferSystem;
    private readonly float spread = 5f; //from spawner
    private BuildPhysicsWorld buildPhysicsWorldSystem;
    private TimerBehavior timerBehavior;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        GameObject timerObject = GameObject.Find("TimerObject");
        if (timerObject != null)
        {
            timerBehavior = timerObject.GetComponent<TimerBehavior>();
        }
    }

    protected override void OnUpdate()
    {
        if (timerBehavior != null && timerBehavior.isActive == true)
        {
            return;
        }
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
        float localSpread = spread;
        PhysicsWorld physicsWorld = buildPhysicsWorldSystem.PhysicsWorld;
        var entityManager = EntityManager;

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref PersonTag personTag, in Translation translation) =>
        {
            int liveNeighborCount = 0;

            float3 currentPosition = translation.Value;

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0)
                        continue;

                    float3 neighborOffset = new float3(x * localSpread, 0, z * localSpread);
                    float3 neighborPosition = currentPosition + neighborOffset;

                    RaycastInput raycastInput = new RaycastInput
                    {
                        Start = neighborPosition + new float3(0, 1, 0),
                        End = neighborPosition + new float3(0, -1, 0),
                        Filter = new CollisionFilter
                        {
                            BelongsTo = (uint)CollisionLayers.Selection,
                            CollidesWith = (uint)(CollisionLayers.Ground | CollisionLayers.Units)
                        }
                    };

                    if (physicsWorld.CastRay(raycastInput, out RaycastHit hit))
                    {
                        var hitEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                        bool isAlive = false;
                        if (entityManager.HasComponent<PersonTag>(hitEntity))
                        {
                            isAlive = entityManager.GetComponentData<PersonTag>(hitEntity).IsAlive;
                        }

                        if (isAlive)
                        {
                            //Debug.Log($"FoundLivingNeighbor");
                            liveNeighborCount++;
                        }
                    }
                }
            }

            bool shouldBeAlive = (personTag.IsAlive && liveNeighborCount >= 2 && liveNeighborCount <= 3) || (!personTag.IsAlive && liveNeighborCount == 3);
            personTag.shouldBeAlive = shouldBeAlive;

        }).Run();

        Entities.ForEach((Entity entity, ref PersonTag personTag) =>
        {
            personTag.IsAlive = personTag.shouldBeAlive;
            var emissionGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(false);
            var emissionComponent = emissionGroup[entity];

            if (personTag.IsAlive)
            {
                emissionComponent.Value = new float4(0.1499598f, 0.8468735f, 0.8468735f, 1f);
                emissionGroup[entity] = emissionComponent;
            }
            else
            {
                emissionComponent.Value = new float4(0.0001f, 0, 0, 1f);
                emissionGroup[entity] = emissionComponent;
            }
        }).Run();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
