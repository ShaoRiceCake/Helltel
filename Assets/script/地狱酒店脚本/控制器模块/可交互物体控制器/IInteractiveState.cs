public enum InteractiveState
{
    Normal,     // 未被选中、未被抓取
    Selected,   // 被选中，未被抓取
    Grabbed     // 被抓取
}

public interface IInteractiveState
{
    void EnterState(InteractiveObjectController controller);
    void ExitState(InteractiveObjectController controller);
}

public class NormalState : IInteractiveState
{
    public void EnterState(InteractiveObjectController controller)
    {
        controller.SetHighlight(false);
    }

    public void ExitState(InteractiveObjectController controller) { }
}

public class SelectedState : IInteractiveState
{
    public void EnterState(InteractiveObjectController controller)
    {
        controller.SetHighlight(true);
    }

    public void ExitState(InteractiveObjectController controller)
    {
        controller.SetHighlight(false);
    }
}

public class GrabbedState : IInteractiveState
{
    public void EnterState(InteractiveObjectController controller)
    {
        controller.SetHighlight(false);
        controller.LockOutline(); // 被抓取时锁定高亮状态
    }

    public void ExitState(InteractiveObjectController controller)
    {
        controller.UnlockOutline(); // 释放时解锁
    }
}
