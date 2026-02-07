using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Veri")]
    public EnemyStats stats; // Scriptable Object'i buraya sürükle

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
            currentHealth = 100; // Varsayýlan güvenlik deðeri
            Debug.LogWarning(gameObject.name + " üzerinde EnemyStats eksik!");
        }

        UpdateUI();
    }

    // Hasar alma fonksiyonu (Dýþarýdan çaðrýlacak)
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        Debug.Log($"{stats?.enemyName} hasar aldý: -{damageAmount}. Kalan Can: {currentHealth}");

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateUI()
    {
        if(UIManager.Instance.healthSlider != null && stats != null)
        {
            float fillValue = (float)currentHealth / stats.maxHealth;
            UIManager.Instance.healthSlider.value = fillValue;

            UIManager.Instance.txtHealth.text = currentHealth.ToString() + "/" + stats.maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " öldü!");
        // Þimdilik sadece objeyi yok edelim veya kapatýp havuza atalým
        // Ýleride buraya ragdoll fiziði eklenebilir.
        Destroy(gameObject);
    }
}