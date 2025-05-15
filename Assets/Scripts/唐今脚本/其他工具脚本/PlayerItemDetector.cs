using UnityEngine;

public class PlayerItemDetector : MonoBehaviour
{
    
    [Header("Debug")]
    public bool isDetectPlayer; 
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            isDetectPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            isDetectPlayer = false;
        }
    }
    
}