using Unity.Entities;

[GenerateAuthoringComponent]
public struct PersonTag : IComponentData
{
    public bool IsAlive;
}