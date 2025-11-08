using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image fillImage;
    public TextMeshProUGUI healthText;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        if (playerHealth != null && fillImage != null)
        {
            float fillValue = (float)playerHealth.currentHealth / playerHealth.maxHealth;
            fillImage.fillAmount = fillValue;

            if (healthText != null)
                healthText.text = playerHealth.currentHealth + " / " + playerHealth.maxHealth;
        }
    }
}
