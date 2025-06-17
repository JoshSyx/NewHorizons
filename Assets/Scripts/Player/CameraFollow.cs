using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Orbit Settings")]
    public float distance = 5f;
    public float pitch = 30f;
    public float yaw = 0f;
    public float smoothTime = 0.2f;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 smoothedTargetPosition;

    private void Start()
    {
        if (target != null)
        {
            smoothedTargetPosition = target.position;
        }
    }

    private void Update()
    {
        if (target == null) return;

        pitch = Mathf.Clamp(pitch, -89f, 89f);

        smoothedTargetPosition = Vector3.SmoothDamp(smoothedTargetPosition, target.position, ref currentVelocity, smoothTime);

        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 offset = cameraRotation * new Vector3(0f, 0f, -distance);
        transform.position = smoothedTargetPosition + offset;

        transform.rotation = cameraRotation;
    }
}
