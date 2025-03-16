using UnityEngine;

public abstract class ControllerBase : MonoBehaviour
{
    protected abstract void InitializeController();

    protected abstract void DestroyController();

}