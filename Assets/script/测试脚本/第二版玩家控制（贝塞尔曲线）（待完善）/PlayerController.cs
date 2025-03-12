using UnityEngine;
using Obi;

public class PlayerController : MonoBehaviour
{
    public HandController handController;
    public Foot footController;
    public bool startWithFootControl = true;
    public ObiParticleAttachment rightHandControl;
    public GameObject handBallPrefab; // �ֲ����Ԥ����
    public Transform handBallSpawnPosition; // �ֲ������ɵ�λ��

    private GameObject handBall;

    private void Start()
    {
        handController.enabled = !startWithFootControl;
        footController.enabled = startWithFootControl;

        if (!startWithFootControl)
        {
            SpawnHandBall();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            ToggleControlMode();
        }
    }

    private void ToggleControlMode()
    {
        bool newState = !footController.enabled;
        footController.enabled = newState;
        handController.enabled = !newState;

        if (footController.enabled)
        {
            DestroyHandBall();
            rightHandControl.enabled = false;
        }
        else
        {
            SpawnHandBall();
            rightHandControl.enabled = true;
        }
    }

    private void SpawnHandBall()
    {
        if (handBallPrefab == null || handBallSpawnPosition == null)
        {
            Debug.LogError("HandBallPrefab or HandBallSpawnPosition is not set.");
            return;
        }

        // �����ֲ���
        handBall = Instantiate(handBallPrefab, handBallSpawnPosition.position, handBallSpawnPosition.rotation);

        // ���ֲ���󶨵� ObiParticleAttachment
        rightHandControl.target = handBall.transform;

        // ���ֲ��򴫵ݸ� HandController
        handController.SetControlObject(handBall);
    }

    private void DestroyHandBall()
    {
        if (handBall != null)
        {
            Destroy(handBall);
            handBall = null;
        }
    }
}