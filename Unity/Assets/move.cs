using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Movement : NetworkBehaviour
{
    public float moveSpeed = 7f;
    public float rotateSpeed = 100f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!isLocalPlayer)
        {
            rb.isKinematic = true;
            this.enabled = false;
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    float h;

    void Update()
    {
        if (!isLocalPlayer) return;

        h = Input.GetAxis("Horizontal");

        Quaternion turn = Quaternion.Euler(0f, h * rotateSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * turn);
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        float v = Input.GetAxis("Vertical");

        Vector3 movement = transform.forward * v * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
    }
    
}
