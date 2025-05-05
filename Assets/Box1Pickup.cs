using UnityEngine;
using Photon.Pun;

public class Box1Pickup : MonoBehaviourPun
{
    public int healAmount = 20;
    public float speedBoostDuration = 5f;
    public float fireBoostDuration = 3f;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PhotonView targetPV = other.GetComponent<PhotonView>();
        if (targetPV != null && targetPV.IsMine)
        {
            targetPV.RPC("Heal", RpcTarget.AllBuffered, healAmount);
            targetPV.RPC("ApplyPowerUp", RpcTarget.AllBuffered, speedBoostDuration);
            targetPV.RPC("FireCollD", RpcTarget.AllBuffered, fireBoostDuration);
        }
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject); 
        }
    }
}