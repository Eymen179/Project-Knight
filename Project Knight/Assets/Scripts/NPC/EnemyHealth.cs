using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using RangeAttribute = UnityEngine.RangeAttribute;

public class EnemyHealth : MonoBehaviour
{
    [Header("Veri")]
    public EnemyStats stats;
    public List<GameObject> crystalPrefabs = new List<GameObject>();

    [Header("NPC UI")]
    public Slider healthSlider;      // NPC'nin kendi Canvas'indaki Slider
    public TextMeshProUGUI healthText; // NPC'nin kendi Canvas'indaki Text

    [HideInInspector] public int currentHealth;
    [HideInInspector] public int healthBeforeDamage;
    [HideInInspector] public bool isDead = false;
    private int dynamicMaxHealth; // Zorluk arttýkca max cani artan NPC'ler icin dinamik max can degeri

    private Animator animator;
    private DamageFlasher damageFlasher;

    [Header("Enhanced Block & Stun Settings")]
    public int maxBlockCount = 3;

    private int currentBlockCount;
    private float lastBlockTime = 0f;

    public float blockResetTime = 5f;
    public float stunDuration = 1f;
    public bool isStunned = false;

    [Header("Loot Settings")]
    [Range(0,100)]
    public int dropChance = 100;
    void Start()
    {
        animator = GetComponent<Animator>();
        damageFlasher = GetComponent<DamageFlasher>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f;
        }

        // Oyun ilk baþladýðýnda sahnede hazýr duran NPC'ler için zorluðu hesapla
        ApplyDifficultyAndReset();
    }
    void Update()
    {
        if (isDead || isStunned) return;

        //Blok yenileme kontrolu
        if (currentBlockCount < maxBlockCount)
        {
            if (Time.time - lastBlockTime >= blockResetTime)
            {
                currentBlockCount = maxBlockCount;
            }
        }
    }
    //DealDamage Animation Event metodunun kullandigi metot
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        //Blok Kontrolu
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

        //Hasar Alma Kontrolu
        healthBeforeDamage = currentHealth; //Hasar almadan onceki cani kaydet.
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();
        
        //Hasar yiyince parlama efekti
        if (damageFlasher != null) damageFlasher.Flash();

        //Olum Kontrolu
        if (currentHealth <= 0) Die();
    }

    //Stun Sistemi
    private IEnumerator StunRoutine()
    {
        isStunned = true;
        animator.SetBool("isBlocking", false);
        animator.SetTrigger("stun"); //NPC'nin sersemleme animasyonu

        //NPC'yi dondur.
        GetComponent<EnemyAI>().enabled = false;
        GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;

        yield return new WaitForSeconds(stunDuration);

        if (!isDead)
        {
            //NPC'yi uyandir.
            GetComponent<EnemyAI>().enabled = true;
            GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = false;
            currentBlockCount = maxBlockCount;
            isStunned = false;
        }
    }
    // --- YENÝ EKLENEN ORTAK METOT ---
    // Bu metot NPC her doðduðunda güncel zorluk çarpanýný SceneController'dan çeker.
    private void ApplyDifficultyAndReset()
    {
        if (stats != null)
        {
            // Güvenlik: SceneController henüz sahnede yoksa çarpaný 1 kabul et
            float multiplier = 1.0f;
            if (SceneController.Instance != null)
            {
                multiplier = SceneController.Instance.globalDifficultyMultiplier;
            }

            dynamicMaxHealth = Mathf.RoundToInt(stats.maxHealth * multiplier);
            currentHealth = dynamicMaxHealth;
            healthBeforeDamage = currentHealth;
        }
        else
        {
            dynamicMaxHealth = 100;
            currentHealth = dynamicMaxHealth;
            Debug.LogWarning(gameObject.name + " üzerinde EnemyStats eksik!");
        }

        UpdateUI();
    }
    public void UpdateUI()
    {
        if (stats != null)
        {
            // GÜVENLÝK KÝLÝDÝ: dynamicMaxHealth 0'dan büyükse bölme iþlemi yap (NaN hatasýný önler)
            if (healthSlider != null && dynamicMaxHealth > 0)
            {
                float fillValue = (float)currentHealth / dynamicMaxHealth;
                healthSlider.value = fillValue;
            }

            if (healthText != null)
            {
                healthText.text = currentHealth.ToString() + "/" + dynamicMaxHealth;
            }
        }
    }
    private void Die()
    {
        isDead = true;

        //Sansa bagli loot dusur.
        SpawnCrystal(transform);

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

        //Object Pooling
        StartCoroutine(DeactivateAfterSeconds(5f));
    }

    private IEnumerator DeactivateAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }

    //SPAWNER'IN ÇAGIRDIGI SIFIRLAMA (RESET) METODU
    public void ResetNPC()
    {
        isDead = false;
        isStunned = false;
        currentBlockCount = maxBlockCount;

        // --- EN KRÝTÝK DEÐÝÞÝKLÝK ---
        // NPC havuzdan her çýktýðýnda, eðer oyuncu ölüp zorluk arttýysa yeni canýný hesaplayarak doðar!
        ApplyDifficultyAndReset();

        gameObject.layer = LayerMask.NameToLayer("NPC");

        // Kinematic uyarýsý kontrolcüsü
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
            rb.freezeRotation = true;
        }

        if (animator != null)
        {
            animator.enabled = true;
            animator.SetFloat("speed", 0f);
        }

        if (healthSlider != null) healthSlider.gameObject.SetActive(true);
        UpdateUI();

        // NAVMESH AGENT'I ZEMINE CIVILEME (WARP)
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.enabled = true;
            ai.ResetAI();
        }
    }
    public void SpawnCrystal(Transform NPCpos)
    {
        int randomRoll = Random.Range(0, 100);

        if (randomRoll >= dropChance)
        {
            Debug.Log("Zar tutmadý, eþya düþmedi.");
            return;
        }

        if (crystalPrefabs.Count == 0) return;
        GameObject crystalPrefab = crystalPrefabs[Random.Range(0, crystalPrefabs.Count)];
        Instantiate(crystalPrefab, NPCpos.position + Vector3.up * 0.5f, Quaternion.identity);
    }
}