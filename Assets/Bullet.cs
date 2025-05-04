using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviourPun
{
    public float speed = 20f;
    public float lifetime = 3f;
    public int damage = 10;

    [Header("VFX")]
    public GameObject hitVFX;  // Prefab hiệu ứng khi trúng

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Thêm dòng này
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hitVFX != null)
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.LookRotation(contact.normal);
            Vector3 pos = contact.point;

            GameObject vfx = Instantiate(hitVFX, pos, rot);
            Destroy(vfx, 2f);
        }

        // Kiểm tra đạn không làm trúng chính player bắn ra
        if (!photonView.IsMine) return;

        PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();
        if (targetView != null && targetView != photonView)
        {
            Health targetHealth = collision.gameObject.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.photonView.RPC("TakeDamage", RpcTarget.All, damage);


                // Nếu damage đủ giết mục tiêu
                if (damage > targetHealth.health && !targetHealth.IsInvincible())
                {
                    RoomManager.instance.kills++;
                    RoomManager.instance.SetHashes();
                }
            }

            PhotonNetwork.Destroy(gameObject);
        }
        PhotonNetwork.Destroy(gameObject);
    }
}
