using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCrystalEffect : MonoBehaviour
{
    [Header("Input ve Slotlar")]
    [SerializeField] private InputActionReference[] inventorySlotActions;
    [SerializeField] private InventorySlot[] inventorySlots;

    [Header("Referanslar")]
    private PlayerAttackSystem playerAttackSystem;
    private EquipmentManager equipmentManager;
    private PlayerHealth playerHealth; // YENÝ EKLENDÝ

    private bool isCrystalActive = false;
    void Start()
    {
        // Validasyon
        if (inventorySlotActions.Length != inventorySlots.Length)
        {
            Debug.LogError("HATA: Input sayýsý ile Slot sayýsý eþit deðil!");
            return;
        }

        playerHealth = GetComponent<PlayerHealth>(); // YENÝ EKLENDÝ
        playerAttackSystem = GetComponent<PlayerAttackSystem>();
        equipmentManager = GetComponent<EquipmentManager>();
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
    private void UseCrystalPerformed(InputAction.CallbackContext context)
    {
        if (isCrystalActive)
        {
            Debug.Log("Kristal Efekti Aktif!");
            // Ýleride buraya hata sesi ekleyebilirsin.
            return;
        }
        // Hangi tuþa basýldýðýný bul
        for (int i = 0; i < inventorySlotActions.Length; i++)
        {
            if (inventorySlotActions[i].action == context.action)
            {
                // O slot dolu mu?
                if (inventorySlots[i].transform.childCount > 0)
                {
                    // Slottaki InventoryItem bileþenini al
                    InventoryItem itemInSlot = inventorySlots[i].transform.GetChild(0).GetComponent<InventoryItem>();

                    // Eðer bu bir "Other" (yani kristal/iksir) tipindeyse kullan
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
                break; // Döngüden çýk
            }
        }
    }

    private void ConsumeItemAndApplyEffect(InventoryItem inventoryItem)
    {
        Item crystalToUse = inventoryItem.item;

        // 1. Efekti Baþlat
        if (crystalToUse.effectDuration > 0)
        {
            // Süreli efekt
            StartCoroutine(EffectRoutine(crystalToUse, crystalToUse.effectDuration));
        }
        else
        {
            // Kalýcý efekt (Kalýcýlýk için PlayerPrefs sistemi buraya entegre edilecek)
            ApplyEffect(crystalToUse, true);
        }

        // 2. Eþyayý Tüket (Envanterden Silme Ýþlemi)
        // Sayýyý düþür
        inventoryItem.count--;

        if (inventoryItem.count <= 0)
        {
            // Eðer sayý bittiyse objeyi yok et
            Destroy(inventoryItem.gameObject);
        }
        else
        {
            // Bitmediyse sayýyý güncelle
            inventoryItem.RefreshCount();
        }
    }
    // GÜNCELLENDÝ: Geri sayým sayacý eklendi
    IEnumerator EffectRoutine(Item crystal, float duration)
    {
        // Efekti ver ve UI'ý ayarla
        ApplyEffect(crystal, true);

        //Efekt aktifken baþka kristal kullanýlamaz.
        SetCrsytalUsability(false);

        Debug.Log($"{crystal.itemName} etkisi baþladý! ({duration} sn)");

        float remainingTime = duration;
        float regenTimer = 0f; // YENÝ: Yenilenme hýzý için sayaç

        // Kalan süre 0'dan büyük olduðu sürece döngüyü çalýþtýr
        while (remainingTime > 0)
        {
            // UI Güncellemesi (Örn: "5.0", "4.9")
            if (UIManager.Instance != null && UIManager.Instance.txtEffectDuration != null)
            {
                // "F1" formatý virgülden sonra tek hane gösterir (Örn: 5.0)
                UIManager.Instance.txtEffectDuration.text = remainingTime.ToString("F1");
            }
            // --- 2. YENÝ: CAN YENÝLEME MANTIÐI ---
            // Eðer kristalin bir yenileme miktarý ve hýzý varsa çalýþýr
            if (crystal.healthRegenerationAmount > 0 && crystal.healthRegenerationSpeed > 0 && playerHealth != null)
            {
                // Sayacý her karede artýr
                regenTimer += Time.deltaTime;

                // Eðer sayaç, belirlenen hýza ulaþtýysa (örn: her 2 saniyede bir)
                if (regenTimer >= crystal.healthRegenerationSpeed)
                {
                    // Caný doldur ve sayacý sýfýrla ki tekrar saymaya baþlasýn
                    playerHealth.Heal(crystal.healthRegenerationAmount);
                    regenTimer = 0f;
                }
            }
            // -------------------------------------
            // Zamaný eksilt
            remainingTime -= Time.deltaTime;

            // Bir sonraki frame'e (kareye) kadar bekle
            yield return null;
        }

        // Süre bittiðinde efekti geri al
        ApplyEffect(crystal, false);

        //Efekt bittiðinde yeni kristal kullanýmý açýlýr.
        SetCrsytalUsability(true);

        Debug.Log($"{crystal.itemName} etkisi bitti.");
    }

    // GÜNCELLENDÝ: Dinamik metin üretimi eklendi
    private void ApplyEffect(Item crystal, bool isApplying)
    {
        // Çarpan faktörü: True ise 1 (Ekle), False ise -1 (Çýkar)
        int factor = isApplying ? 1 : -1;

        // --- SALDIRI HASARI VE KRÝTÝK EFEKTLERÝ ---
        if (playerAttackSystem != null)
        {
            if (crystal.isPermanent && isApplying)
            {
                // KALICI: factor'e gerek yok, sadece 1 kere ekliyoruz ve geri alýnmýyor
                playerAttackSystem.permanentBonusDamage += crystal.attackDamage;
                playerAttackSystem.permanentBonusCritChance += crystal.attackDamageMultiplierChance;
                playerAttackSystem.permanentBonusCritMultiplier += crystal.attackDamageMultiplier;
            }
            else if (!crystal.isPermanent)
            {
                // SÜRELÝ: factor ile ekle veya çýkar
                playerAttackSystem.bonusDamage += (crystal.attackDamage * factor);
                playerAttackSystem.bonusCritChance += (crystal.attackDamageMultiplierChance * factor);
                playerAttackSystem.bonusCritMultiplier += (crystal.attackDamageMultiplier * factor);
            }
        }

        // --- SALDIRI HIZI EFEKTÝ ---
        if (equipmentManager != null)
        {
            if (crystal.isPermanent && isApplying)
            {
                // KALICI HIZ
                equipmentManager.permanentBonusAttackSpeed += (crystal.attackSpeed / 10f);
            }
            else if (!crystal.isPermanent)
            {
                // SÜRELÝ HIZ
                equipmentManager.bonusAttackSpeed += (crystal.attackSpeed / 10f * factor);
            }
            equipmentManager.UpdateAttackSpeed(); // Silah ve animatör hýzýný güncelle
        }

        // --- CAN DOLDURMA VE KALICI CAN YÜKSELTME ---
        if (isApplying && crystal.health != 0 && playerHealth != null)
        {
            playerHealth.Heal(crystal.health); // Her halükarda anlýk caný doldur

            if (crystal.isPermanent)
            {
                playerHealth.maxHealth += crystal.health; // Kalýcýysa maksimum kapasiteyi artýr
                playerHealth.UpdateUI(); // UI'ý güncelle ki yeni max can görünür olsun
            }
        }
        //Saldiri hasari, kritik sansi, kritik carpani efekt ayarlari
        /*if (playerAttackSystem != null)
        {
            playerAttackSystem.bonusDamage += (crystal.attackDamage * factor);
            playerAttackSystem.bonusCritChance += (crystal.attackDamageMultiplierChance * factor);
            playerAttackSystem.bonusCritMultiplier += (crystal.attackDamageMultiplier * factor);
        }
        //Saldiri hizi efekt ayari
        if (equipmentManager != null)
        {
            equipmentManager.bonusAttackSpeed += (crystal.attackSpeed / 10f * factor);
            equipmentManager.UpdateAttackSpeed();
        }
        //Can doldurma efekt ayari
        if (isApplying && crystal.health != 0 && playerHealth != null)
        {
            playerHealth.Heal(crystal.health);
        }
        //Kalýcý can yükseltme efekt ayari (Sadece uygularken çalýþýr, bitince geri alýnmaz)
        if (isApplying && crystal.health != 0 && playerHealth != null && crystal.isPermanent)
        {
            playerHealth.Heal(crystal.health);
            playerHealth.maxHealth += crystal.health; // Maksimum caný da artýr
            playerHealth.UpdateUI(); // UI'ý güncelle ki yeni max can görünür olsun
        }*/
        // --- UI GÜNCELLEME KISMI ---
        // Sadece süreli efektler için UI panelini aç/kapat
        if (crystal.effectDuration > 0 && UIManager.Instance != null)
        {
            UIManager.Instance.pnlCrystalEffectStatus.SetActive(isApplying);

            // Sadece efekt baþlarken yazýlarý oluþturalým (biterken panel kapanacaðý için gerek yok)
            if (isApplying)
            {
                string effectDetails = "";

                // Dinamik Ýþaret Mantýðý: Sadece 0'dan büyükse "+" koyar, küçükse hiçbir þey koymaz (kendi eksisi görünür)

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

                // Oluþturulan dinamik metni UI'a gönder
                if (UIManager.Instance.txtEffects != null)
                {
                    UIManager.Instance.txtEffects.text = effectDetails;
                }
            }
        }
    }
    private void SetCrsytalUsability(bool isUsable)
    {
        isCrystalActive = !isUsable;
    }
}