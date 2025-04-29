public bool DEBUG_STOP_BEHAVIOR_TREE = false; // 调试用，是否停止行为树

void Update()
{
    // ...existing code...

    // 如果沟上了，且行为树正在运行，就停止行为树
    if (DEBUG_STOP_BEHAVIOR_TREE && IsBehaviorTreeRunning())
    {
        StopBehaviorTree();
    }

    // ...existing code...
}

private bool IsBehaviorTreeRunning()
{
    // 检查行为树是否正在运行的逻辑
    // ...existing code...
    return false; // 示例返回值，请根据实际逻辑修改
}

private void StopBehaviorTree()
{
    // 停止行为树的逻辑
    // ...existing code...
}
