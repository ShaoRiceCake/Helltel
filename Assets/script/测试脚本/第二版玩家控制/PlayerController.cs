using UnityEngine;
using Obi;

public class PlayerController : MonoBehaviour
{
    public HandController handController;
    public Foot footController;
    public bool startWithFootControl = true;
    public ObiParticleAttachment rightHandControl;
    public GameObject handBallPrefab; // 手部球的预制体

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

        // 生成手部球，位置和旋转基于 handBallSpawnPosition 的实时世界坐标
        handBall = Instantiate(handBallPrefab, ObiGetGroupParticles.GetParticleWorldPositions(rightHandControl)[0], Quaternion.identity);

        // 将手部球绑定到 ObiParticleAttachment
        rightHandControl.target = handBall.transform;

        // 将手部球传递给 HandController
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
//    public HandControl2 handController; // 使用第二版手部控制器
//    public Foot footController;
//    public bool startWithFootControl = true;
//    public ObiParticleAttachment leftHandControl; // 左手控制点
//    public ObiParticleAttachment rightHandControl; // 右手控制点
//    public GameObject handBallPrefabLeft; // 左手球的预制体
//    public GameObject handBallPrefabRight; // 右手球的预制体

//    private GameObject handBallLeft; // 左手球实例
//    private GameObject handBallRight; // 右手球实例

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
//        if (Input.GetMouseButtonDown(2)) // 中键切换控制模式
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

//        // 生成左手球
//        if (leftHandControl != null)
//        {
//            handBallLeft = Instantiate(handBallPrefabLeft, ObiGetGroupParticles.GetParticleWorldPositions(leftHandControl)[0], Quaternion.identity);
//            leftHandControl.target = handBallLeft.transform;
//        }

//        // 生成右手球
//        if (rightHandControl != null)
//        {
//            handBallRight = Instantiate(handBallPrefabRight, ObiGetGroupParticles.GetParticleWorldPositions(rightHandControl)[0], Quaternion.identity);
//            rightHandControl.target = handBallRight.transform;
//        }

//        // 将左手和右手球传递给 HandController
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