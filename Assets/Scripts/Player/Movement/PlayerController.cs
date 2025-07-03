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
    public PlayerInput playerInput;

    private Vector3 lastManualForward = Vector3.forward; // Default

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
        HandleDashInput();
        HandleActionInputs();
        HandleShooting();
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

        HandleRotation(aimInput, moveInput);
    }

    private void HandleRotation(Vector2 aimInput, Vector2 moveInput)
    {
        string scheme = playerInput != null ? playerInput.currentControlScheme : "Keyboard&Mouse";

        GameObject aimAssistTarget = playerCombat.GetAimAssistTarget();
        if (aimAssistTarget != null)
        {
            Vector3 dirToTarget = aimAssistTarget.transform.position - transform.position;
            dirToTarget.y = 0f;

            if (dirToTarget.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothTime);
                // Don't update lastManualForward here because this is aim assist rotation
                return; // highest priority, exit early
            }
        }

        if (aimInput.sqrMagnitude > 0.01f)
        {
            if (scheme == "Keyboard&Mouse")
            {
                Ray ray = Camera.main.ScreenPointToRay(aimInput);
                if (Physics.Raycast(ray, out RaycastHit hit, 10000f))
                {
                    Vector3 dir = hit.point - transform.position;
                    dir.y = 0f;

                    if (dir.sqrMagnitude > 0.1f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(dir);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothTime);
                        lastManualForward = dir.normalized; // store manual forward
                        return;
                    }
                }
            }
            else
            {
                Vector3 aimDir = new Vector3(aimInput.x, 0f, aimInput.y);
                if (aimDir.sqrMagnitude > 0.1f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(aimDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothTime);
                    lastManualForward = aimDir.normalized; // store manual forward
                    return;
                }
            }
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothTime);
            lastManualForward = moveDir.normalized; // store manual forward
        }
    }

    private void HandleActionInputs()
    {
        if (UserInput.Instance == null) return;

        if (ItemPickup.PlayerIsInPickupRange)
        {
            if (AnyActionJustPressed())
            {
                ItemPickup.PickupWeaponAtRange();
                return;
            }
        }

        // Only SELECT slot on button press
        CheckAndHandleActionInput(Slot.Action1, UserInput.Instance.ActionOneJustPressed);
        CheckAndHandleActionInput(Slot.Action2, UserInput.Instance.ActionTwoJustPressed);
        CheckAndHandleActionInput(Slot.Action3, UserInput.Instance.ActionThreeJustPressed);
        CheckAndHandleActionInput(Slot.Action4, UserInput.Instance.ActionFourJustPressed);
        CheckAndHandleActionInput(Slot.Action5, UserInput.Instance.ActionFiveJustPressed);
    }
    private void CheckAndHandleActionInput(Slot slot, bool justPressed)
    {
        if (!justPressed) return;

        var item = PlayerInventory.Instance.GetEquippedItem(slot);
        if (item == null)
        {
            SelectSlot(slot); // Show UI even if empty
            return;
        }

        bool isContinuousFire = false;

        if (item is AbilityItem ability)
            isContinuousFire = ability.IsContinuousFire;
        else if (item is WeaponItem weapon)
            isContinuousFire = weapon.IsContinuousFire;

        if (isContinuousFire)
            SelectSlot(slot); // Continuous items only select the slot (held trigger will shoot)
        else
            TryPerformAction(slot, true); // Instant-fire weapons fire immediately
    }

    private void SelectSlot(Slot slot)
    {
        PlayerInventory.Instance.SetLastSelectedSlot(slot);

        OverlayManager overlay = OverlayManager.Instance;
        if (overlay != null)
        {
            overlay.ShowActiveSlot(slot);
        }
    }


    private void TryPerformAction(Slot slot, bool pressed)
    {
        if (!pressed) return;
        PlayerInventory.Instance.SetLastSelectedSlot(slot);

        var overlay = OverlayManager.Instance;
        if (overlay != null)
        {
            overlay.ShowActiveSlot(slot);
        }

        var item = PlayerInventory.Instance.GetEquippedItem(slot);
        if (item == null)
        {
            Debug.LogWarning($"No item equipped in slot: {slot}");
            return;
        }

        if (item is AbilityItem ability && ability.abilityType == AbilityType.Dash)
        {
            playerDash?.TryDash(lastMoveDirection.normalized);
        }
        else
        {
            playerCombat?.AttackWithSlot(slot);
        }
    }


    private bool AnyActionJustPressed()
    {
        var input = UserInput.Instance;
        return input.ActionOneJustPressed || input.ActionTwoJustPressed ||
               input.ActionThreeJustPressed || input.ActionFourJustPressed ||
               input.ActionFiveJustPressed || input.ActionSixJustPressed;
    }

    private void HandleShooting()
    {
        var overlay = OverlayManager.Instance;

        Slot lastSlot = PlayerInventory.Instance.GetLastSelectedSlot();
        var item = PlayerInventory.Instance.GetEquippedItem(lastSlot);
        if (item == null)
        {
            playerCombat.StopShooting();
            return;
        }

        if (overlay != null)
        {
            overlay.ShowActiveSlot(lastSlot);
        }

        // Check if item is AbilityItem or WeaponItem, and get IsContinuousFire flag
        bool isContinuousFire = false;
        bool isDashAbility = false;

        if (item is AbilityItem ability)
        {
            isContinuousFire = ability.IsContinuousFire;
            isDashAbility = ability.abilityType == AbilityType.Dash;
        }
        else if (item is WeaponItem weapon)
        {
            isContinuousFire = weapon.IsContinuousFire;
        }

        // Handle dash ability separately
        if (isDashAbility)
        {
            if (UserInput.Instance.LeftTriggerJustPressed)
            {
                playerDash?.TryDash(lastMoveDirection.normalized);
            }
            playerCombat.StopShooting();
            return;
        }

        // Handle shooting for weapons and other abilities
        if (UserInput.Instance.LeftTriggerJustPressed || (isContinuousFire && UserInput.Instance.LeftTriggerBeingHeld))
        {
            if (item is WeaponItem weaponItem && weaponItem.IsRanged)
            {
                playerCombat.StartShootingWithWeapon(weaponItem);

                // If not continuous fire, stop immediately after shooting once
                if (!isContinuousFire)
                {
                    playerCombat.StopShooting();
                }
            }
            else
            {
                playerCombat.AttackWithSlot(lastSlot);

                // Stop shooting if not continuous fire
                if (!isContinuousFire)
                {
                    playerCombat.StopShooting();
                }
            }
        }
        else if (isContinuousFire && !UserInput.Instance.LeftTriggerBeingHeld)
        {
            playerCombat.StopShooting();
        }
        else if (!isContinuousFire)
        {
            playerCombat.StopShooting();
        }
    }

    private void HandleDashInput()
    {
        if (UserInput.Instance.DashTriggered)
        {
            Vector3 dashWorldDir = UserInput.Instance.GetWorldDashDirection();
            playerDash?.TryDash(dashWorldDir);
        }
    }
}
