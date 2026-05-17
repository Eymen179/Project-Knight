using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 250;
    public int currentHealth;

    [Header("Defense (Block) Settings")]
    private Animator animator;
    private DamageFlasher damageFlasher;

    [Header("Advanced Block & Stun Settings")]
    public int maxBlockCount = 3;

    private int currentBlockCount;
    private float lastBlockTime = 0f;

    public float blockResetTime = 5f;
    public float stunDuration = 1f;

    private bool isStunned = false;
    public bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        currentBlockCount = maxBlockCount;
        animator = GetComponent<Animator>();
        damageFlasher = GetComponent<DamageFlasher>();

        if (SceneController.Instance != null && SceneController.Instance.savedMaxHealth != -1)
        {
            //Sahne gecisinden gelen can bilgileri
            maxHealth = SceneController.Instance.savedMaxHealth;
            currentHealth = SceneController.Instance.savedCurrentHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }

        UpdateUI();
    }
    private void Update()
    {
        if (isDead || isStunned) return;

        // 5 Saniye Kurali: Blok yenileme
        if (currentBlockCount < maxBlockCount)
        {
            if (Time.time - lastBlockTime >= blockResetTime)
            {
                currentBlockCount = maxBlockCount;
                Debug.Log("Oyuncu yorgunluðunu attý, 3 blok hakký yenilendi!");
            }
        }
    }
    //Ýyilesme Metodu
    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;

        //Can yenileme - max can kontrolcusu
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

        //Bloklama Kontrolcusu
        if (!isStunned && animator != null && animator.GetBool("isBlocking"))
        {
            currentBlockCount--;
            lastBlockTime = Time.time;

            if (currentBlockCount > 0)
            {
                Debug.Log($"Saldýrý BLOKLANDI! Kalan Hak: {currentBlockCount}");
                return;
            }
            else
            {
                //Blok hakki dolunca
                Debug.Log("GARD KIRILDI! Oyuncu Sersemledi!");
                StartCoroutine(StunRoutine());
                return;
            }
        }

        //Hasar yeme
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        Debug.Log($"Oyuncu Hasar Aldý! Kalan Can: {currentHealth}");
        UpdateUI();

        if (damageFlasher != null) damageFlasher.Flash();

        //Olum kontrolcusu
        if (currentHealth <= 0) Die();
    }

    //Stun sistemi
    private IEnumerator StunRoutine()
    {
        isStunned = true;
        animator.SetBool("isBlocking", false); //Blok zorla kapatilir.
        animator.SetTrigger("stun");

        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerCombatManager>().enabled = false;

        // 2 Saniye Bekle
        yield return new WaitForSeconds(stunDuration);

        if (!isDead)
        {
            GetComponent<PlayerMovement>().enabled = true;
            GetComponent<PlayerCombatManager>().enabled = true;
            currentBlockCount = maxBlockCount;
            isStunned = false;
            Debug.Log("Oyuncu sersemlemeden çýktý, savaþa hazýr!");
        }
    }
    //UI Guncelleme metodu
    public void UpdateUI()
    {
        if (UIManager.Instance.playerHealthSlider != null)
        {
            float fillValue = (float)currentHealth / maxHealth;
            UIManager.Instance.playerHealthSlider.value = fillValue;
        }

        if (UIManager.Instance.txtHealth != null)
        {
            UIManager.Instance.txtHealth.text = currentHealth.ToString() + "/" + maxHealth;
        }
    }
    //Olum metodu
    public void Die()
    {
        isDead = true;
        Debug.Log("Oyuncu Öldü! (Ölüm ekraný altyapýsý tetiklendi)");

        UIManager.Instance.pnlDieScreen.SetActive(true);
        InventoryManager.Instance.CursorVisibility(true);

        gameObject.GetComponent<PlayerCombatManager>().enabled = false;
        gameObject.GetComponent<PlayerMovement>().enabled = false;
        gameObject.GetComponent<PlayerInteraction>().enabled = false;

        gameObject.GetComponent<Animator>().SetFloat("speed", 0f);
    }
}