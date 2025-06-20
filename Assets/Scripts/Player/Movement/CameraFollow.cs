using UnityEngine;

/// <summary>
/// Makes the camera follow a target with smooth dampening and orbit-like rotation control.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    /// <summary>
    /// The target the camera will follow.
    /// </summary>
    [SerializeField] private Transform target;

    [Header("Orbit Settings")]
    /// <summary>
    /// Distance from the target to the camera.
    /// </summary>
    [SerializeField] private float distance = 5f;

    /// <summary>
    /// Vertical angle (pitch) of the camera in degrees.
    /// </summary>
    [SerializeField] private float pitch = 30f;

    /// <summary>
    /// Horizontal angle (yaw) of the camera in degrees.
    /// </summary>
    [SerializeField] private float yaw = 0f;

    [Header("Smoothing")]
    /// <summary>
    /// Time taken to smooth the camera's follow movement.
    /// </summary>
    [SerializeField] private float smoothTime = 0.2f;

    /// <summary>
    /// Speed at which the camera's rotation is smoothed.
    /// </summary>
    [SerializeField] private float rotationSmoothSpeed = 5f;

    /// <summary>
    /// Multiplier for how far ahead the camera should look based on target speed.
    /// </summary>
    [SerializeField] private float lookAheadMultiplier = 0.5f;

    /// <summary>
    /// Maximum distance the camera can look ahead based on target movement.
    /// </summary>
    [SerializeField] private float maxLookAheadDistance = 3f;

    /// <summary>
    /// Internal velocity tracker for SmoothDamp.
    /// </summary>
    private Vector3 currentVelocity = Vector3.zero;

    /// <summary>
    /// The current smoothed position of the camera's follow target.
    /// </summary>
    private Vector3 smoothedTargetPosition;

    /// <summary>
    /// The position of the target in the previous frame.
    /// </summary>
    private Vector3 previousTargetPosition;

    /// <summary>
    /// Offset applied to the target position to anticipate movement.
    /// </summary>
    private Vector3 lookAheadOffset = Vector3.zero;

    /// <summary>
    /// Smoothed vertical camera angle.
    /// </summary>
    private float currentPitch;

    /// <summary>
    /// Smoothed horizontal camera angle.
    /// </summary>
    private float currentYaw;

    /// <summary>
    /// Initializes camera position and rotation.
    /// </summary>
    private void Start()
    {
        if (target != null)
        {
            smoothedTargetPosition = target.position;
            previousTargetPosition = target.position;
        }

        currentPitch = pitch;
        currentYaw = yaw;
    }

    /// <summary>
    /// Updates camera position and rotation at fixed intervals for smooth physics-based following.
    /// </summary>
    private void FixedUpdate()
    {
        if (target == null) return;

        pitch = Mathf.Clamp(pitch, -89f, 89f);

        // Calculate target velocity and look-ahead offset
        Vector3 targetDelta = target.position - previousTargetPosition;
        Vector3 targetVelocity = targetDelta / Time.fixedDeltaTime;
        previousTargetPosition = target.position;

        float speed = targetVelocity.magnitude;

        // Predictive look-ahead based on movement
        Vector3 desiredLookAhead = Vector3.ClampMagnitude(
            targetVelocity.normalized * lookAheadMultiplier * speed,
            maxLookAheadDistance
        );
        lookAheadOffset = Vector3.Lerp(lookAheadOffset, desiredLookAhead, Time.fixedDeltaTime * 3f);

        // Smooth follow position
        smoothedTargetPosition = Vector3.SmoothDamp(
            smoothedTargetPosition,
            target.position + lookAheadOffset,
            ref currentVelocity,
            smoothTime
        );

        // Smooth rotation
        currentPitch = Mathf.LerpAngle(currentPitch, pitch, rotationSmoothSpeed * Time.deltaTime);
        currentYaw = Mathf.LerpAngle(currentYaw, yaw, rotationSmoothSpeed * Time.deltaTime);

        // Apply rotation and position
        Quaternion cameraRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 offset = cameraRotation * new Vector3(0f, 0f, -distance);

        transform.position = smoothedTargetPosition + offset;
        transform.rotation = cameraRotation;
    }
}
