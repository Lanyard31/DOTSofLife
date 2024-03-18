using Unity.Entities;

[GenerateAuthoringComponent]
public struct PersonTag : IComponentData
{
    public bool shouldBeAlive;
    public bool IsAlive;
    public int xCoord;
    public int zCoord;
}