using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCrystalEffect : MonoBehaviour
{
    [Header("Inputs & Slots")]
    [SerializeField] private InputActionReference[] inventorySlotActions;
    [SerializeField] private InventorySlot[] inventorySlots;

    [Header("References")]
    private PlayerAttackSystem playerAttackSystem;
    private EquipmentManager equipmentManager;
    private PlayerHealth playerHealth;

    [Header("Visual Effects")]
    public ParticleSystem crystalUseEffect;

    [Header("Permanent Effect Counters")]
    public int permanentHealthCount = 0;
    public int permanentDamageCount = 0;
    public int permanentSpeedCount = 0;

    private bool isCrystalActive = false;
    void Start()
    {
        if (inventorySlotActions.Length != inventorySlots.Length)
        {
            Debug.LogError("HATA: Input sayýsý ile Slot sayýsý eþit deðil!");
            return;
        }

        playerHealth = GetComponent<PlayerHealth>();
        playerAttackSystem = GetComponent<PlayerAttackSystem>();
        equipmentManager = GetComponent<EquipmentManager>();

        //Sahne gecislerinde kalici efektlerin korunmasi icin geri yukleme islemi
        if (SceneController.Instance != null && SceneController.Instance.hasSavedPermanentEffects)
        {
            //UI degiskenleri
            permanentHealthCount = SceneController.Instance.savedPermanentHealthCount;
            permanentDamageCount = SceneController.Instance.savedPermanentDamageCount;
            permanentSpeedCount = SceneController.Instance.savedPermanentSpeedCount;

            //Kalici efektler
            if (playerAttackSystem != null)
            {
                playerAttackSystem.permanentBonusDamage = SceneController.Instance.savedPermanentBonusDamage;
                playerAttackSystem.permanentBonusCritChance = SceneController.Instance.savedPermanentBonusCritChance;
                playerAttackSystem.permanentBonusCritMultiplier = SceneController.Instance.savedPermanentBonusCritMultiplier;
            }

            //Kilictaki efektler
            if (equipmentManager != null)
            {
                equipmentManager.permanentBonusAttackSpeed = SceneController.Instance.savedPermanentBonusAttackSpeed;

                equipmentManager.UpdateAttackSpeed();
            }

            //UI Guncellemesi
            UpdatePermanentUI();
        }
    }
    private void OnEnable()
    {
        foreach (var inputs in inventorySlotActions)
        {
            if (inputs != null)
            {
                inputs.action.Enable();
                inputs.action.performed += UseCrystalPerformed;
            }
        }
    }
    private void OnDisable()
    {
        foreach (var inputs in inventorySlotActions)
        {
            if (inputs != null)
            {
                inputs.action.performed -= UseCrystalPerformed;
                inputs.action.Disable();
            }
        }
    }

    //2-3-4 Tuslari metodu
    private void UseCrystalPerformed(InputAction.CallbackContext context)
    {
        if (isCrystalActive)
        {
            Debug.Log("Kristal Efekti Aktif!");
            // Ýleride buraya hata sesi ekleyebilirsin.
            return;
        }
        //2-3-4 tus kontrolcusu
        for (int i = 0; i < inventorySlotActions.Length; i++)
        {
            if (inventorySlotActions[i].action == context.action)
            {
                //Slotta kristal var mi?
                if (inventorySlots[i].transform.childCount > 0)
                {
                    InventoryItem itemInSlot = inventorySlots[i].transform.GetChild(0).GetComponent<InventoryItem>();

                    if (itemInSlot.item.itemType == Item.ItemType.Other)
                    {
                        ConsumeItemAndApplyEffect(itemInSlot);
                    }
                }
                else
                {
                    // Boþ slota basýldý (Ses efekti buraya)
                    Debug.Log("Slot Boþ!");
                }
                break;
            }
        }
    }

    private void ConsumeItemAndApplyEffect(InventoryItem inventoryItem)
    {
        Item crystalToUse = inventoryItem.item;

        //Kullanilan kristalin rengine gore VFX
        if (crystalUseEffect != null)
        {
            //Particle System'in ana (main) modulune erisiyoruz.
            var main = crystalUseEffect.main;

            //Kristalin rengini partikulun baslangic rengi (startColor) yapiyoruz.
            main.startColor = crystalToUse.itemColor;

            crystalUseEffect.Play();
        }

        if (crystalToUse.effectDuration > 0)
        {
            //Sureli efekt
            StartCoroutine(EffectRoutine(crystalToUse, crystalToUse.effectDuration));
        }
        else
        {
            //Kalici efekt
            ApplyEffect(crystalToUse, true);
        }

        //Envanter slotu temizleme
        inventoryItem.count--;

        if (inventoryItem.count <= 0)
        {
            Destroy(inventoryItem.gameObject);
        }
        else
        {
            inventoryItem.RefreshCount();
        }
    }
    //Sureli efekt Coroutine'u
    IEnumerator EffectRoutine(Item crystal, float duration)
    {
        //Can yenileme efekti haric tum efektler buradan uygulanir.
        ApplyEffect(crystal, true);

        SetCrsytalUsability(false);

        Debug.Log($"{crystal.itemName} etkisi baþladý! ({duration} sn)");

        float remainingTime = duration;
        float regenTimer = 0f;

        //Efekt Paneli
        while (remainingTime > 0)
        {
            if (UIManager.Instance != null && UIManager.Instance.txtEffectDuration != null)
            {
                // "F1" (Orn: 5.0)
                UIManager.Instance.txtEffectDuration.text = remainingTime.ToString("F1");
            }

            //Can yenileme efekti
            if (crystal.healthRegenerationAmount > 0 && crystal.healthRegenerationSpeed > 0 && playerHealth != null)
            {
                //Sayaci her karede artir.
                regenTimer += Time.deltaTime;

                // Eger sayac, belirlenen hiza ulastiysa (orn: her 2 saniyede bir)
                if (regenTimer >= crystal.healthRegenerationSpeed)
                {
                    playerHealth.Heal(crystal.healthRegenerationAmount);
                    regenTimer = 0f;
                }
            }

            remainingTime -= Time.deltaTime;

            yield return null;
        }

        //EFekti geri al.
        ApplyEffect(crystal, false);

        SetCrsytalUsability(true);

        Debug.Log($"{crystal.itemName} etkisi bitti.");
    }

    //Efekt uygulama metodu (Can yenileme haric)
    private void ApplyEffect(Item crystal, bool isApplying)
    {
        //Efekt acma kapama ayari
        int factor = isApplying ? 1 : -1;

        //Hasar ve kritik efektleri
        if (playerAttackSystem != null)
        {
            if (crystal.isPermanent && isApplying)//Kalici
            {
                playerAttackSystem.permanentBonusDamage += crystal.attackDamage;
                playerAttackSystem.permanentBonusCritChance += crystal.attackDamageMultiplierChance;
                playerAttackSystem.permanentBonusCritMultiplier += crystal.attackDamageMultiplier;

                if (crystal.attackDamage > 0)
                {
                    permanentDamageCount++;
                    UpdatePermanentUI();
                }
            }
            else if (!crystal.isPermanent)//Sureli
            {
                playerAttackSystem.bonusDamage += (crystal.attackDamage * factor);
                playerAttackSystem.bonusCritChance += (crystal.attackDamageMultiplierChance * factor);
                playerAttackSystem.bonusCritMultiplier += (crystal.attackDamageMultiplier * factor);
            }
        }

        //Saldiri hizi efekti
        if (equipmentManager != null)
        {
            if (crystal.isPermanent && isApplying)
            {
                //Kalici
                equipmentManager.permanentBonusAttackSpeed += (crystal.attackSpeed / 10f);

                if (crystal.attackSpeed > 0)
                {
                    permanentSpeedCount++;
                    UpdatePermanentUI();
                }
            }
            else if (!crystal.isPermanent)
            {
                //Sureli
                equipmentManager.bonusAttackSpeed += (crystal.attackSpeed / 10f * factor);
            }
            equipmentManager.UpdateAttackSpeed();
        }

        //Can Doldurma Efekti
        if (isApplying && crystal.health != 0 && playerHealth != null)
        {
            playerHealth.Heal(crystal.health);

            if (crystal.isPermanent)//Kalici ise
            {
                playerHealth.maxHealth += crystal.health;
                playerHealth.UpdateUI();

                if (crystal.health > 0)
                {
                    permanentHealthCount++;
                    UpdatePermanentUI();
                }
            }
        }
        //UI Ayarlari (Sureli Efekt Paneli)
        if (crystal.effectDuration > 0 && UIManager.Instance != null)
        {
            UIManager.Instance.pnlCrystalEffectStatus.SetActive(isApplying);

            //Efekt yazilari
            if (isApplying)
            {
                string effectDetails = "";

                //Efekt ozelligine gore "+" ya da "-" ayari yapilir.
                if (crystal.attackDamage != 0)
                    effectDetails += $"Attack Damage {(crystal.attackDamage > 0 ? "+" : "")}{crystal.attackDamage}\n";

                if (crystal.attackSpeed != 0)
                    effectDetails += $"Attack Speed {(crystal.attackSpeed > 0 ? "+" : "")}{crystal.attackSpeed}\n";

                if (crystal.attackRange != 0)
                    effectDetails += $"Attack Range {(crystal.attackRange > 0 ? "+" : "")}{crystal.attackRange}\n";

                if (crystal.attackDamageMultiplierChance != 0)
                    effectDetails += $"Crit Chance {(crystal.attackDamageMultiplierChance > 0 ? "+" : "")}{crystal.attackDamageMultiplierChance}%\n";

                if (crystal.attackDamageMultiplier != 0)
                    effectDetails += $"Crit Multiplier {(crystal.attackDamageMultiplier > 0 ? "+" : "")}{crystal.attackDamageMultiplier}\n";

                if (crystal.health != 0)
                    effectDetails += $"Health {(crystal.health > 0 ? "+" : "")}{crystal.health}\n";

                if (crystal.healthRegenerationAmount != 0)
                    effectDetails += $"Health Regen {(crystal.healthRegenerationAmount > 0 ? "+" : "")}{crystal.healthRegenerationAmount}\n";

                if (crystal.healthRegenerationSpeed != 0)
                    effectDetails += $"Health Regen Speed {(crystal.healthRegenerationSpeed > 0 ? "+" : "")}{crystal.healthRegenerationSpeed}\n";

                if (UIManager.Instance.txtEffects != null)
                {
                    UIManager.Instance.txtEffects.text = effectDetails;
                }
            }
        }
    }
    //UI Ayarlari (Kalici Efekt Paneli)
    public void UpdatePermanentUI()
    {
        if (UIManager.Instance == null) return;

        if (!UIManager.Instance.pnlPermanentCrystals.activeSelf &&
            (permanentHealthCount > 0 || permanentDamageCount > 0 || permanentSpeedCount > 0))
        {
            UIManager.Instance.pnlPermanentCrystals.SetActive(true);
        }

        if (UIManager.Instance.txtHealthPermanent != null)
            UIManager.Instance.txtHealthPermanent.text = $"x{permanentHealthCount}";

        if (UIManager.Instance.txtDamagePermanent != null)
            UIManager.Instance.txtDamagePermanent.text = $"x{permanentDamageCount}";

        if (UIManager.Instance.txtSpeedPermanent != null)
            UIManager.Instance.txtSpeedPermanent.text = $"x{permanentSpeedCount}";
    }
    //Sureli kristal aktifken kristal kullanimini ayarlayan metot
    private void SetCrsytalUsability(bool isUsable)
    {
        isCrystalActive = !isUsable;
    }
}