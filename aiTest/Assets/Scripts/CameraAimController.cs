using UnityEngine;

public class CameraAimController : MonoBehaviour
{
    [SerializeField] private AimStateChannel aimState;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera aimCamera;

    private void Start()
    {
        if (aimState == null || mainCamera == null || aimCamera == null)
        {
            Debug.LogWarning($"[{nameof(CameraAimController)}] Missing required reference(s). aimState, mainCamera, or aimCamera is null.");
            return;
        }
    }

    private void OnEnable()
    {
        if (aimState == null || mainCamera == null || aimCamera == null)
        {
            Debug.LogWarning($"[{nameof(CameraAimController)}] Missing required reference(s). aimState, mainCamera, or aimCamera is null.");
            return;
        }

        aimState.AimingChanged += HandleAimingChanged;
        HandleAimingChanged(aimState.IsAiming);
    }

    private void OnDisable()
    {
        if (aimState == null) return;
        aimState.AimingChanged -= HandleAimingChanged;
        SetCameraState(false);
    }

    private void HandleAimingChanged(bool isAiming)
    {
        SetCameraState(isAiming);
    }

    private void SetCameraState(bool isAiming)
    {
        if (mainCamera == null || aimCamera == null)
            return;

        mainCamera.enabled = !isAiming;
        aimCamera.enabled = isAiming;
    }
}
