using UnityEngine;
using Photon.Pun;

public class TankShooting : MonoBehaviourPun
{
    public GameObject bulletPrefab;
    public Transform firePoint;

    public float fireCooldown = 0.5f; // Thời gian chờ giữa 2 lần bắn
    private float lastFireTime;

    void Update()
    {
        if (!photonView.IsMine) return;

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && Time.time >= lastFireTime + fireCooldown)
        {
            Fire();
        }
    }

    void Fire()
    {
        PhotonNetwork.Instantiate(bulletPrefab.name, firePoint.position, firePoint.rotation);

        lastFireTime = Time.time; // Cập nhật thời gian bắn

        // Mất bất tử khi bắn
        if (photonView.IsMine)
        {
            GetComponent<Health>().CancelInvincible();
        }
    }
}
