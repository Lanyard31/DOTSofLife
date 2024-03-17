using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using Unity.Rendering;

public class GenerationUpdateSystem : SystemBase
{
    private ComponentDataFromEntity<URPMaterialPropertyEmissionColor> EmissionGroup;

    protected override void OnCreate()
    {
        EmissionGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(true);
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        var emissionGroup = EmissionGroup;

        Entities.ForEach((Entity entity) =>
        {
            /*
            //needs to check if entities have living tag
            var emissionComponent = emissionGroup[entity];
            emissionComponent.Value.x = 0.1499598f;
            emissionComponent.Value.y = 0.8468735f;
            emissionComponent.Value.z = 0.8468735f;
            emissionGroup[entity] = emissionComponent;
            */
        }).Schedule();
    }

}
