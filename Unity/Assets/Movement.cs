using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviourPun
{
    public float moveSpeed = 7f;
    public float rotateSpeed = 100f;
    private float originalSpeed = 7f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!photonView.IsMine)
        {
            rb.isKinematic = true;
            this.enabled = false;
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    float h;

    void Update()
    {
        if (!photonView.IsMine) return;

        h = Input.GetAxis("Horizontal");

        Quaternion turn = Quaternion.Euler(0f, h * rotateSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * turn);
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        float v = Input.GetAxis("Vertical");

        Vector3 movement = transform.forward * v * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
    }
    [PunRPC]
    public void ApplyPowerUp(float duration)
    {
        moveSpeed += 5f; 

        CancelInvoke(nameof(ResetPowerUp));
        Invoke(nameof(ResetPowerUp), duration);
    }

    private void ResetPowerUp()
    {
        moveSpeed = originalSpeed;
    }
}
