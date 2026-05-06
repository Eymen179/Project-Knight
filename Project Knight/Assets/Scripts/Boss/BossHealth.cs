using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealth : MonoBehaviour
{
    [Header("Veri & UI")]
    public EnemyStats stats;

    [Header("Durumlar (Salt Okunur)")]
    public int currentHealth;
    public bool isDead = false;

    [Header("Blok Ayarlarý")]
    public float maxBlockDuration = 5f;  // Maksimum blokta kalma süresi
    public float blockCooldown = 10f;    // Yeniden blok yapabilmesi için beklemesi gereken süre

    private float currentBlockTimer = 0f;
    private float currentCooldownTimer = 0f;

    private Animator animator;
    private BossAttackSystem attackSystem;
    private DamageFlasher damageFlasher;

    void Start()
    {
        animator = GetComponent<Animator>();
        attackSystem = GetComponent<BossAttackSystem>();
        damageFlasher = GetComponent<DamageFlasher>();

        if (stats != null)
        {
            currentHealth = stats.maxHealth;
        }

        UpdateUI();
    }

    void Update()
    {
        if (isDead) return;

        // 1. Blok Cooldown (Bekleme Süresi) Sayacý
        if (currentCooldownTimer > 0)
        {
            currentCooldownTimer -= Time.deltaTime;
        }

        // 2. Güvenlik: Blok maksimum 5 saniye sürebilir (Sonsuza kadar blokta kalmasýný engeller)
        if (animator.GetBool("isBlocking"))
        {
            currentBlockTimer += Time.deltaTime;

            if (currentBlockTimer >= maxBlockDuration)
            {
                EndBlock(); // 5 saniye dolduysa zorla bloku indir!
            }
        }
    }

    // --- HASAR ALMA SÝSTEMÝ ---
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        // Boss þu an blok durumundaysa hasar almaz (Stun da yemez)
        if (animator.GetBool("isBlocking"))
        {
            Debug.Log("Boss saldýrýyý kusursuzca blokladý!");
            // Ýleride buraya kýlýçlarýn çarpýþma sesini veya kývýlcým partikülünü (Instantiate) ekleyebilirsin.
            return;
        }

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        // Hasar alýndýðýnda parlamayý tetikle
        if (damageFlasher != null) damageFlasher.Flash();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- BLOK KONTROL MEKANÝZMASI (AI / ML-AGENTS ÝÇÝN) ---

    // Yapay Zeka blok yapmaya karar verdiðinde bu fonksiyonu çaðýracak.
    public bool TryStartBlock()
    {
        // Kural 1: Eðer 10 saniyelik ceza bitmediyse BLOK YAPAMAZ.
        // Kural 2: Boss þu an saldýrý yapýyorsa (ActionLocked) animasyonu bölemeyeceði için BLOK YAPAMAZ.
        if (currentCooldownTimer > 0 || attackSystem.isActionLocked)
            return false;

        animator.SetBool("isBlocking", true);
        attackSystem.LockAction(); // Boss'u kilitledik ki araya baþka eylem sýzmasýn
        currentBlockTimer = 0f;

        return true; // Blok baþarýyla baþladý
    }

    // Blok animasyonu bittiðinde (veya 5 saniye dolduðunda) çaðrýlýr.
    // Bunu istersen Blok animasyonunun SON karesine Animation Event olarak da ekleyebilirsin.
    public void EndBlock()
    {
        if (!animator.GetBool("isBlocking")) return;

        animator.SetBool("isBlocking", false);
        attackSystem.UnlockAction(); // Kilidi açtýk, artýk tekrar saldýrabilir

        currentBlockTimer = 0f;
        currentCooldownTimer = blockCooldown; // 10 saniyelik bekleme cezasýný baþlattýk

        Debug.Log("Boss gardýný indirdi, 10 saniye boyunca tekrar blok yapamaz.");
    }

    // --- UI VE ÖLÜM ---
    private void UpdateUI()
    {
        if (stats != null)
        {
            if (UIManager.Instance.bossHealthSlider != null)
            {
                UIManager.Instance.bossHealthSlider.value = (float)currentHealth / stats.maxHealth;
            }

            if (UIManager.Instance.txtBossHealth != null)
            {
                UIManager.Instance.txtBossHealth.text = currentHealth.ToString() + "/" + stats.maxHealth;
            }
        }
    }

    private void Die()
    {
        isDead = true;

        // Varsa Boss'un görkemli ölme animasyonunu oynat
        if (animator != null) animator.SetTrigger("die");

        // Collider'ý kapat ki oyuncu görünmez bir duvara çarpmasýn
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Can barýný ekrandan kaldýr
        if (UIManager.Instance.bossHealthSlider != null) UIManager.Instance.bossHealthSlider.gameObject.SetActive(false);

        /* ML-AGENTS GELECEK PLANI: 
         * Ajanýn karar verme sistemini durdurmak için ileride þu satýrý ekleyeceðiz:
         * GetComponent<Unity.MLAgents.DecisionRequester>().enabled = false;
         */

        Debug.Log("BOSS MAÐLUP EDÝLDÝ!");
    }
}