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

    void Start()
    {
        // Validasyon
        if (inventorySlotActions.Length != inventorySlots.Length)
        {
            Debug.LogError("HATA: Input sayýsý ile Slot sayýsý eþit deðil!");
            return;
        }

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

    // Coroutine isimlendirmesi "Routine" ile biterse daha anlaþýlýr olur
    /*IEnumerator EffectRoutine(Item crystal, float duration)
    {
        // Efekti ver
        ApplyEffect(crystal, true);
        Debug.Log($"{crystal.itemName} etkisi baþladý! ({duration} sn)");

        yield return new WaitForSeconds(duration);

        // Efekti geri al
        ApplyEffect(crystal, false);
        Debug.Log($"{crystal.itemName} etkisi bitti.");
    }

    // Int (1/0) yerine Bool (true/false) kullanýmý
    private void ApplyEffect(Item crystal, bool isApplying)
    {
        // Çarpan faktörü: True ise 1 (Ekle), False ise -1 (Çýkar)
        int factor = isApplying ? 1 : -1;

        //UI Aktifligi
        UIManager.Instance.pnlCrystalEffectStatus.SetActive(isApplying);

        if (playerAttackSystem != null)
        {
            playerAttackSystem.bonusDamage += (crystal.attackDamage * factor);
            playerAttackSystem.bonusCritChance += (crystal.attackDamageMultiplierChance * factor);
            playerAttackSystem.bonusCritMultiplier += (crystal.attackDamageMultiplier * factor);
        }

        if (equipmentManager != null)
        {
            // Saldýrý hýzý int olduðu için float'a çeviriyoruz (/10f)
            equipmentManager.bonusAttackSpeed += (crystal.attackSpeed * factor);

            // EquipmentManager'a hýzý güncellemesini söyle
            equipmentManager.UpdateAttackSpeed();
        }

        if(factor > 0)
        {

        }
    }*/

    // GÜNCELLENDÝ: Geri sayým sayacý eklendi
    IEnumerator EffectRoutine(Item crystal, float duration)
    {
        // Efekti ver ve UI'ý ayarla
        ApplyEffect(crystal, true);
        Debug.Log($"{crystal.itemName} etkisi baþladý! ({duration} sn)");

        float remainingTime = duration;

        // Kalan süre 0'dan büyük olduðu sürece döngüyü çalýþtýr
        while (remainingTime > 0)
        {
            // UI Güncellemesi (Örn: "5.0", "4.9")
            if (UIManager.Instance != null && UIManager.Instance.txtEffectDuration != null)
            {
                // "F1" formatý virgülden sonra tek hane gösterir (Örn: 5.0)
                UIManager.Instance.txtEffectDuration.text = remainingTime.ToString("F1");
            }

            // Zamaný eksilt
            remainingTime -= Time.deltaTime;

            // Bir sonraki frame'e (kareye) kadar bekle
            yield return null;
        }

        // Süre bittiðinde efekti geri al
        ApplyEffect(crystal, false);
        Debug.Log($"{crystal.itemName} etkisi bitti.");
    }

    // GÜNCELLENDÝ: Dinamik metin üretimi eklendi
    private void ApplyEffect(Item crystal, bool isApplying)
    {
        // Çarpan faktörü: True ise 1 (Ekle), False ise -1 (Çýkar)
        int factor = isApplying ? 1 : -1;

        if (playerAttackSystem != null)
        {
            playerAttackSystem.bonusDamage += (crystal.attackDamage * factor);
            playerAttackSystem.bonusCritChance += (crystal.attackDamageMultiplierChance * factor);
            playerAttackSystem.bonusCritMultiplier += (crystal.attackDamageMultiplier * factor);
        }

        if (equipmentManager != null)
        {
            // Saldýrý hýzý int olduðu için float'a çeviriyoruz (/10f)
            equipmentManager.bonusAttackSpeed += (crystal.attackSpeed / 10f * factor);
            equipmentManager.UpdateAttackSpeed();
        }

        // --- UI GÜNCELLEME KISMI ---
        // Sadece süreli efektler için UI panelini aç/kapat
        if (crystal.effectDuration > 0 && UIManager.Instance != null)
        {
            UIManager.Instance.pnlCrystalEffectStatus.SetActive(isApplying);

            // Sadece efekt baþlarken yazýlarý oluþturalým (biterken panel kapanacaðý için gerek yok)
            if (isApplying)
            {
                string effectDetails = "";

                // Özellik 0'dan farklýysa metne ekle. 
                // Pozitif sayýlarda baþýna "+" koymak için (crystal.X > 0 ? "+" : "") mantýðý kullanýyoruz.
                // Negatif sayýlarda eksi iþareti zaten otomatik olarak yazdýrýlýr.

                if (crystal.attackDamage != 0)
                    effectDetails += $"Attack Damage {(crystal.attackDamage > 0 ? "+" : "")}{crystal.attackDamage}\n";

                if (crystal.attackSpeed != 0)
                    effectDetails += $"Attack Speed {(crystal.attackSpeed > 0 ? "+" : "")}{crystal.attackSpeed}\n";

                if (crystal.attackDamageMultiplierChance != 0)
                    effectDetails += $"Crit Chance {(crystal.attackDamageMultiplierChance > 0 ? "+" : "")}{crystal.attackDamageMultiplierChance}%\n";

                if (crystal.attackDamageMultiplier != 0)
                    effectDetails += $"Crit Multiplier {(crystal.attackDamageMultiplier > 0 ? "+" : "")}{crystal.attackDamageMultiplier}\n";

                // Ýleride zýrh, can vs. eklediðinde buraya ayný kalýpla ekleyebilirsin:
                // if (crystal.armor != 0)
                //    effectDetails += $"Armor {(crystal.armor > 0 ? "+" : "")}{crystal.armor}\n";

                // Oluþturulan dinamik metni UI'a gönder
                if (UIManager.Instance.txtEffects != null)
                {
                    UIManager.Instance.txtEffects.text = effectDetails;
                }
            }
        }
    }
}