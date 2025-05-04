using UnityEngine;
using Photon.Pun;

public class TankShooting : MonoBehaviourPun
{
    public GameObject bulletPrefab;
    public Transform firePoint;

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) // Click chuột trái
        {
            Fire();
        }
    }

    void Fire()
    {
        PhotonNetwork.Instantiate(bulletPrefab.name, firePoint.position, firePoint.rotation);

        // Mất bất tử khi bắn
        if (photonView.IsMine)
        {
            GetComponent<Health>().CancelInvincible();
        }
    }
}