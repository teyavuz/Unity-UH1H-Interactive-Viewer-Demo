using UnityEngine;

public class CameraAnchor : MonoBehaviour
{
    [Header("Enter State")]
    public GameManager.GameState enterState = GameManager.GameState.Inspect;

    [Header("Inspect Limits")]
    public float yawLimit = 25f;
    public float minDistance = 1.5f;
    public float maxDistance = 3.5f;

    public bool lockPitch = true;
    public float fixedPitch = 10f;
}
