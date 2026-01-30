using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MoveController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.6f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedStickVelocity = -2f;

    // Input state
    private Vector2 moveInput;
    private bool jumpPressed;

    // Movement/physics state
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private bool isGrounded;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            jumpPressed = true;
        }
    }

    private void Update()
    {
        // (1) Ground check update
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickVelocity;
        }

        // (2) Camera-relative planar move direction
        Vector3 camForward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 camRight = cameraTransform != null ? cameraTransform.right : transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward = camForward.sqrMagnitude > 0f ? camForward.normalized : Vector3.forward;
        camRight = camRight.sqrMagnitude > 0f ? camRight.normalized : Vector3.right;

        Vector3 desiredMoveDir = camForward * moveInput.y + camRight * moveInput.x;
        if (desiredMoveDir.sqrMagnitude > 1f)
        {
            desiredMoveDir.Normalize();
        }

        // (3) Horizontal velocity
        Vector3 desiredHorizontalVelocity = desiredMoveDir * moveSpeed;
        if (desiredMoveDir.sqrMagnitude > 0f)
        {
            float maxDelta = acceleration > 0f ? acceleration * Time.deltaTime : float.PositiveInfinity;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredHorizontalVelocity, maxDelta);
        }
        else
        {
            float maxDelta = deceleration > 0f ? deceleration * Time.deltaTime : float.PositiveInfinity;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, maxDelta);
        }

        // (4) Jump 처리 (one-shot)
        if (jumpPressed && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        jumpPressed = false;

        // (5) Gravity
        verticalVelocity += gravity * Time.deltaTime;

        // (6) Final move
        Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);

        // (7) End-of-frame cleanup
        jumpPressed = false;
    }
}
