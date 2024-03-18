using Unity.Entities;

public struct TimerComponent : IComponentData
{
    public bool isActive;
    public float timeRemaining;
}
