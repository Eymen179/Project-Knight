using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    [Header("Iþýnlanma Ayarlarý")]
    [Tooltip("Bu portaldan geçince hangi sahne yüklenecek?")]
    // Kendi yazdýðýn enum'ý burada çaðýrýyoruz. Inspector'da þýk bir liste olacak.
    public SceneController.GameScenes destinationScene;

    // Karakter portalýn içine girdiðinde (temas ettiðinde) tetiklenir
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"{destinationScene} sahnesine ýþýnlanýlýyor...");

            // 1. Envanteri Kaydet
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.SaveInventory();
            }

            // 2. Kalýcý Efektleri Kaydet
            PlayerCrystalEffect crystalEffect = other.GetComponent<PlayerCrystalEffect>();
            PlayerAttackSystem attackSystem = other.GetComponent<PlayerAttackSystem>();
            EquipmentManager equipmentManager = other.GetComponent<EquipmentManager>();

            if (crystalEffect != null && attackSystem != null && equipmentManager != null)
            {
                SceneController.Instance.savedPermanentHealthCount = crystalEffect.permanentHealthCount;
                SceneController.Instance.savedPermanentDamageCount = crystalEffect.permanentDamageCount;
                SceneController.Instance.savedPermanentSpeedCount = crystalEffect.permanentSpeedCount;

                SceneController.Instance.savedPermanentBonusDamage = attackSystem.permanentBonusDamage;
                SceneController.Instance.savedPermanentBonusCritChance = attackSystem.permanentBonusCritChance;
                SceneController.Instance.savedPermanentBonusCritMultiplier = attackSystem.permanentBonusCritMultiplier;
                SceneController.Instance.savedPermanentBonusAttackSpeed = equipmentManager.permanentBonusAttackSpeed;

                SceneController.Instance.hasSavedPermanentEffects = true;
            }

            // --- YENÝ EKLENEN: CAN VE MAKSÝMUM CANI KAYDET ---
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Kalýcý saðlýk basýldýysa maxHealth zaten artmýþtýr, bunu direkt kaydediyoruz
                SceneController.Instance.savedCurrentHealth = playerHealth.currentHealth;
                SceneController.Instance.savedMaxHealth = playerHealth.maxHealth;
            }
            // -------------------------------------------------

            // 3. Sahneyi Yükle
            SceneController.Instance.LoadScene(destinationScene);
        }
    }
}
