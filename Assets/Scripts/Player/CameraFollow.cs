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
    private Vector3 currentPosition;

    private void LateUpdate()
    {
        if (target == null) return;

        pitch = Mathf.Clamp(pitch, -89f, 89f);

        Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = orbitRotation * new Vector3(0, 0, -distance);

        Vector3 desiredPosition = target.position + offset;

        currentPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        transform.position = currentPosition;

        transform.LookAt(target.position);
    }
}
