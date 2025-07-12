using UnityEngine;
using TMPro;

public class CoinUIManager : MonoBehaviour
{
    public static CoinUIManager Instance;

    [SerializeField] private TextMeshProUGUI coinsText;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateCoins(int coins)
    {
        if (coinsText != null)
        {
            coinsText.text = $"{coins}";
        }
    }
}
