using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public HandController handController;
    public FootController footController;
    public bool startWithFootControl = true;
    public GameObject handBall;

    private void Start()
    {
        handController.enabled = !startWithFootControl;
        footController.enabled = startWithFootControl;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            ToggleControlMode();
        }

        if (footController.enabled)
        {
            handBall.SetActive(false);
        }
        else
        {
            handBall.SetActive(true);
        }
    }

    private void ToggleControlMode()
    {
        bool newState = !footController.enabled;
        footController.enabled = newState;
        handController.enabled = !newState;


    }
}