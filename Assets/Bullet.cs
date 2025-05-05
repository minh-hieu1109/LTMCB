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

        // Chỉ xử lý va chạm nếu đạn này là của mình (người bắn)
        if (!photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject); // vẫn nên huỷ để đồng bộ visual
            return;
        }

        PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();
        if (targetView != null && targetView != photonView)
        {
            Health targetHealth = collision.gameObject.GetComponent<Health>();
            if (targetHealth != null)
            {
                if (targetHealth.IsInvincible())
                {
                    Debug.Log("Dang bat tu");
                }
                else
                {
                    targetHealth.photonView.RPC("TakeDamage", RpcTarget.All, damage);

                    if (damage > targetHealth.health)
                    {
                        RoomManager.instance.kills++;
                        RoomManager.instance.SetHashes();
                    }

                    Debug.Log("ban duoc");
                }
            }
        }

        PhotonNetwork.Destroy(gameObject);
    }
}
