using UnityEngine.Events;

public abstract class PassiveItem : ItemBase
{
    public override UnityEvent OnGrabbed { get; set; } = new UnityEvent();
    public override UnityEvent OnReleased { get; set; } = new UnityEvent();
}