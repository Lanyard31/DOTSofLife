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
    private const int minLivingCellsToStart = 3;
    private int numberOfLivingCells;
    private bool shouldRunLifeLogic = false;
    private EntityCommandBufferSystem entityCommandBufferSystem;
    private float spread = 5f; //from spawner
    private BuildPhysicsWorld buildPhysicsWorldSystem;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
        float localSpread = spread;
        PhysicsWorld physicsWorld = buildPhysicsWorldSystem.PhysicsWorld;
        var entityManager = EntityManager;
        bool shouldRunLifeLogic = this.shouldRunLifeLogic;
        int localNumberOfLivingCells = numberOfLivingCells;

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

                    RaycastHit hit;
                    if (physicsWorld.CastRay(raycastInput, out hit))
                    {
                        var hitEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                        bool isAlive = false;
                        if (entityManager.HasComponent<PersonTag>(hitEntity))
                        {
                            isAlive = entityManager.GetComponentData<PersonTag>(hitEntity).IsAlive;
                        }

                        if (isAlive)
                        {
                            Debug.Log($"FoundLivingNeighbor");
                            liveNeighborCount++;
                        }
                    }
                }
            }

            if (liveNeighborCount > 0)
            {
                Debug.Log($"Length of liveNeighborCount: {liveNeighborCount}");
            }

            if (liveNeighborCount >= minLivingCellsToStart)
            {
                shouldRunLifeLogic = true;
            }

            else if (shouldRunLifeLogic && localNumberOfLivingCells < 1)
            {
                shouldRunLifeLogic = false;
            }


            if (shouldRunLifeLogic)
            {
                Debug.Log("Should run life logic!");
                bool shouldBeAlive = (personTag.IsAlive && liveNeighborCount >= 2 && liveNeighborCount <= 3) || (!personTag.IsAlive && liveNeighborCount == 3);

                if (shouldBeAlive)
                {
                    localNumberOfLivingCells++;
                    personTag.IsAlive = true;

                    var emissionGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(false);
                    var emissionComponent = emissionGroup[entity];
                    emissionComponent.Value = new float4(0.0001f, 0, 0, 1f);
                    emissionGroup[entity] = emissionComponent;
                }

                else
                {
                    localNumberOfLivingCells--;
                    personTag.IsAlive = false;

                    var emissionGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(false);
                    var emissionComponent = emissionGroup[entity];
                    emissionComponent.Value = new float4(0.0001f, 0, 0, 1f);
                    emissionGroup[entity] = emissionComponent;
                }
            }
        }).Run();

        this.numberOfLivingCells = localNumberOfLivingCells;
        this.shouldRunLifeLogic = shouldRunLifeLogic;

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
