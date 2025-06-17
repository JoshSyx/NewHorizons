using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    public float speed = 8f;
    public float slipSpeed = 30f;
    public float accelerationSpeed = 50f;

    private Vector2 moveInput;
    private Vector3 currentVelocity;
    private Vector3 slopeNormal = Vector3.up;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        Ray ray = new Ray(rb.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1f))
        {
            slopeNormal = hitInfo.normal;

            Vector3 moveDir = Vector3.ProjectOnPlane(inputDir, slopeNormal).normalized;

            Vector3 targetVelocity = moveDir * speed;
            float changeRate = (targetVelocity.sqrMagnitude > 0.01f) ? accelerationSpeed : slipSpeed;

            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, changeRate * Time.deltaTime);

            Vector3 moveDelta = new Vector3(currentVelocity.x, 0f, currentVelocity.z) * Time.deltaTime;
            rb.MovePosition(rb.position + moveDelta);

            Vector3 groundedPosition = new Vector3(rb.position.x, hitInfo.point.y, rb.position.z);
            rb.MovePosition(groundedPosition);

            if (moveInput.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 0.15f));
            }
        }
    }

}
