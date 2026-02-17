using UnityEngine;

public class PlayerAttackSystem : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform attackPoint; // Az önce oluþturduðun objeyi buraya sürükle
    public float attackRange = 1.0f; // Vuruþ menzili (yarýçap)
    public LayerMask enemyLayers; // Sadece "Enemy" veya "NPC" layer'ýna vurmasý için

    // Basitleþtirilmiþ eriþim (EquipmentManager'dan veriyi çekiyoruz):
    [HideInInspector] public int damageToDeal = 10; // Varsayýlan yumruk hasarý

    private EquipmentManager equipmentManager;
    private Item itemInHand;

    void Start()
    {
        equipmentManager = GetComponent<EquipmentManager>();
    }

    // BU FONKSÝYONU ANIMATION EVENT ÝLE ÇAÐIRACAÐIZ
    public void DealDamage()
    {
        itemInHand = equipmentManager.currentItemInHand;

        if (itemInHand != null)
        {
            damageToDeal = DamageCalculate(damageToDeal);
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
    private int DamageCalculate(int damageToDeal)
    {
        int randomValue = Random.Range(1, 101);

        // Þans deðerin (chance) 75 ise; 1'den 75'e kadar olan sayýlar kazanýr.
        // Eðer þansýn 0 ise; 1 <= 0 olamayacaðý için asla girmez.
        if (randomValue <= itemInHand.attackDamageMultiplierChance)
        {
            Debug.Log($"Kritik Vuruþ! (Zar: {randomValue} <= Þans: {itemInHand.attackDamageMultiplierChance})");

            // Hasarý multiplier ile çarpýp tam sayýya çeviriyoruz
            return Mathf.RoundToInt(damageToDeal * itemInHand.attackDamageMultiplier);
        }

        return itemInHand.attackDamage;
    }

    // Editörde saldýrý menzilini görmek için yardýmcý çizim
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}