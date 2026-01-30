using UnityEngine;

public class WeaponAimOffsetController : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] private AimStateChannel aimState;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private Transform aimSocket;

    [Header("Position")]
    [SerializeField] private Vector3 hipLocalPos;
    [SerializeField] private float posLerpSpeed = 12f;

    [Header("Options")]
    [SerializeField] private bool captureHipLocalPosOnStart = true;

    // 내부 상태
    private Vector3 targetLocalPos;
    private bool isValid;

    private void Start()
    {
        if (weaponSocket == null || aimSocket == null || aimState == null)
        {
            Debug.LogWarning("[WeaponAimOffsetController] Missing required reference.");
            isValid = false;
            return;
        }

        if (captureHipLocalPosOnStart)
        {
            hipLocalPos = weaponSocket.localPosition;
        }

        targetLocalPos = weaponSocket.localPosition;
        isValid = true;
    }

    private void OnEnable()
    {
        if (aimState == null)
        {
            Debug.LogWarning("[WeaponAimOffsetController] AimStateChannel is null.");
            return;
        }

        aimState.AimingChanged += HandleAimingChanged;

        // 즉시 1회 동기화
        HandleAimingChanged(aimState.IsAiming);
    }

    private void OnDisable()
    {
        if (aimState != null)
        {
            aimState.AimingChanged -= HandleAimingChanged;
        }
    }

    private void HandleAimingChanged(bool isAiming)
    {
        if (!isValid)
            return;

        targetLocalPos = isAiming ? aimSocket.localPosition : hipLocalPos;
    }

    private void Update()
    {
        if (!isValid)
            return;

        weaponSocket.localPosition = Vector3.Lerp(
            weaponSocket.localPosition,
            targetLocalPos,
            Time.deltaTime * posLerpSpeed
        );
    }
}
