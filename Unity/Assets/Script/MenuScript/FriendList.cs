using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FriendList : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public TMP_Text nicknameText;

    public void Setup(FindPlayerManager.PlayerData data)
    {
        nicknameText.text = data.nickname;
        // nếu có avatar hay status thì cập nhật thêm ở đây
    }
}
