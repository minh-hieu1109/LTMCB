using UnityEngine;
using Photon.Pun;

public class Box1Pickup : MonoBehaviourPun
{
    public int healAmount = 20;
    public float speedBoostDuration = 5f;
    public float fireBoostDuration = 3f;

    public float rotationSpeed = 50f;
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 2f;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

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
