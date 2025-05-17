using UnityEngine;
using Mirror;
using TMPro;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string nickname;

    public TextMeshPro nicknameText;

    // Khi nickname thay đổi, tự động cập nhật UI
    void OnNicknameChanged(string oldName, string newName)
    {
        if (nicknameText != null)
            nicknameText.text = newName;
    }

    // Gọi trên client có quyền điều khiển để gửi lệnh lên server thay đổi nickname
    [Command]
    public void CmdSetNickname(string name)
    {
        nickname = name;
    }

    // Nếu muốn đặt nickname ngay khi spawn hoặc bắt đầu
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // Ví dụ: gửi lệnh đặt nickname mặc định
        // CmdSetNickname("Player" + netId);
    }
}
