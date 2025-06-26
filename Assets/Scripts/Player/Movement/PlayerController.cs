using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 8f;
    public float gravity = -9.81f;
    public float rotationSmoothTime = 0.2f;

    private float verticalVelocity = 0f;
    private CharacterController controller;
    private Vector3 lastMoveDirection = Vector3.forward;

    private PlayerCombat playerCombat;
    private PlayerDash playerDash;
    public PlayerInput playerInput; // Reference to PlayerInput

    private Vector3 lastMousePosition;
    private float mouseMoveThreshold = 1.0f; // adjust as needed
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerCombat = GetComponent<PlayerCombat>();
        playerDash = GetComponent<PlayerDash>();
    }

    private void FixedUpdate()
    {
        if (playerDash == null || !playerDash.IsDashing)
        {
            HandleMovement();
        }
    }

    private void Update()
    {
        HandleActionInputs();
        Debug.Log(Mouse.current.position.ReadValue());

    }

    private void HandleMovement()
    {
        Vector2 moveInput = UserInput.Instance.MoveInput;
        Vector2 aimInput = UserInput.Instance.AimInput;

        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = inputDir;
        }

        Vector3 move = inputDir * speed;
        move.y = verticalVelocity;

        verticalVelocity += gravity * Time.deltaTime;
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -1f;
        }

        controller.Move(move * Time.deltaTime);

        HandleRotation(aimInput);
    }

    private void HandleRotation(Vector2 aimInput)
    {
        string scheme = playerInput.currentControlScheme;

        // --- Mouse aiming ---
        Vector3 currentMousePos = Input.mousePosition;
        if ((currentMousePos - lastMousePosition).sqrMagnitude > mouseMoveThreshold)
        {
            playerInput.SwitchCurrentControlScheme("Keyboard&Mouse");
        }
        lastMousePosition = currentMousePos;

        if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Ray ray = Camera.main.ScreenPointToRay(currentMousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
            {
                Vector3 direction = hit.point - transform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime);
                }
            }
            else
            {
                Debug.LogWarning("[Rotation] Raycast from mouse did NOT hit ground.");
            }

            return;
        }

        // --- Gamepad aiming ---
        if (scheme == "Gamepad")
        {
            Vector3 aimDir = new Vector3(aimInput.x, 0f, aimInput.y);
            if (aimDir.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(aimDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime);
            }
            return;
        }

        // --- Default fallback to movement direction ---
        if (lastMoveDirection.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lastMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothTime);
        }
    }

    private void HandleActionInputs()
    {
        if (UserInput.Instance == null) return;

        // Attempt pickup if player is in range and presses any action button
        if (ItemPickup.PlayerIsInPickupRange)
        {
            if (UserInput.Instance.ActionOneJustPressed ||
                UserInput.Instance.ActionTwoJustPressed ||
                UserInput.Instance.ActionThreeJustPressed ||
                UserInput.Instance.ActionFourJustPressed ||
                UserInput.Instance.ActionFiveJustPressed ||
                UserInput.Instance.ActionSixJustPressed)
            {
                ItemPickup.PickupWeaponAtRange(); // Auto-pickup based on WeaponItem.slot
                return;
            }
        }

        // Skip action inputs while dashing
        if (playerDash != null && playerDash.IsDashing) return;

        // Handle action inputs
        if (UserInput.Instance.ActionOneJustPressed) TryPerformAction(WeaponSlot.Action1);
        if (UserInput.Instance.ActionTwoJustPressed) TryPerformAction(WeaponSlot.Action2);
        if (UserInput.Instance.ActionThreeJustPressed) TryPerformAction(WeaponSlot.Action3);
        if (UserInput.Instance.ActionFourJustPressed) TryPerformAction(WeaponSlot.Action4);
        if (UserInput.Instance.ActionFiveJustPressed) TryPerformAction(WeaponSlot.Action5);
        if (UserInput.Instance.ActionSixJustPressed) TryPerformAction(WeaponSlot.Action6);
    }

    private void TryPerformAction(WeaponSlot slot)
    {
        var weapon = PlayerInventory.Instance.GetEquippedWeapon(slot);
        if (weapon == null)
        {
            Debug.LogWarning($"No weapon equipped in slot: {slot}");
            return;
        }

        if (weapon is WeaponItem weaponItem && weaponItem.IsDashAbility)
        {
            playerDash?.TryDash(lastMoveDirection.normalized);
        }
        else
        {
            playerCombat?.AttackWithSlot(slot);
        }
    }
}
