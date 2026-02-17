using UnityEngine;

public class PlayerAttackSystem : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform attackPoint;
    public float attackRange = 1.0f;
    public LayerMask enemyLayers;

    private EquipmentManager equipmentManager;
    private Item itemInHand;

    // --- YENÝ EKLENEN BONUS DEÐÝÞKENLERÝ (Public yapýyoruz ki CrystalEffect eriþsin) ---
    [Header("Active Buffs/Effects")]
    public int bonusDamage = 0;                 // Kristalden gelen ekstra hasar
    public float bonusCritMultiplier = 0f;      // Kristalden gelen ekstra kritik çarpaný
    public int bonusCritChance = 0;             // Kristalden gelen ekstra kritik þansý
    // ---------------------------------------------------------------------------------

    void Start()
    {
        equipmentManager = GetComponent<EquipmentManager>();
    }

    public void DealDamage()
    {
        // 1. ÖNCE ELÝMÝZDEKÝ SÝLAHI GÜNCELLEYELÝM (Eksik Olan Kýsým)
        if (equipmentManager != null)
        {
            // EquipmentManager'dan güncel eþyayý alýyoruz
            itemInHand = equipmentManager.currentItemInHand;
        }
        // 1. Hasarý EquipmentManager'dan al
        int baseDamage = 1;
        if (equipmentManager != null)
        {
            baseDamage = equipmentManager.GetCurrentWeaponDamage();
        }

        // 2. Bonus hasarý ekle (Kristal etkisi burada devreye giriyor)
        int totalDamage = baseDamage + bonusDamage;

        // 3. Kritik hesaplamaya gönder
        int finalDamage = DamageCalculate(totalDamage);

        // ... (OverlapSphere ve Vuruþ kodlarý aynen kalacak) ...
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.gameObject == gameObject) continue;
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(finalDamage);
            }
        }
    }

    private int DamageCalculate(int currentDamage)
    {
        if (itemInHand == null) return currentDamage;

        // Þans hesaplarken bonus þansý da ekle
        // Örn: Kýlýç %10 + Kristal %20 = %30 þans
        int totalChance = itemInHand.attackDamageMultiplierChance + bonusCritChance;

        if (totalChance > 100) totalChance = 100;

        int randomValue = Random.Range(1, 101);

        if (randomValue <= totalChance)
        {
            Debug.Log("Kritik Vuruþ!");

            // Çarpan hesaplarken bonus çarpaný da ekle
            float totalMultiplier = itemInHand.attackDamageMultiplier + bonusCritMultiplier;

            return Mathf.RoundToInt(currentDamage * totalMultiplier);
        }

        return currentDamage;
    }

    // Editörde saldýrý menzilini görmek için yardýmcý çizim
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}