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
    }

    protected override void OnUpdate()
    {

        if (Input.GetMouseButtonDown(0))
        {
            SelectSingleUnit();
        }
    }
        
    private void SelectSingleUnit()
    {
        _collisionWorld = _buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        var rayStart = ray.origin;
        var rayEnd = ray.GetPoint(10000f);

        if(Raycast(rayStart, rayEnd, out var raycastHit))
        {
            var hitEntity = _buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
            if (EntityManager.HasComponent<PersonTag>(hitEntity))
            {
                if (EntityManager.HasComponent<LivingTag>(hitEntity))
                {
                    //make dead
                    EntityManager.RemoveComponent<LivingTag>(hitEntity);
                    var emissionGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(false);
                    var emissionComponent = emissionGroup[hitEntity];
                    emissionComponent.Value.x = 0.0001f;
                    emissionComponent.Value.y = 0;
                    emissionComponent.Value.z = 0;
                    emissionGroup[hitEntity] = emissionComponent;
                }
                else
                {
                    //make alive
                    EntityManager.AddComponent<LivingTag>(hitEntity);
                    var emissionGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(false);
                    var emissionComponent = emissionGroup[hitEntity];
                    emissionComponent.Value.x = 0.1499598f;
                    emissionComponent.Value.y = 0.8468735f;
                    emissionComponent.Value.z = 0.8468735f;
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
