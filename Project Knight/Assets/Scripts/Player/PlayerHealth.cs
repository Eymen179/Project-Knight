using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarlarý")]
    public int maxHealth = 250;
    public int currentHealth;

    [Header("Savunma (Blok) Ayarlarý")]

    private Animator animator;
    private DamageFlasher damageFlasher;

    [Header("Geliþmiþ Blok & Stun Ayarlarý")]
    public int maxBlockCount = 3;       // Peþ peþe maksimum blok hakký

    private int currentBlockCount;      // Kalan blok hakkýmýz
    private float lastBlockTime = 0f;   // Son bloklanan saldýrýnýn zamaný

    public float blockResetTime = 5f;   // Haklarýn yenilenmesi için gereken süre
    public float stunDuration = 1f;     // Sersemleme süresi

    private bool isStunned = false;     // Karakter sersemlemiþ durumda mý?
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        currentBlockCount = maxBlockCount; // Baþlangýçta 3 hakkýmýz var
        animator = GetComponent<Animator>();
        damageFlasher = GetComponent<DamageFlasher>();

        if (SceneController.Instance != null && SceneController.Instance.savedMaxHealth != -1)
        {
            // Verileri köprüden çek (Böylece 250 yerine kalýcý artýrýlmýþ 280 caný alýr)
            maxHealth = SceneController.Instance.savedMaxHealth;
            currentHealth = SceneController.Instance.savedCurrentHealth;
        }
        else
        {
            // Oyun ilk defa baþlýyorsa caný fulleyerek baþlat
            currentHealth = maxHealth;
        }

        UpdateUI();
    }
    private void Update()
    {
        if (isDead || isStunned) return;

        // 5 Saniye Kuralý: Eðer haklarýmýz eksikse ve son bloktan beri 5 saniye geçtiyse
        if (currentBlockCount < maxBlockCount)
        {
            if (Time.time - lastBlockTime >= blockResetTime)
            {
                currentBlockCount = maxBlockCount;
                Debug.Log("Oyuncu yorgunluðunu attý, 3 blok hakký yenilendi!");
            }
        }
    }
    // --- YENÝ EKLENEN ÝYÝLEÞME FONKSÝYONU ---
    public void Heal(int healAmount)
    {
        if (isDead) return; // Öldüyse can basýlamaz

        currentHealth += healAmount;

        // Canýmýz maksimum caný geçmesin
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        UpdateUI();
        Debug.Log($"Oyuncu Ýyileþti! (+{healAmount}) Mevcut Can: {currentHealth}");
    }
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        // 1. BLOK KONTROLÜ (Eðer sersemlememiþsek ve blok yapýyorsak)
        if (!isStunned && animator != null && animator.GetBool("isBlocking"))
        {
            currentBlockCount--; // 1 hakkýmýzý düþ
            lastBlockTime = Time.time; // Zamanlayýcýyý sýfýrla

            if (currentBlockCount > 0)
            {
                Debug.Log($"Saldýrý BLOKLANDI! Kalan Hak: {currentBlockCount}");
                return; // Hasar alma
            }
            else
            {
                // HAKKIMIZ BÝTTÝ -> SERSEMLEME (STUN)
                Debug.Log("GARD KIRILDI! Oyuncu Sersemledi!");
                StartCoroutine(StunRoutine());
                return; // Gard kýrýldýðýnda o vuruþun hasarýný almaz ama kilitlenir
            }
        }

        // 2. HASAR ALMA
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        Debug.Log($"Oyuncu Hasar Aldý! Kalan Can: {currentHealth}");
        UpdateUI();

        if (damageFlasher != null) damageFlasher.Flash();

        // 3. ÖLÜM KONTROLÜ
        if (currentHealth <= 0) Die();
    }

    // --- SERSEMLEME (STUN) SÝSTEMÝ ---
    private IEnumerator StunRoutine()
    {
        isStunned = true;
        animator.SetBool("isBlocking", false); // Gardý zorla indir
        animator.SetTrigger("stun"); // Sersemleme animasyonunu oynat

        // Hareketi ve Saldýrýyý Kapat
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerCombatManager>().enabled = false;

        // 2 Saniye Bekle
        yield return new WaitForSeconds(stunDuration);

        if (!isDead)
        {
            // 2 saniye sonra her þeyi geri aç ve haklarý yenile
            GetComponent<PlayerMovement>().enabled = true;
            GetComponent<PlayerCombatManager>().enabled = true;
            currentBlockCount = maxBlockCount;
            isStunned = false;
            Debug.Log("Oyuncu sersemlemeden çýktý, savaþa hazýr!");
        }
    }
    public void UpdateUI()
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
        isDead = true;
        Debug.Log("Oyuncu Öldü! (Ölüm ekraný altyapýsý tetiklendi)");
        // Ýleriki adýmlarda buraya ölüm animasyonu ve UI Game Over ekraný gelecek
    }
}