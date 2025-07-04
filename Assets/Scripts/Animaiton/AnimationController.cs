using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class AnimationController : MonoBehaviour
{
    [SerializeField] private string moveX_name = "MoveX";
    [SerializeField] private string moveZ_name = "MoveZ";

    [SerializeField] private float dampTime = 0.1f;
    [SerializeField] private float movementThreshold = 0.05f;

    private Animator animator;
    private CharacterController characterController;
    private Vector3 lastPosition;

    private PlayerController playerController; // Reference to get LastManualForward

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        velocity.y = 0f;

        if (velocity.magnitude < movementThreshold)
        {
            velocity = Vector3.zero;
        }

        Vector3 localVelocity = Vector3.zero;

        if (playerController != null)
        {
            if (playerController.LastManualForward != Vector3.zero)
            {
                Quaternion intendedRotation = Quaternion.LookRotation(playerController.LastManualForward, Vector3.up);
                localVelocity = Quaternion.Inverse(intendedRotation) * velocity;
            }
            else
            {
                // Fallback: use transform's own orientation
                localVelocity = transform.InverseTransformDirection(velocity);
                Debug.LogWarning($"{gameObject.name}: LastManualForward was zero. Falling back to local velocity.");
            }
        }
        else
        {
            localVelocity = transform.InverseTransformDirection(velocity);
            Debug.LogWarning($"{gameObject.name}: PlayerController not found. Using local velocity.");
        }

        // Ensure velocity is valid before applying to animator
        if (IsVectorValid(localVelocity))
        {
            animator.SetFloat(moveX_name, localVelocity.x, dampTime, Time.deltaTime);
            animator.SetFloat(moveZ_name, localVelocity.z, dampTime, Time.deltaTime);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: localVelocity was invalid (NaN). Skipping animator update.");
        }
    }

    private bool IsVectorValid(Vector3 v)
    {
        return !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z);
    }
}
