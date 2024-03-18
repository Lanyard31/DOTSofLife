using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using System;
using Unity.Rendering;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine.Rendering;

[AlwaysUpdateSystem]
public class ClickToToggleSystem : SystemBase
{
    private Camera _mainCamera;
    private BuildPhysicsWorld _buildPhysicsWorld;
    private CollisionWorld _collisionWorld;

    protected override void OnCreate()
    {
        _mainCamera = Camera.main;
        _buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }

    protected override void OnUpdate()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (Input.GetMouseButton(0))
        {
            if (_buildPhysicsWorld != null && _buildPhysicsWorld.PhysicsWorld.Equals(default(PhysicsWorld)))
            {
                Debug.LogError("_buildPhysicsWorld.PhysicsWorld is null. Ensure it is properly initialized.");
                return;
            }
            SelectSingleUnit();
        }
    }

    private void SelectSingleUnit()
    {

        _collisionWorld = _buildPhysicsWorld.PhysicsWorld.CollisionWorld;
        //here is the culprit

        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        var rayStart = ray.origin;
        var rayEnd = ray.GetPoint(2000f);

        if (Raycast(rayStart, rayEnd, out var raycastHit))
        {
            var hitEntity = _buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
            if (EntityManager.HasComponent<PersonTag>(hitEntity))
            {
                var personTag = EntityManager.GetComponentData<PersonTag>(hitEntity);
                if (personTag.IsAlive)
                {
                    // Make dead
                    personTag.IsAlive = false;
                    EntityManager.SetComponentData(hitEntity, personTag);
                    
                    var emissionGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(false);
                    var emissionComponent = emissionGroup[hitEntity];
                    emissionComponent.Value = new float4(0.0001f, 0, 0, 1f);
                    emissionGroup[hitEntity] = emissionComponent;
                }
                else
                {
                    // Make alive
                    personTag.IsAlive = true;
                    EntityManager.SetComponentData(hitEntity, personTag);

                    Debug.Log("Should be alive and lighting up");

                    var emissionGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(false);
                    var emissionComponent = emissionGroup[hitEntity];
                    emissionComponent.Value = new float4(0.1499598f, 0.8468735f, 0.8468735f, 1f);
                    emissionGroup[hitEntity] = emissionComponent;
                }
            }
        }
    }

    private bool Raycast(float3 rayStart, float3 rayEnd, out RaycastHit raycastHit)
    {
        var raycastInput = new RaycastInput
        {
            Start = rayStart,
            End = rayEnd,
            Filter = new CollisionFilter
            {
                BelongsTo = (uint) CollisionLayers.Selection,
                CollidesWith = (uint) (CollisionLayers.Ground | CollisionLayers.Units)
            }
        };
        return _collisionWorld.CastRay(raycastInput, out raycastHit);
    }
}
