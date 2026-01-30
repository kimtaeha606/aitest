// AimInputController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class AimInputController : MonoBehaviour
{
    [Header("Required")]
    public AimStateChannel aimState;

    [Header("Options")]
    public bool toggleMode = false;
    public bool startAiming = false;

    private void Awake()
    {
        if (aimState == null)
        {
            Debug.LogWarning("[AimInputController] Missing reference: aimState (AimStateChannel).", this);
            return;
        }

        aimState.SetAiming(startAiming);
    }

    // Input System 콜백으로만 사용 (공유 이름 고정)
    public void OnAim(InputAction.CallbackContext context)
    {
        if (aimState == null)
        {
            Debug.LogWarning("[AimInputController] Missing reference: aimState (AimStateChannel).", this);
            return;
        }

        if (!toggleMode)
        {
            // 홀드: 눌렀을 때 ON, 뗐을 때 OFF
            if (context.performed)
            {
                aimState.SetAiming(true);
            }
            else if (context.canceled)
            {
                aimState.SetAiming(false);
            }
        }
        else
        {
            // 토글: 눌렀을 때만 ON/OFF 전환
            if (context.performed)
            {
                aimState.SetAiming(!aimState.IsAiming);
            }
        }
    }
}
