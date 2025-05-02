using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class Health : MonoBehaviourPun
{
    public int health;
    public bool isLocalPlayer;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;

    [PunRPC]
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (photonView.IsMine)
        {
            healthText.text = health.ToString();
        }

        if (health <= 0)
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
                RoomManager.instance.SpawnPlayer();
                RoomManager.instance.deaths++;
                RoomManager.instance.SetHashes();

               
            }
        }
    }
    
}