using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private CharacterController controller;
    private Vector3 lastDashDirection;

    public bool IsDashing => isDashing;
    public bool CanDash => !isDashing && dashCooldownTimer <= 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    public void TryDash(Vector3 direction)
    {
        if (!CanDash || direction.sqrMagnitude < 0.01f) return;

        lastDashDirection = direction.normalized;
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        float timer = 0f;

        while (timer < dashDuration)
        {
            Vector3 moveDelta = lastDashDirection * dashSpeed * Time.deltaTime;
            controller.Move(moveDelta);
            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}
