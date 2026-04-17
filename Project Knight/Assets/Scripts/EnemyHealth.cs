using System.Collections;
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

    [HideInInspector] public int currentHealth;
    [HideInInspector] public int healthBeforeDamage;

    private Animator animator;

    [HideInInspector]public bool isDead = false;

    [Header("Geliþmiþ Blok & Stun Ayarlarý")]
    public int maxBlockCount = 3;
    private int currentBlockCount;
    private float lastBlockTime = 0f;
    public float blockResetTime = 5f;
    public float stunDuration = 1f;
    public bool isStunned = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Baþlangýç canýný statlardan al
        if (stats != null)
        {
            currentHealth = stats.maxHealth;

            healthBeforeDamage = currentHealth; // Ýlk deðer olarak baþlangýç canýný atýyoruz
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
    void Update()
    {
        if (isDead || isStunned) return;

        // 5 Saniye Kuralý
        if (currentBlockCount < maxBlockCount)
        {
            if (Time.time - lastBlockTime >= blockResetTime)
            {
                currentBlockCount = maxBlockCount;
            }
        }
    }
    //Animation Event
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        // --- 1. BLOK KONTROLÜ ---
        if (!isStunned && animator != null && animator.GetBool("isBlocking"))
        {
            currentBlockCount--;
            lastBlockTime = Time.time;

            if (currentBlockCount > 0)
            {
                Debug.Log($"{stats?.enemyName} blokladý! Kalan Hakký: {currentBlockCount}");
                return;
            }
            else
            {
                Debug.Log($"{stats?.enemyName} GARD KIRILDI ve Sersemledi!");
                StartCoroutine(StunRoutine());
                return;
            }
        }

        // --- 2. HASAR ALMA ---
        healthBeforeDamage = currentHealth; // Hasar almadan önceki caný kaydet
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();

        // --- 3. ÖLÜM KONTROLÜ ---
        if (currentHealth <= 0) Die();
    }

    // --- SERSEMLEME (STUN) SÝSTEMÝ ---
    private IEnumerator StunRoutine()
    {
        isStunned = true;
        animator.SetBool("isBlocking", false);
        animator.SetTrigger("stun"); // NPC'nin sersemleme animasyonunu tetikle

        // NPC'nin Yapay Zekasýný ve Yürümesini Durdur
        GetComponent<EnemyAI>().enabled = false;
        GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;

        yield return new WaitForSeconds(stunDuration);

        if (!isDead)
        {
            // Yapay zekayý uyandýr
            GetComponent<EnemyAI>().enabled = true;
            GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = false;
            currentBlockCount = maxBlockCount;
            isStunned = false;
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

    /*private void Die()
    {
        isDead = true;

        Debug.Log(gameObject.name + " öldü!");
        Destroy(gameObject);
    }*/
    private void Die()
    {
        isDead = true;

        GetComponent<EnemyAI>().enabled = false;
        GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        if (animator != null) animator.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.freezeRotation = false;
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                Vector3 knockbackDirection = player.transform.forward + (Vector3.up * 0.8f);
                rb.AddForce(knockbackDirection.normalized * 10f, ForceMode.Impulse);
                rb.AddTorque(player.transform.right * 5f, ForceMode.Impulse);
            }
        }

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        // ESKÝ KOD: Destroy(gameObject, 5f);
        // YENÝ KOD: 5 saniye sonra kapat ki Spawner bunu görüp yeniden doðursun!
        StartCoroutine(DeactivateAfterSeconds(5f));
    }

    private IEnumerator DeactivateAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false); // Obje kapanýr, havuza geri döner
    }

    // SPAWNER'IN ÇAÐIRDIÐI SIFIRLAMA (RESET) METODU
    // SPAWNER'IN ÇAÐIRDIÐI SIFIRLAMA (RESET) METODU
    public void ResetNPC()
    {
        isDead = false;
        isStunned = false;
        currentBlockCount = maxBlockCount;

        if (stats != null)
        {
            currentHealth = stats.maxHealth;
            healthBeforeDamage = currentHealth; // <--- ÝÞTE EKSÝK OLAN HAYAT KURTARICI SATIR!
        }

        gameObject.layer = LayerMask.NameToLayer("NPC");

        // --- 1. ÇÖZÜM: KINEMATIC UYARISINI GÝDERME ---
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Eðer obje havuzda kaskatý haldeyse (Kinematic ise) hýzýný sýfýrlamaya çalýþma!
            // Sadece öldüðünde fizikselleþtiyse hýzýný sýfýrla.
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
            rb.freezeRotation = true;
        }
        // ---------------------------------------------

        if (animator != null)
        {
            animator.enabled = true;
            // Animasyonu zorla Idle (Bekleme) durumuna al ki koþuda takýlý kalmasýn
            animator.SetFloat("speed", 0f);
        }

        if (healthSlider != null) healthSlider.gameObject.SetActive(true);
        UpdateUI();

        // --- 2. ÇÖZÜM: NAVMESH AGENT'I ZEMÝNE ÇÝVÝLEME (WARP) ---
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
            // Iþýnlanma sonrasý ajanýn kafasý karýþmasýn diye onu zorla bulunduðu konuma baðlarýz
            agent.Warp(transform.position);
        }

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.enabled = true;
            ai.ResetAI();
        }
        // --------------------------------------------------------
    }
}