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
    private const int minLivingCellsToStart = 5;
    private NativeList<Entity> livingCells;
    private NativeList<Entity> deadCells;
    private EntityCommandBufferSystem entityCommandBufferSystem;
    private float spread = 5f; //from spawner
    private BuildPhysicsWorld buildPhysicsWorldSystem;

    protected override void OnCreate()
    {
        livingCells = new NativeList<Entity>(Allocator.Persistent);
        deadCells = new NativeList<Entity>(Allocator.Persistent);
        entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnDestroy()
    {
        livingCells.Dispose();
        deadCells.Dispose();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
        float localSpread = spread;
        PhysicsWorld physicsWorld = buildPhysicsWorldSystem.PhysicsWorld;
        var localLivingCells = new NativeList<Entity>(Allocator.Temp);
        var localDeadCells = new NativeList<Entity>(Allocator.Temp);
        var entityManager = EntityManager;

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref LivingTag livingTag, in Translation translation) =>
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

                    RaycastHit hit;
                    if (physicsWorld.CastRay(raycastInput, out hit))
                    {
                        var hitEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                        bool hasLivingTag = entityManager.HasComponent<LivingTag>(hitEntity);

                        if (hasLivingTag)
                        {
                            Debug.Log("Got a live one!");
                            liveNeighborCount++;
                        }
                    }
                }
            }

            bool shouldBeAlive = (liveNeighborCount >= 2 && liveNeighborCount <= 3);
            //shouldBeAlive = !livingTag.IsAlive && liveNeighborCount == 3;

            if (shouldBeAlive)
            {
                localLivingCells.Add(entity);
                commandBuffer.AddComponent(entity, new LivingTag());
                var emissionGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(false);
                var emissionComponent = emissionGroup[entity];
                emissionComponent.Value.x = 0.1499598f;
                emissionComponent.Value.y = 0.8468735f;
                emissionComponent.Value.z = 0.8468735f;
                emissionGroup[entity] = emissionComponent;
            }
            else
            {
                localDeadCells.Add(entity);
                commandBuffer.RemoveComponent<LivingTag>(entity);
            }
        }).Run();

        localLivingCells.Dispose();
        localDeadCells.Dispose();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}