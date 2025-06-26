using UnityEngine;

public class Projectile : MonoBehaviour
{
    private DamageData damageData;
    private GameObject owner;
    private Rigidbody rb;
    private TrailRenderer trail;

    private float stickDuration = 5f;
    private float knockbackForce = 3f;
    private bool hasHit = false;

    private float originalTrailTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();

        if (rb == null)
            Debug.LogWarning("Rigidbody missing on projectile.");

        if (trail != null)
            originalTrailTime = trail.time;
    }

    public void Initialize(DamageData data, GameObject source, Vector3 velocity, WeaponItem weapon)
    {
        damageData = data;
        owner = source;

        if (rb != null)
        {
            rb.useGravity = weapon.useGravity;
            rb.linearVelocity = velocity;
        }

        stickDuration = weapon.stickDuration;
        knockbackForce = weapon.knockbackForce;

        Destroy(gameObject, weapon.maxLifetime);
    }

    private void Update()
    {
        if (!hasHit)
        {
            if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
            }
        }
        else
        {
            if (trail != null)
            {
                // Stop emitting new trail points
                trail.emitting = false;

                // Fade out the trail over stickDuration seconds
                trail.time = Mathf.MoveTowards(trail.time, 0f, Time.deltaTime * (originalTrailTime / stickDuration));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Ignore collisions with owner and children
        if (other.transform.IsChildOf(owner.transform))
            return;

        if (other.TryGetComponent(out Health health))
        {
            Vector3 knockbackDir = (other.transform.position - owner.transform.position).normalized;
            health.InflictDamage(damageData, knockbackDir, knockbackForce);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        hasHit = true;
        transform.parent = other.transform;

        if (trail != null)
            trail.emitting = false;

        Destroy(gameObject, stickDuration);
    }
}
