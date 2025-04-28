using UnityEngine;
using Unity.Netcode;

public class TestBaseControl : MonoBehaviour
{
    public TestControllerInformation controlHandler;
    public GameObject forwardObject;
    protected virtual void Start()
    {
        controlHandler = GetComponent<TestControllerInformation>();

        NullCheckerTool.CheckNull(forwardObject, controlHandler);
    }
}
