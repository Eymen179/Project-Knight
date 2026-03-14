using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarlarý")]
    public int maxHealth = 250;
    private int currentHealth;

    [Header("Savunma (Blok) Ayarlarý")]

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        // Ses çalmak için AudioSource bileþenini al veya yoksa otomatik ekle
        UpdateUI();
    }

    public void TakeDamage(int damageAmount)
    {
        // 1. BLOK KONTROLÜ: Animator'deki "isBlocking" parametresi aktif mi?
        // PlayerCombatManager scripti blok tuþuna basýldýðýnda bu parametreyi True yapýyor.
        if (animator != null && animator.GetBool("isBlocking"))
        {
            Debug.Log("Saldýrý baþarýyla BLOKLANDI!");

            // Ses efekti çal

            // Hasar almadan fonksiyondan çýk
            return;
        }

        // 2. HASAR ALMA (Blok yapýlmýyorsa burasý çalýþýr)
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"Oyuncu Hasar Aldý! Kalan Can: {currentHealth}");

        UpdateUI();

        // 3. ÖLÜM KONTROLÜ
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void UpdateUI()
    {
        // Artýk UIManager deðil, kendi referanslarýmýzý kontrol ediyoruz
        // 1. Slider Güncelleme
        if (UIManager.Instance.playerHealthSlider != null)
        {
            float fillValue = (float)currentHealth / maxHealth;
            UIManager.Instance.playerHealthSlider.value = fillValue;
        }

        // 2. Text Güncelleme
        if (UIManager.Instance.txtHealth != null)
        {
            UIManager.Instance.txtHealth.text = currentHealth.ToString() + "/" + maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Oyuncu Öldü! (Ölüm ekraný altyapýsý tetiklendi)");
        // Ýleriki adýmlarda buraya ölüm animasyonu ve UI Game Over ekraný gelecek
    }
}