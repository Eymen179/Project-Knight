using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class BossHealth : MonoBehaviour
{
    [Header("Veri & UI")]
    public EnemyStats stats;

    [Header("Eðitim / Test Ayarlarý")]
    [Tooltip("Eðer açýksa UIManager yerine Boss'un altýndaki (Child) yerel UI kullanýlýr.")]
    public bool isTrainingMode = false;
    public Slider localHealthSlider;       // Eðitimdeki yerel can barý
    public TextMeshProUGUI localHealthText;  // Eðitimdeki yerel can yazýsý

    [Header("Durumlar (Salt Okunur)")]
    public int currentHealth;
    public bool isDead = false;

    [Header("Blok Ayarlarý")]
    public float minBlockDuration = 1f;
    public float maxBlockDuration = 5f;
    public float blockCooldown = 10f;

    private float currentBlockTimer = 0f;
    private float currentCooldownTimer = 0f;
    private float activeBlockDuration = 0f;

    private Animator animator;
    private BossAttackSystem attackSystem;
    private DamageFlasher damageFlasher;

    // --- ML-AGENTS REFERANSI ---
    private BossAgent agent;

    [Header("Görev/Kapý Sistemi (Events)")]
    public UnityEvent onBossDied;

    void Start()
    {
        animator = GetComponent<Animator>();
        attackSystem = GetComponent<BossAttackSystem>();
        damageFlasher = GetComponent<DamageFlasher>();
        agent = GetComponent<BossAgent>(); // Ajaný bulduk

        if (stats != null) currentHealth = stats.maxHealth;

        // Yerel slider'ýn maksimum deðerini ayarla
        if (localHealthSlider != null) localHealthSlider.maxValue = 1f;

        UpdateUI();
    }

    void Update()
    {
        if (isDead) return;

        if (currentCooldownTimer > 0) currentCooldownTimer -= Time.deltaTime;

        if (animator.GetBool("isBlocking"))
        {
            currentBlockTimer += Time.deltaTime;
            if (currentBlockTimer >= activeBlockDuration) EndBlock();
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        if (animator.GetBool("isBlocking"))
        {
            // ÖDÜL: Baþarýlý blok yaptýðý için küçük bir ödül
            if (agent != null) agent.AddReward(0.2f);
            Debug.Log("Boss saldýrýyý kusursuzca blokladý!");
            return;
        }

        // CEZA: Hasar aldýðý için aldýðý hasarla orantýlý ufak bir ceza
        if (agent != null) agent.AddReward(-0.1f);

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();
        if (damageFlasher != null) damageFlasher.Flash();

        if (currentHealth <= 0) Die();
    }

    public bool TryStartBlock()
    {
        if (currentCooldownTimer > 0 || attackSystem.isActionLocked) return false;

        animator.SetBool("isBlocking", true);
        attackSystem.LockAction();
        currentBlockTimer = 0f;
        activeBlockDuration = Random.Range(minBlockDuration, maxBlockDuration);

        return true;
    }

    public void EndBlock()
    {
        if (!animator.GetBool("isBlocking")) return;

        animator.SetBool("isBlocking", false);
        attackSystem.UnlockAction();

        currentBlockTimer = 0f;
        currentCooldownTimer = blockCooldown;
    }

    // --- GÜNCELLENEN UI SÝSTEMÝ ---
    private void UpdateUI()
    {
        if (stats == null) return;

        float fillValue = (float)currentHealth / stats.maxHealth;
        string healthStr = currentHealth.ToString() + "/" + stats.maxHealth;

        if (isTrainingMode)
        {
            // Eðitim modundaysak Boss'un kendi kafasýndaki UI'ý güncelle
            if (localHealthSlider != null) localHealthSlider.value = fillValue;
            if (localHealthText != null) localHealthText.text = healthStr;
        }
        else
        {
            // Asýl oyundaysak UIManager'daki devasa UI'ý güncelle
            if (UIManager.Instance != null && UIManager.Instance.bossHealthSlider != null)
            {
                UIManager.Instance.bossHealthSlider.value = fillValue;
                UIManager.Instance.txtBossHealth.text = healthStr;
            }
        }
    }

    private void Die()
    {
        isDead = true;
        if (animator != null) animator.SetTrigger("die");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Doðru UI'ý kapat
        if (isTrainingMode)
        {
            if (localHealthSlider != null) localHealthSlider.gameObject.SetActive(false);
        }
        else
        {
            if (UIManager.Instance != null && UIManager.Instance.bossHealthSlider != null)
                UIManager.Instance.bossHealthSlider.gameObject.SetActive(false);
        }

        // --- YENÝ EKLENEN KISIM ---
        onBossDied?.Invoke(); // Boss ölünce sinyal gönder
        // --------------------------

        // BÜYÜK CEZA: Boss ölürse aðýr eksi puan alýr ve eðitim turu (Episode) biter.
        if (agent != null)
        {
            agent.SetReward(-1.0f);
            agent.EndEpisode();
        }
    }

    public void ResetHealth()
    {
        isDead = false;
        if (stats != null) currentHealth = stats.maxHealth;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // Doðru UI'ý geri aç
        if (isTrainingMode)
        {
            if (localHealthSlider != null) localHealthSlider.gameObject.SetActive(true);
        }
        else
        {
            if (UIManager.Instance != null && UIManager.Instance.bossHealthSlider != null)
                UIManager.Instance.bossHealthSlider.gameObject.SetActive(true);
        }

        UpdateUI();
    }
}