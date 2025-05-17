using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : NetworkBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public int damage = 10;

    [Header("VFX")]
    public GameObject hitVFX;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hitVFX != null)
        {
            ContactPoint contact = collision.contacts[0];
            Instantiate(hitVFX, contact.point, Quaternion.LookRotation(contact.normal));
        }

        if (!isServer) return;

        Health targetHealth = collision.gameObject.GetComponent<Health>();
        if (targetHealth != null && !targetHealth.IsInvincible())
        {
            targetHealth.TakeDamage(damage);
        }

        NetworkServer.Destroy(gameObject);
    }
}
