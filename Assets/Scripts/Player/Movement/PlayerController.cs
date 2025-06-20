using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 8f;
    public float slipSpeed = 30f;
    public float accelerationSpeed = 50f;
    public float gravity = -9.81f;

    [Header("Dash Settings")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private Vector3 currentVelocity;
    private Vector3 slopeNormal = Vector3.up;
    private float verticalVelocity = 0f;

    private CharacterController controller;
    private Vector3 lastMoveDirection = Vector3.forward;

    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    public bool IsDashing => isDashing;

    private PlayerCombat playerCombat;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    private void Update()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        HandleDashInput();
        HandleAttackInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleDashInput()
    {
        if (UserInput.instance == null) return;

        if (UserInput.instance.actionOneJustPressed && !isDashing && dashCooldownTimer <= 0f && lastMoveDirection.sqrMagnitude > 0.01f)
        {
            StartCoroutine(DashRoutine(lastMoveDirection.normalized));
        }
    }

    private void HandleAttackInput()
    {
        if (UserInput.instance == null || isDashing) return;  // Optional: block attacks while dashing

        if (UserInput.instance.actionTwoJustPressed)
        {
            playerCombat.DoAttack1();
        }
        if (UserInput.instance.actionThreeJustPressed)
        {
            playerCombat.DoAttack2();
        }
        if (UserInput.instance.actionFourJustPressed)
        {
            playerCombat.DoAttack3();
        }
    }

    private System.Collections.IEnumerator DashRoutine(Vector3 dashDirection)
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        float timer = 0f;
        float originalVerticalVelocity = verticalVelocity;

        while (timer < dashDuration)
        {
            Vector3 moveDelta = dashDirection * dashSpeed * Time.deltaTime;
            controller.Move(moveDelta);

            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        currentVelocity = dashDirection * speed;
        verticalVelocity = originalVerticalVelocity;
    }

    private void MovePlayer()
    {
        if (UserInput.instance == null || isDashing)
            return;

        Vector2 input = UserInput.instance.MoveInput;
        Vector3 inputDir = new Vector3(input.x, 0f, input.y).normalized;
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

    private bool IsGrounded(out RaycastHit hitInfo)
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, out hitInfo, 1.2f);
    }
}
