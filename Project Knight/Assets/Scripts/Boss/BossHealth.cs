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

    [Header("Ragdoll Settings")]
    public Rigidbody hipsRigidbody;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Rigidbody mainRigidbody;
    private Collider mainCollider;

    // "Shotgun (Çoklu Vuruþ)" Bug'ý Korumasý
    private float lastDamageTime = 0f;

    // --- ML-AGENTS REFERANSI ---
    private BossAgent agent;

    [Header("Görev/Kapý Sistemi (Events)")]
    public UnityEvent onBossDied;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        attackSystem = GetComponent<BossAttackSystem>();
        damageFlasher = GetComponent<DamageFlasher>();
        agent = GetComponent<BossAgent>();

        mainRigidbody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();

        // (true) parametresi ile kapalý olsalar bile alt kemikleri bulmasýný saðlarýz
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = GetComponentsInChildren<Collider>(true);
    }

    void Start()
    {
        if (stats != null) currentHealth = stats.maxHealth;

        if (localHealthSlider != null) localHealthSlider.maxValue = 1f;

        // Oyun baþlarken Ragdoll'u kapalý tut, animasyonlarý oynat
        if (ragdollRigidbodies != null && ragdollRigidbodies.Length > 0)
        {
            SetRagdollState(false);
        }

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

        // --- SHOTGUN (ÇOKLU VURUÞ) KORUMASI ---
        if (Time.time < lastDamageTime + 0.1f) return;
        lastDamageTime = Time.time;

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
        // if (animator != null) animator.SetTrigger("die"); (Buna gerek kalmadý)

        // --- RAGDOLL'U AKTÝF ET ---
        SetRagdollState(true);

        // Vuruþ hissi için kalça kemiðine darbe kuvveti uygula (Boss aðýr olduðu için kuvveti artýrdýk: 25f)
        if (hipsRigidbody != null)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                Vector3 knockbackDirection = player.transform.forward + (Vector3.up * 0.8f);
                hipsRigidbody.AddForce(knockbackDirection.normalized * 25f, ForceMode.Impulse);
            }
        }

        // Fizik motoru ile savaþmamasý için NavMeshAgent'ý kapat
        UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null) navAgent.enabled = false;

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

        onBossDied?.Invoke(); // Boss ölünce sinyal gönder

        // BÜYÜK CEZA: Boss ölürse aðýr eksi puan alýr ve eðitim turu (Episode) biter.
        if (agent != null)
        {
            agent.enabled = false; // Beyni tamamen kapat
        }
    }

    public void ResetHealth()
    {
        isDead = false;
        if (stats != null) currentHealth = stats.maxHealth;

        // --- RAGDOLL'U KAPAT VE ANÝMASYONA DÖN ---
        SetRagdollState(false);

        UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null) navAgent.enabled = true;

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
    private void SetRagdollState(bool isRagdoll)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb == mainRigidbody) continue;
            rb.isKinematic = !isRagdoll;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col == mainCollider) continue;
            col.enabled = true; // Yaþarken de açýk kalmalýlar ki kýlýç çarpabilsin
        }

        if (mainCollider != null) mainCollider.enabled = !isRagdoll;

        if (mainRigidbody != null)
        {
            // Boss NavMeshAgent kullandýðý için ana bedeni buzda kaymamasý adýna Kinematic kalmalý
            mainRigidbody.isKinematic = true;
        }

        if (animator != null) animator.enabled = !isRagdoll;
    }
}