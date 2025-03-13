using UnityEngine;
using Obi;
using Unity.Netcode;
public class PlayerController : NetworkBehaviour
{
    public HandController handController;
    public Foot footController;
    public bool startWithFootControl = true;
    public ObiParticleAttachment rightHandControl;
    public GameObject handBallPrefab; // 手部球的预制体
    public GameObject player_camera;
    public GameObject playerColliderWorld;
    public GameObject handBall;

    private void Start()
    {
        handController.enabled = !startWithFootControl;
        footController.enabled = startWithFootControl;

        if (!startWithFootControl)
        {
            SpawnHandBallRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GameManager.instance.OnStartGame.AddListener(() =>
        {
            GameManager.instance.isGameing = true;
        });
        if (!IsLocalPlayer)
        {
            player_camera.SetActive(false);
            DestroyImmediate(playerColliderWorld);
        }

    }

    private void Update()
    {
        if (!GameManager.instance.isGameing) return;

        if (!IsLocalPlayer&& NetworkManager.Singleton) return;

        if (Input.GetMouseButtonDown(2))
        {
            ToggleControlModeRpc();
        }
    }
    [Rpc(SendTo.Everyone)]
    private void ToggleControlModeRpc()
    {
        bool newState = !footController.enabled;
        footController.enabled = newState;
        handController.enabled = !newState;

        if (footController.enabled)
        {
            DestroyHandBallRpc();
            rightHandControl.enabled = false;
        }
        else
        {
            SpawnHandBallRpc();
            rightHandControl.enabled = true;
        }
    }

    private void SpawnHandBallRpc()
    {
        if (handBallPrefab == null)
        {
            Debug.LogError("HandBallPrefab or HandBallSpawnPosition is not set.");
            return;
        }
        handBall = handBallPrefab;
        // 生成手部球，位置和旋转基于 handBallSpawnPosition 的实时世界坐标
        //handBall = Instantiate(handBallPrefab, ObiGetGroupParticles.GetParticleWorldPositions(rightHandControl)[0],Quaternion.identity);
        handBall.transform.position = ObiGetGroupParticles.GetParticleWorldPositions(rightHandControl)[0];
        // 将手部球绑定到 ObiParticleAttachment
        rightHandControl.target = handBall.transform;

        // 将手部球传递给 HandController
        handController.SetControlObject(handBall);
    }

    private void DestroyHandBallRpc()
    {
        if (handBall != null)
        {
            handController.SetControlObject(null);
            handBall = null;
        }
    }


    private void OnApplicationQuit()
    {
        if(NetworkManager.Singleton.IsListening && NetworkObject != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}