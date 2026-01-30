using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// FPS Look Controller (Input System callback context)
/// - Rotates a camera pivot transform (yaw + pitch)
/// - Hides  locks the cursor while playing
/// - Pitch clamped to [-90, 90]
/// </summary>
public class LookController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot; // assign in Inspector

    [Header("Settings")]
    [SerializeField] private float sensitivity = 2.0f;
    [SerializeField] private bool invertY = false;

    [Header("Pitch Clamp")]
    [SerializeField] private float minPitch = -90f;
    [SerializeField] private float maxPitch = 90f;

    private float yaw;
    private float pitch;
    private Vector2 lookDelta;

    private void Awake()
    {
        ApplyCursorLock(true);

        // Initialize from current pivot rotation
        if (cameraPivot != null)
        {
            Vector3 e = cameraPivot.eulerAngles;
            yaw = e.y;

            // Convert 0..360 to -180..180 for pitch
            float p = e.x;
            if (p > 180f) p -= 360f;
            pitch = Mathf.Clamp(p, minPitch, maxPitch);

            cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    private void OnEnable()
    {
        ApplyCursorLock(true);
    }

    private void OnDisable()
    {
        ApplyCursorLock(false);
    }

    private void Update()
    {
        if (cameraPivot == null) return;

        // Optional: Esc toggles cursor lock
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            bool willLock = Cursor.lockState != CursorLockMode.Locked;
            ApplyCursorLock(willLock);
        }

        if (Cursor.lockState != CursorLockMode.Locked) return;

        yaw += lookDelta.x * sensitivity;

        float ySign = invertY ? 1f : -1f;
        pitch += lookDelta.y * sensitivity * ySign;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            lookDelta = context.ReadValue<Vector2>();
            return;
        }

        if (context.canceled)
        {
            lookDelta = Vector2.zero;
        }
    }

    private static void ApplyCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
