using UnityEngine;
using DG.Tweening;

public class CameraAnchorNavigator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OrbitCameraController orbit;
    [SerializeField] private Transform cameraRig;

    [Header("Transition")]
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private Ease moveEase = Ease.InOutCubic;

    private Tween moveTween;
    private Tween rotTween;

    public void GoToAnchor(CameraAnchor anchor)
    {
        if (anchor == null || orbit == null || cameraRig == null) return;

        moveTween?.Kill();
        rotTween?.Kill();

        // İstersen state: Transition
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameManager.GameState.Transition);

        orbit.enabled = false;

        moveTween = cameraRig.DOMove(anchor.transform.position, moveDuration).SetEase(moveEase);

        rotTween = cameraRig.DORotateQuaternion(anchor.transform.rotation, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                // Rotasyonu controller'a senkronla
                orbit.SnapRotationTo(anchor.transform.rotation);

                // Zoom'u MAX'e çek
                orbit.SnapDistance(anchor.maxDistance);

                // Inspect limitlerini uygula
                orbit.SetAnchorInspect(anchor);

                // Anchor'ın seçtiği state'e geç
                if (GameManager.Instance != null)
                    GameManager.Instance.SetState(anchor.enterState);

                orbit.enabled = true;
            });
    }

    public void BackToFreelook()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameManager.GameState.Freelook);

        orbit.SetFreelook();
    }
}
