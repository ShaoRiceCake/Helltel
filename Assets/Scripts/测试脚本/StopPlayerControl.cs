using UnityEngine;

[RequireComponent(typeof(PlayerControlInformationProcess))]
public class StopPlayerControl : MonoBehaviour
{
    private PlayerControlInformationProcess _playerControl;
    private void Awake()
    {
        _playerControl = GetComponent<PlayerControlInformationProcess>();
        if (_playerControl == null)
        {
            Debug.LogError("PlayerControlInformationProcess component not found!", this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _playerControl._stopPlayerControl = !_playerControl._stopPlayerControl;
        }
    }

}