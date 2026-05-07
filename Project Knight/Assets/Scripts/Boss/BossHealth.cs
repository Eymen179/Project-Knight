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

    void Start()
    {
        animator = GetComponent<Animator>();
        attackSystem = GetComponent<BossAttackSystem>();
        damageFlasher = GetComponent<DamageFlasher>();
        agent = GetComponent<BossAgent>(); // Ajaný bulduk

        if (stats != null) currentHealth = stats.maxHealth;
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

    private void UpdateUI()
    {
        if (stats != null)
        {
            if (UIManager.Instance.bossHealthSlider != null)
                UIManager.Instance.bossHealthSlider.value = (float)currentHealth / stats.maxHealth;

            if (UIManager.Instance.txtBossHealth != null)
                UIManager.Instance.txtBossHealth.text = currentHealth.ToString() + "/" + stats.maxHealth;
        }
    }

    private void Die()
    {
        isDead = true;
        if (animator != null) animator.SetTrigger("die");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (UIManager.Instance.bossHealthSlider != null) UIManager.Instance.bossHealthSlider.gameObject.SetActive(false);

        // BÜYÜK CEZA: Boss ölürse aðýr eksi puan alýr ve eðitim turu (Episode) biter.
        if (agent != null)
        {
            agent.SetReward(-1.0f);
            agent.EndEpisode();
        }
    }

    // --- EÐÝTÝM ÝÇÝN SIFIRLAMA METODU ---
    public void ResetHealth()
    {
        isDead = false;
        if (stats != null) currentHealth = stats.maxHealth;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        if (UIManager.Instance.bossHealthSlider != null) UIManager.Instance.bossHealthSlider.gameObject.SetActive(true);
        UpdateUI();
    }
}