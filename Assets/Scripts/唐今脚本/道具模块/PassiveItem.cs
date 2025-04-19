using UnityEngine.Events;

public class PassiveItem : ItemBase
{
    public override UnityEvent OnGrabbed { get; set; }
    public override UnityEvent OnReleased { get; set; }
    public override bool IsGrabbed { get; set; }
}