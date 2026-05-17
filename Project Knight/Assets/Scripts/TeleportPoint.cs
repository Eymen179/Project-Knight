using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("Bu portaldan geçince hangi sahne yüklenecek?")]
    public SceneController.GameScenes destinationScene;

    [Tooltip("Gidilecek sahnedeki doðma noktasýnýn adý (Örn: ZindanKuzeyGiris)")]
    public string targetSpawnPointID;

    private void OnTriggerEnter(Collider other)
    {
        TeleportPointSettings(other);
    }

    //Ýsinlanma Metodu
    public void TeleportPointSettings(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"{destinationScene} sahnesine ýþýnlanýlýyor...");

            //Envanteri kaydet.
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.SaveInventory();
            }

            //Kalici Efektleri Kaydet.
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

            //Cani kaydet.
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                SceneController.Instance.savedCurrentHealth = playerHealth.currentHealth;
                SceneController.Instance.savedMaxHealth = playerHealth.maxHealth;
            }

            //Spawn Noktasini kaydet.
            if (SceneController.Instance != null)
            {
                SceneController.Instance.targetSpawnPointID = this.targetSpawnPointID;
            }

            SceneController.Instance.LoadScene(destinationScene);
        }
    }
}
