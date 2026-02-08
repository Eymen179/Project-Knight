using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Veri")]
    public EnemyStats stats;

    // YENÝ: UIManager yerine kendi slider ve text'imizi kullanacaðýz
    [Header("NPC UI")]
    public Slider healthSlider;      // NPC'nin kendi Canvas'ýndaki Slider
    public TextMeshProUGUI healthText; // NPC'nin kendi Canvas'ýndaki Text

    private int currentHealth;

    void Start()
    {
        // Baþlangýç canýný statlardan al
        if (stats != null)
        {
            currentHealth = stats.maxHealth;
        }
        else
        {
            currentHealth = 100;
            Debug.LogWarning(gameObject.name + " üzerinde EnemyStats eksik!");
        }

        // Baþlangýçta UI'ý ayarla
        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f; // Slider deðer aralýðýný 0-1 yapýyoruz
            healthSlider.value = 1f;
        }

        UpdateUI();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        // Can eksiye düþmesin
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"{stats?.enemyName} hasar aldý: -{damageAmount}. Kalan Can: {currentHealth}");

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateUI()
    {
        // Artýk UIManager deðil, kendi referanslarýmýzý kontrol ediyoruz
        if (stats != null)
        {
            // 1. Slider Güncelleme
            if (healthSlider != null)
            {
                float fillValue = (float)currentHealth / stats.maxHealth;
                healthSlider.value = fillValue;
            }

            // 2. Text Güncelleme
            if (healthText != null)
            {
                healthText.text = currentHealth.ToString() + "/" + stats.maxHealth;
            }
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " öldü!");
        Destroy(gameObject);
    }
}