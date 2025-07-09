using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Movement : NetworkBehaviour
{
    public float moveSpeed = 7f;
    public float rotateSpeed = 50;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    float moveInput;
    float rotateInput;

    void Update()
    {
        if (!isLocalPlayer) return;

        moveInput = Input.GetAxis("Vertical");
        rotateInput = Input.GetAxis("Horizontal");

        CmdMove(moveInput, rotateInput);
    }

    [Command]
    void CmdMove(float move, float rotate)
    {
        Quaternion turn = Quaternion.Euler(0f, rotate * rotateSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * turn);

        Vector3 movement = transform.forward * move * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
    }
}
