using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 8f;
    public float gravity = -9.81f;

    [Header("Dash Settings")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private float verticalVelocity = 0f;

    private CharacterController controller;
    private Vector3 lastMoveDirection = Vector3.forward;

    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    private PlayerCombat playerCombat;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    private void FixedUpdate()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (!isDashing)
            HandleMovement();
    }

    private void Update()
    {
        HandleActionInputs();
    }

    private void HandleMovement()
    {
        Vector2 input = UserInput.Instance.MoveInput;
        Vector3 inputDir = new Vector3(input.x, 0f, input.y).normalized;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = inputDir;
        }

        Vector3 move = inputDir * speed;
        move.y = verticalVelocity;

        verticalVelocity += gravity * Time.deltaTime;
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -1f;

        controller.Move(move * Time.deltaTime);

        if (lastMoveDirection.sqrMagnitude > 0.01f)
        {
            bool lastUsedIsRanged = false;

            if (playerCombat != null)
            {
                WeaponItem lastWeapon = playerCombat.LastUsedWeapon;
                lastUsedIsRanged = lastWeapon != null && !lastWeapon.IsMelee;
            }

            Quaternion targetRotation;

            if (lastUsedIsRanged)
            {
                GameObject aimTarget = playerCombat.GetAimAssistTarget();

                if (aimTarget != null)
                {
                    Vector3 toEnemy = (aimTarget.transform.position - transform.position).normalized;

                    Vector3 moveDirFlat = lastMoveDirection;
                    moveDirFlat.y = 0f;
                    moveDirFlat.Normalize();

                    Vector3 enemyDirFlat = toEnemy;
                    enemyDirFlat.y = 0f;
                    enemyDirFlat.Normalize();

                    float angleBetween = Vector3.SignedAngle(moveDirFlat, enemyDirFlat, Vector3.up);
                    float maxOffset = 50f; // max partial rotation angle

                    float clampedAngle = Mathf.Clamp(angleBetween, -maxOffset, maxOffset);

                    // Rotate move direction partially toward enemy direction
                    Vector3 finalDir = Quaternion.Euler(0f, clampedAngle, 0f) * moveDirFlat;

                    targetRotation = Quaternion.LookRotation(finalDir);
                }
                else
                {
                    targetRotation = Quaternion.LookRotation(lastMoveDirection);
                }
            }
            else
            {
                // If last used weapon wasn't ranged, rotate fully toward movement direction
                targetRotation = Quaternion.LookRotation(lastMoveDirection);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
        }
    }



    private void HandleActionInputs()
    {
        if (UserInput.Instance == null) return;

        // Pickup priority
        if (ItemPickup.PlayerIsInPickupRange)
        {
            if (UserInput.Instance.ActionOneJustPressed)
            {
                ItemPickup.PickupWeaponAtRange(WeaponSlot.Melee);
                return;
            }
            if (UserInput.Instance.ActionTwoJustPressed)
            {
                ItemPickup.PickupWeaponAtRange(WeaponSlot.Primary);
                return;
            }
            if (UserInput.Instance.ActionThreeJustPressed)
            {
                ItemPickup.PickupWeaponAtRange(WeaponSlot.Special);
                return;
            }
            if (UserInput.Instance.ActionFourJustPressed)
            {
                ItemPickup.PickupWeaponAtRange(WeaponSlot.Dash);
                return;
            }
        }

        if (isDashing) return;

        if (UserInput.Instance.ActionOneJustPressed)
            TryPerformAction(WeaponSlot.Melee);

        if (UserInput.Instance.ActionTwoJustPressed)
            TryPerformAction(WeaponSlot.Primary);

        if (UserInput.Instance.ActionThreeJustPressed)
            TryPerformAction(WeaponSlot.Special);

        if (UserInput.Instance.ActionFourJustPressed)
            TryPerformAction(WeaponSlot.Dash);
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
            if (dashCooldownTimer <= 0f && lastMoveDirection.sqrMagnitude > 0.01f)
            {
                StartCoroutine(DashRoutine(lastMoveDirection.normalized));
            }
        }
        else
        {
            playerCombat.AttackWithSlot(slot);
        }
    }

    private IEnumerator DashRoutine(Vector3 dashDirection)
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        float timer = 0f;

        while (timer < dashDuration)
        {
            Vector3 moveDelta = dashDirection * dashSpeed * Time.deltaTime;
            controller.Move(moveDelta);

            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}
