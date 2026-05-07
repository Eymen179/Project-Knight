using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro için gerekli

public class DummyHealth : MonoBehaviour
{
    [Header("Can Ayarlarý")]
    public int maxHealth = 250;
    public int currentHealth;

    [Header("UI Ayarlarý")]
    public Slider localHealthSlider;
    public TextMeshProUGUI healthText; // Can rakamýný gösterecek Text bileþeni

    public bool isDead = false;

    private DamageFlasher damageFlasher;

    void Start()
    {
        currentHealth = maxHealth;

        if (localHealthSlider != null) localHealthSlider.maxValue = 1f;

        UpdateUI();

        damageFlasher = GetComponent<DamageFlasher>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        // --- YENÝ EKLENEN: SAVUNMA (BLOK) KONTROLÜ ---
        // Eðer bu kuklada DummyDefensive scripti varsa ve TryBlock() true dönerse, hasar almadan çýk.
        DummyDefensive defensive = GetComponent<DummyDefensive>();
        if (defensive != null && defensive.TryBlock())
        {
            return;
        }
        // ---------------------------------------------

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        if (damageFlasher != null) damageFlasher.Flash();

        UpdateUI();

        if (currentHealth <= 0) Die();

        Debug.Log("Dummy hasar yiyo");
    }

    private void UpdateUI()
    {
        if (localHealthSlider != null)
        {
            localHealthSlider.value = (float)currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    public void ResetDummy()
    {
        isDead = false;
        currentHealth = maxHealth;
        gameObject.SetActive(true);
        UpdateUI();

        // --- YENÝ EKLENEN: EÐÝTÝM SIFIRLANIRKEN SAVUNMAYI DA SIFIRLA ---
        DummyDefensive defensive = GetComponent<DummyDefensive>();
        if (defensive != null) defensive.ResetDefensive();

        // --- YENÝ EKLENEN: HAREKETÝ SIFIRLA ---
        DummyMovement movement = GetComponent<DummyMovement>();
        if (movement != null) movement.ResetMovement();
    }

    private void Die()
    {
        isDead = true;
        gameObject.SetActive(false);
    }
}