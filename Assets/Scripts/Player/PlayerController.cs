using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement, including dashing, acceleration, slope handling, and basic physics.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    /// <summary>Normal walking speed.</summary>
    public float speed = 8f;

    /// <summary>Speed used when there's no input (sliding effect).</summary>
    public float slipSpeed = 30f;

    /// <summary>Acceleration speed for smoothing velocity transitions.</summary>
    public float accelerationSpeed = 50f;

    /// <summary>Gravity force applied when airborne.</summary>
    public float gravity = -9.81f;

    /// <summary>Speed of the dash movement.</summary>
    public float dashSpeed = 25f;

    /// <summary>Duration the dash lasts.</summary>
    public float dashDuration = 0.2f;

    /// <summary>Cooldown period before the next dash can occur.</summary>
    public float dashCooldown = 1f;

    private Vector2 moveInput;
    private Vector3 currentVelocity;
    private Vector3 slopeNormal = Vector3.up;
    private float verticalVelocity = 0f;

    private CharacterController controller;
    private Vector3 lastMoveDirection = Vector3.forward;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    /// <summary>
    /// Indicates whether the player is currently dashing. Useful for external systems (e.g., camera).
    /// </summary>
    public bool IsDashing => isDashing;

    /// <summary>
    /// Initializes component references.
    /// </summary>
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Callback for movement input.
    /// </summary>
    /// <param name="context">Input context from the Input System.</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Callback for dash input. Triggers the dash if conditions are met.
    /// </summary>
    /// <param name="context">Input context from the Input System.</param>
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && !isDashing && dashCooldownTimer <= 0f && lastMoveDirection.sqrMagnitude > 0.01f)
        {
            StartCoroutine(DashRoutine());
        }
    }

    /// <summary>
    /// Handles fixed time-step updates, including cooldowns and player movement.
    /// </summary>
    private void FixedUpdate()
    {
        dashCooldownTimer -= Time.deltaTime;
        MovePlayer();
    }

    /// <summary>
    /// Coroutine that handles dash movement and termination logic.
    /// </summary>
    private System.Collections.IEnumerator DashRoutine()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        Vector3 dashDirection = lastMoveDirection.normalized;
        Vector3 lastPosition = transform.position;

        while (dashTimer > 0f)
        {
            Vector3 moveDelta = dashDirection * dashSpeed * Time.deltaTime;
            controller.Move(moveDelta);

            float movedDistance = (transform.position - lastPosition).magnitude;
            lastPosition = transform.position;

            // Early cancel if stuck against a wall
            if (movedDistance < moveDelta.magnitude * 0.2f)
            {
                break;
            }

            dashTimer -= Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    /// <summary>
    /// Handles movement, acceleration, gravity, and orientation.
    /// </summary>
    private void MovePlayer()
    {
        if (isDashing)
            return;

        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 moveDir = inputDir;

        if (IsGrounded(out RaycastHit hitInfo))
        {
            slopeNormal = hitInfo.normal;
            moveDir = Vector3.ProjectOnPlane(inputDir, slopeNormal).normalized;

            if (verticalVelocity < 0f)
                verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 horizontalTargetVelocity = moveDir * speed;
        float changeRate = (horizontalTargetVelocity.sqrMagnitude > 0.01f) ? accelerationSpeed : slipSpeed;
        currentVelocity = Vector3.MoveTowards(currentVelocity, horizontalTargetVelocity, changeRate * Time.deltaTime);

        Vector3 velocity = currentVelocity;
        velocity.y = verticalVelocity;

        Vector3 move = velocity * Time.deltaTime;
        controller.Move(move);

        if (inputDir.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveDir;
        }

        if (lastMoveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
        }
    }

    /// <summary>
    /// Checks whether the player is grounded using a raycast.
    /// </summary>
    /// <param name="hitInfo">Hit info from the raycast.</param>
    /// <returns>True if the player is grounded, false otherwise.</returns>
    private bool IsGrounded(out RaycastHit hitInfo)
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, out hitInfo, 1.2f);
    }
}
