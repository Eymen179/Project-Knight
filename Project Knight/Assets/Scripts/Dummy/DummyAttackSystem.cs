using UnityEngine;

public class DummyAttackSystem : MonoBehaviour
{
    [Header("Saldýrý Ayarlarý")]
    public int baseDamage = 20;       // Dummy'nin vuracaðý sabit hasar
    public float attackRange = 1.5f;  // Vuruþ küresinin boyutu

    [Header("Referanslar")]
    public Transform attackPoint;     // Dummy'nin önündeki boþ obje
    public LayerMask bossLayer;       // Sadece Boss'un katmanýný seçeceksin (Örn: "Enemy")

    // Bu metodu Dummy'nin Animator penceresindeki saldýrý animasyonuna 
    // "Animation Event" olarak eklemeyi unutma!
    public void DealDamage()
    {
        if (attackPoint == null) return;

        // attackPoint merkezli, attackRange yarýçaplý bir küre çiz ve Boss katmanýndakileri bul
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, bossLayer);

        foreach (Collider enemy in hitEnemies)
        {
            // Kendimize vurmamak için güvenlik kontrolü
            if (enemy.gameObject == gameObject) continue;

            // Çarptýðýmýz objede BossHealth var mý?
            BossHealth bossHealth = enemy.GetComponentInParent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(baseDamage);

                // (Ýsteðe baðlý) Konsolda çalýþtýðýný görmek için:
                // Debug.Log("Dummy, Boss'a hasar verdi: " + baseDamage);
            }
        }
    }

    // Editörde saldýrý menzilini kýrmýzý bir küre olarak görmek için
    private void OnDrawGizmos()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}