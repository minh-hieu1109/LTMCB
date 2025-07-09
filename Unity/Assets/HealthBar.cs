using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    public static HealthUIManager Instance;

    [SerializeField] private Slider healthSlider;

    private void Awake()
    {
        Instance = this;
    }

    public void SetMaxHealth(int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    public void UpdateHealth(int currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
}
