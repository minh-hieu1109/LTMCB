using UnityEngine;
using TMPro;
using Mirror;
using System.Collections.Generic;
using System.Collections;

public class LeaderboardUI : NetworkBehaviour
{
    [Header("UI")]
    public GameObject holder;                // Cái đè Tab
    public GameObject[] slots;              // Các dòng
    public TextMeshProUGUI[] nameTexts;
    public TextMeshProUGUI[] scoreTexts;

    [Header("Options")]
    public float refreshRate = 1f;

    private bool show = false;

    void Start()
    {
        holder.SetActive(false);
        if (isLocalPlayer)
            StartCoroutine(AutoRefresh());
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // Ấn giữ Tab để hiển thị
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            show = true;
            holder.SetActive(true);
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            show = false;
            holder.SetActive(false);
        }
    }

    IEnumerator AutoRefresh()
    {
        while (true)
        {
            if (show) RequestLeaderboard();
            yield return new WaitForSeconds(refreshRate);
        }
    }

    public void RequestLeaderboard()
    {
        if (isLocalPlayer)
        {
            CmdRequestLeaderboard();
        }
    }

    [Command]
    void CmdRequestLeaderboard()
    {
        List<PlayerData> dataList = new List<PlayerData>();

        foreach (var player in LeaderboardManager.Instance.allPlayers)
        {
            string name = $"Player {player.connectionToClient.connectionId}";
            int kills = player.kills;
            int deaths = player.deaths;
            dataList.Add(new PlayerData(name, kills, deaths));
        }

        dataList.Sort((a, b) => b.kills.CompareTo(a.kills));
        RpcUpdateLeaderboard(dataList.ToArray());
    }

    [ClientRpc]
    void RpcUpdateLeaderboard(PlayerData[] data)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < data.Length)
            {
                slots[i].SetActive(true);
                nameTexts[i].text = data[i].name;
                scoreTexts[i].text = $"{data[i].kills} / {data[i].deaths}";
            }
            else
            {
                slots[i].SetActive(false);
            }
        }
    }

    public struct PlayerData
    {
        public string name;
        public int kills;
        public int deaths;

        public PlayerData(string name, int kills, int deaths)
        {
            this.name = name;
            this.kills = kills;
            this.deaths = deaths;
        }
    }
}
