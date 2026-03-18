using TMPro;
using UnityEngine;
using UnityEngine.Audio;
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

    private Animator animator;

    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();

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
    //Animation Event
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return; // Öldüyse daha fazla hasar alma

        // --- 1. BLOK KONTROLÜ ---
        // Eðer AI blok yapmaya karar vermiþse ve animasyon aktifse
        if (animator != null && animator.GetBool("isBlocking"))
        {
            Debug.Log($"{stats?.enemyName} saldýrýný ustaca BLOKLADI!");

            return; // Hasar almadan fonksiyondan çýk
        }

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
        isDead = true;

        Debug.Log(gameObject.name + " öldü!");
        Destroy(gameObject);
    }
}