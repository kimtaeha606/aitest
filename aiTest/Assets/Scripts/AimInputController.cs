using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// AimInputController
/// 역할:
/// - 플레이어의 조준 입력을 받는다
/// - 조준 상태 ON/OFF를 판단한다
/// - 결과를 "조준 상태 채널"로 전달한다
///
/// 아는 것:
/// - 조준 상태를 "쓸 수 있는 창구"(채널)
///
/// 모르는 것(절대 참조/호출하지 않음):
/// - 카메라 / 무기 / UI / 사운드
/// </summary>
public sealed class AimInputController : MonoBehaviour
{
    [Header("Aim State Channel (Output)")]
    [Tooltip("조준 상태를 외부로 브로드캐스트하는 채널(창구).")]
    [SerializeField] private BoolEventChannelSO aimStateChannel;

    [Header("Options")]
    [Tooltip("Hold(누르고 있는 동안 조준) 모드. 꺼두면 Toggle(누를 때마다 전환) 모드.")]
    [SerializeField] private bool holdToAim = true;

    [Tooltip("비활성화될 때 조준 상태를 강제로 OFF로 내릴지 여부.")]
    [SerializeField] private bool forceOffOnDisable = true;

    private bool _isAiming;

    /// <summary>현재 조준 상태(읽기 전용)</summary>
    public bool IsAiming => _isAiming;

    /// <summary>
    /// Input System 콜백 전용.
    /// - 이 함수는 "입력 → 상태 판정 → 채널 전송"만 한다.
    /// - 조준 상태를 필요로 하는 시스템(카메라/무기/등)은 채널을 구독해서 반응한다.
    /// </summary>
    public void OnAim(InputAction.CallbackContext ctx)
    {
        if (holdToAim)
        {
            // Hold 모드: Press(Performed)=ON, Release(Canceled)=OFF
            if (ctx.performed) SetAiming(true);
            else if (ctx.canceled) SetAiming(false);
        }
        else
        {
            // Toggle 모드: performed 순간에 토글
            if (ctx.performed) SetAiming(!_isAiming);
        }
    }

    private void OnDisable()
    {
        if (!forceOffOnDisable) return;

        // 오브젝트 비활성화/파괴 시 조준 상태가 남지 않도록 OFF 보장
        if (_isAiming) SetAiming(false);
    }

    private void SetAiming(bool value)
    {
        if (_isAiming == value) return;

        _isAiming = value;

        // 조준 상태 채널로 전달 (구독자에게 브로드캐스트)
        if (aimStateChannel != null)
            aimStateChannel.Raise(_isAiming);
    }
}

/// <summary>
/// "조준 상태 채널(쓸 수 있는 창구)" 최소 구현.
/// - AimInputController는 이 타입만 알고, 구독자는 이 채널을 통해 상태를 받는다.
/// - 프로젝트에 이미 채널 시스템이 있다면, 이 파일에서 아래 클래스를 제거하고
///   기존 채널 타입(BoolEventChannel 등)으로 aimStateChannel 타입만 바꾸면 된다.
/// </summary>
[CreateAssetMenu(menuName = "Channels/Bool Event Channel", fileName = "BoolEventChannel")]
public sealed class BoolEventChannelSO : ScriptableObject
{
    public event System.Action<bool> OnRaised;

    public void Raise(bool value)
    {
        OnRaised?.Invoke(value);
    }
}
