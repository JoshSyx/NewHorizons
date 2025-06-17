using UnityEngine;

public class CameraTargetSync : MonoBehaviour
{
    public Rigidbody targetRigidbody;
    public float followSpeed = 50f;

    private void LateUpdate()
    {
        if (targetRigidbody != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetRigidbody.position, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRigidbody.rotation, followSpeed * Time.deltaTime);
        }
    }
}