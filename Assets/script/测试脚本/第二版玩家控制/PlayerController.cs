using UnityEngine;
using Obi;

public class PlayerController : MonoBehaviour
{
    public HandController handController;
    public Foot footController;
    public bool startWithFootControl = true;
    public ObiParticleAttachment rightHandControl;
    public GameObject handBallPrefab; // �ֲ����Ԥ����

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
        if (handBallPrefab == null)
        {
            Debug.LogError("HandBallPrefab or HandBallSpawnPosition is not set.");
            return;
        }

        // �����ֲ���λ�ú���ת���� handBallSpawnPosition ��ʵʱ��������
        handBall = Instantiate(handBallPrefab, ObiGetGroupParticles.GetParticleWorldPositions(rightHandControl)[0], Quaternion.identity);

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

//using UnityEngine;
//using Obi;

//public class PlayerController : MonoBehaviour
//{
//    public HandControl2 handController; // ʹ�õڶ����ֲ�������
//    public Foot footController;
//    public bool startWithFootControl = true;
//    public ObiParticleAttachment leftHandControl; // ���ֿ��Ƶ�
//    public ObiParticleAttachment rightHandControl; // ���ֿ��Ƶ�
//    public GameObject handBallPrefabLeft; // �������Ԥ����
//    public GameObject handBallPrefabRight; // �������Ԥ����

//    private GameObject handBallLeft; // ������ʵ��
//    private GameObject handBallRight; // ������ʵ��

//    private void Start()
//    {
//        handController.enabled = !startWithFootControl;
//        footController.enabled = startWithFootControl;

//        if (!startWithFootControl)
//        {
//            SpawnHandBalls();
//        }
//    }

//    private void Update()
//    {
//        if (Input.GetMouseButtonDown(2)) // �м��л�����ģʽ
//        {
//            ToggleControlMode();
//        }
//    }

//    private void ToggleControlMode()
//    {
//        bool newState = !footController.enabled;
//        footController.enabled = newState;
//        handController.enabled = !newState;

//        if (footController.enabled)
//        {
//            DestroyHandBalls();
//            leftHandControl.enabled = false;
//            rightHandControl.enabled = false;
//        }
//        else
//        {
//            SpawnHandBalls();
//            leftHandControl.enabled = true;
//            rightHandControl.enabled = true;
//        }
//    }

//    private void SpawnHandBalls()
//    {
//        if (handBallPrefabLeft == null || handBallPrefabRight == null)
//        {
//            Debug.LogError("HandBallPrefabLeft or HandBallPrefabRight is not set.");
//            return;
//        }

//        // ����������
//        if (leftHandControl != null)
//        {
//            handBallLeft = Instantiate(handBallPrefabLeft, ObiGetGroupParticles.GetParticleWorldPositions(leftHandControl)[0], Quaternion.identity);
//            leftHandControl.target = handBallLeft.transform;
//        }

//        // ����������
//        if (rightHandControl != null)
//        {
//            handBallRight = Instantiate(handBallPrefabRight, ObiGetGroupParticles.GetParticleWorldPositions(rightHandControl)[0], Quaternion.identity);
//            rightHandControl.target = handBallRight.transform;
//        }

//        // �����ֺ������򴫵ݸ� HandController
//        handController.SetControlObjects(handBallLeft, handBallRight);
//    }

//    private void DestroyHandBalls()
//    {
//        if (handBallLeft != null)
//        {
//            Destroy(handBallLeft);
//            handBallLeft = null;
//        }

//        if (handBallRight != null)
//        {
//            Destroy(handBallRight);
//            handBallRight = null;
//        }
//    }
//}