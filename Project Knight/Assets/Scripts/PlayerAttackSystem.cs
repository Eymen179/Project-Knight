using UnityEngine;

public class PlayerAttackSystem : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform attackPoint; // Az önce oluþturduðun objeyi buraya sürükle
    public float attackRange = 1.0f; // Vuruþ menzili (yarýçap)
    public LayerMask enemyLayers; // Sadece "Enemy" veya "NPC" layer'ýna vurmasý için

    private EquipmentManager equipmentManager;

    void Start()
    {
        equipmentManager = GetComponent<EquipmentManager>();
    }

    // BU FONKSÝYONU ANIMATION EVENT ÝLE ÇAÐIRACAÐIZ
    public void DealDamage()
    {
        // Basitleþtirilmiþ eriþim (EquipmentManager'dan veriyi çekiyoruz):
        int damageToDeal = 10; // Varsayýlan yumruk hasarý

        if (equipmentManager.currentItemInHand != null)
        {
            damageToDeal = equipmentManager.currentItemInHand.attackDamage;
        }

        // 2. Alaný Tara (OverlapSphere)
        // AttackPoint merkezli bir küre çiz ve içindeki colliderlarý bul
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        // 3. Bulunanlara Hasar Ver
        foreach (Collider enemy in hitEnemies)
        {
            // Kendimize vurmayalým
            if (enemy.transform.root == transform) continue;

            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageToDeal);

                // Ýsteðe baðlý: Vuruþ efekti/sesi burada çalýnabilir
            }
        }
    }

    // Editörde saldýrý menzilini görmek için yardýmcý çizim
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}