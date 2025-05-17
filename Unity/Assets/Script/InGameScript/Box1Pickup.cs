//using UnityEngine;
//using Mirror;

//public class Box1Pickup : NetworkBehaviour
//{
//    public int healAmount = 20;
//    public float speedBoostDuration = 5f;
//    public float fireBoostDuration = 3f;

//    public float rotationSpeed = 50f;
//    public float floatAmplitude = 0.5f;
//    public float floatSpeed = 2f;
//    private Vector3 startPos;

//    private void Start()
//    {
//        startPos = transform.position;
//    }

//    private void Update()
//    {
//        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
//        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
//        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (!other.CompareTag("Player")) return;

//        // Kiểm tra player local authority
//        Player player = other.GetComponent<Player>();
//        if (player != null && player.isLocalPlayer)
//        {
//            // Gọi Command để heal và áp dụng powerup trên server
//            player.Heal(healAmount);
//            player.CmdApplySpeedBoost(speedBoostDuration);
//            player.CmdApplyFireCooldownBoost(fireBoostDuration);

//            // Yêu cầu server huỷ vật phẩm này
//            CmdDestroyPickup();
//        }
//    }

//    [Command]
//    private void CmdDestroyPickup()
//    {
//        NetworkServer.Destroy(gameObject);
//    }
//}
