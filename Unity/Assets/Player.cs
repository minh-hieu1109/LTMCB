using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;

public class Player : MonoBehaviourPun
{
    public string nickname;
    public TextMeshPro nicknameText;
    [PunRPC]
    public void SetNickname(string name)
    {
        nickname = name;

        nicknameText.text = nickname;
    }
    
}
  