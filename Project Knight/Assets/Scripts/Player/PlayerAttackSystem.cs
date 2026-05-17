using UnityEngine;

public class PlayerAttackSystem : MonoBehaviour
{
    [Header("Settings")]
    public Transform attackPoint;
    public LayerMask enemyLayers;

    private EquipmentManager equipmentManager;
    private Item itemInHand;

    [Header("Active Buffs/Effects")]
    public int bonusDamage = 0;
    public float bonusCritMultiplier = 0f;
    public int bonusCritChance = 0;

    [Header("Permanent Upgrades")]
    public int permanentBonusDamage = 0;
    public float permanentBonusCritMultiplier = 0f;
    public int permanentBonusCritChance = 0;

    void Start()
    {
        equipmentManager = GetComponent<EquipmentManager>();
    }

    public void DealDamage()
    {
        //Eldeki kilic kontrolcusu
        if (equipmentManager != null)
        {
            itemInHand = equipmentManager.currentItemInHand;
        }

        //Aktif hasar kontrolcusu
        int baseDamage = 1;
        if (equipmentManager != null)
        {
            baseDamage = equipmentManager.GetCurrentWeaponDamage();
        }

        //Anlik hasar hesabi
        int totalDamage = baseDamage + bonusDamage + permanentBonusDamage;

        //Final hasar hesabi
        int finalDamage = DamageCalculate(totalDamage);

        //Saldiri kuresine giren objelerin tespiti
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, itemInHand.attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.gameObject == gameObject) continue;
            EnemyHealth enemyHealth = enemy.GetComponentInParent<EnemyHealth>();
            BossHealth bossHealth = enemy.GetComponentInParent<BossHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(finalDamage);
            }
            else if (bossHealth != null)
            {
                bossHealth.TakeDamage(finalDamage);
            }
        }
    }

    //Kritik hasar hesaplama metodu
    private int DamageCalculate(int currentDamage)
    {
        if (itemInHand == null) return currentDamage;

        //Son kritik sansi: Eldeki kilic + gecici kristal efekti + kalici kristal efekti
        int totalChance = itemInHand.attackDamageMultiplierChance + bonusCritChance + permanentBonusCritChance;

        if (totalChance > 100) totalChance = 100;

        int randomValue = Random.Range(1, 101);

        if (randomValue <= totalChance)
        {
            Debug.Log("Kritik Vuruþ!");

            //Son kritik hasar carpani: Eldeki kilic + gecici kristal efekti + kalici kristal efekti
            float totalMultiplier = itemInHand.attackDamageMultiplier + bonusCritMultiplier + permanentBonusCritMultiplier;

            return Mathf.RoundToInt(currentDamage * totalMultiplier);
        }

        return currentDamage;
    }

    private void OnDrawGizmos()
    {
        float gizmosRange = 1f;
        if (itemInHand != null) gizmosRange = itemInHand.attackRange;
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, gizmosRange);
    }
}