using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendList : MonoBehaviour
{
    public TMP_Text nameText;
    public Button btnChat;
    public Button btnDelete;

    private int friendId;
    private string friendName;

    // Reference đến Confirm Panel
    public GameObject confirmPanel;
    public TMP_Text confirmMessage;
    public Button confirmYesButton;
    public Button confirmNoButton;

    public void Setup(FindPlayerManager.PlayerData friendData)
    {
        Debug.Log("=== BẮT ĐẦU Setup FriendList ===");
        Debug.Log($"friendData: {friendData}");
        Debug.Log($"friendData.id: {friendData?.id}");
        Debug.Log($"friendData.nickname: {friendData?.nickname}");

        if (friendData == null)
        {
            Debug.LogError("friendData is null in Setup!");
            return;
        }

        friendId = friendData.id;
        friendName = friendData.nickname;

        Debug.Log($"Set friendId: {friendId}");
        Debug.Log($"Set friendName: {friendName}");

        if (nameText != null)
        {
            nameText.text = friendName;
            Debug.Log($"nameText set to: {friendName}");
        }
        else
        {
            Debug.LogError("nameText is null!");
        }

        if (btnChat != null)
        {
            btnChat.onClick.RemoveAllListeners();
            btnChat.onClick.AddListener(() => FindPlayerManager.instance.OnOpenChatFriend());
            Debug.Log("btnChat listener added");
        }
        else
        {
            Debug.LogError("btnChat is null!");
        }

        if (btnDelete != null)
        {
            btnDelete.onClick.RemoveAllListeners();
            btnDelete.onClick.AddListener(() => ShowConfirmPanel());
            Debug.Log("btnDelete listener added");
        }
        else
        {
            Debug.LogError("btnDelete is null!");
        }

        Debug.Log("=== KẾT THÚC Setup FriendList ===");
    }

    void ShowConfirmPanel()
    {
        Debug.Log("=== BẮT ĐẦU ShowConfirmPanel ===");
        Debug.Log($"Current friendId: {friendId}");
        Debug.Log($"Current friendName: {friendName}");
        Debug.Log($"this.gameObject: {this.gameObject}");

        if (confirmPanel == null || confirmMessage == null || confirmYesButton == null || confirmNoButton == null)
        {
            Debug.LogError("Chưa gán đầy đủ các thành phần Confirm Panel vào prefab.");
            return;
        }

        if (FindPlayerManager.instance == null)
        {
            Debug.LogError("FindPlayerManager.instance is null!");
            return;
        }

        confirmPanel.SetActive(true);
        confirmMessage.text = $"Bạn có muốn xoá \"{friendName}\" không?";

        confirmYesButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.RemoveAllListeners();

        confirmYesButton.onClick.AddListener(() =>
        {
            Debug.Log("Confirm YES clicked");
            confirmPanel.SetActive(false);

            Debug.Log($"About to delete friend - ID: {friendId}, GameObject: {this.gameObject}");

            if (this.gameObject != null)
            {
                FindPlayerManager.instance.StartCoroutine(
                    FindPlayerManager.instance.DeleteFriend(friendId, this.gameObject)
                );
            }
            else
            {
                Debug.LogError("this.gameObject is null when trying to delete friend!");
            }
        });

        confirmNoButton.onClick.AddListener(() =>
        {
            Debug.Log("Confirm NO clicked");
            confirmPanel.SetActive(false);
        });

        Debug.Log("=== KẾT THÚC ShowConfirmPanel ===");
    }
}
