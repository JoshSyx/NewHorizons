using System.Collections;
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
    private Vector3 velocityAtHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();

        if (rb == null)
            Debug.LogWarning("Rigidbody missing on projectile.");

        if (trail != null)
            originalTrailTime = trail.time;
    }

    // Use interface IRangedItem to support WeaponItem, AbilityItem, etc.
    public void Initialize(DamageData data, GameObject source, Vector3 velocity, IRangedItem rangedItem)
    {
        damageData = data;
        owner = source;
        velocityAtHit = velocity;

        if (rb != null)
        {
            rb.useGravity = rangedItem.useGravity;
            rb.linearVelocity = velocity;
        }

        stickDuration = rangedItem.stickDuration;
        knockbackForce = rangedItem.knockbackForce;

        Destroy(gameObject, rangedItem.maxLifetime);
    }

    private void Update()
    {
        if (!hasHit && rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
        else if (hasHit && trail != null)
        {
            trail.emitting = false;
            trail.time = Mathf.MoveTowards(trail.time, 0f, Time.deltaTime * (originalTrailTime / stickDuration));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (owner == null || other.transform.IsChildOf(owner.transform)) return;

        GameObject target = other.transform.root.gameObject;

        if (target == null) return;

        Vector3 targetPosition = target.transform.position;

        Combat ownerCombat = owner.GetComponent<Combat>();
        if (ownerCombat != null)
        {
            Vector3 knockbackDir = (targetPosition - transform.position).normalized;
            ownerCombat.ApplyDamage(target, damageData, knockbackDir, knockbackForce);
        }
        else
        {
            Debug.LogWarning("Projectile's owner has no Combat component.");
        }

        StartCoroutine(StickAndDestroyDelayed(other));
    }

    private IEnumerator StickAndDestroyDelayed(Collider other)
    {
        yield return null; // wait a frame so target destruction completes
        StickToTarget(other);
        Destroy(gameObject, stickDuration);
    }


    private void StickToTarget(Collider target)
    {
        hasHit = true;

        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.isKinematic = true;
        }

        transform.parent = target.transform;

        if (trail != null)
        {
            trail.emitting = false;
        }
    }
}
