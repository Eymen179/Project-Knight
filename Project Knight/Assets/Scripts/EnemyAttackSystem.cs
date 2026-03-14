using UnityEngine;

public class EnemyAttackSystem : MonoBehaviour
{
    [Header("Saldýrý Ayarlarý")]
    public EnemyStats stats;           // Hasar deðerini buradan alacak
    public Transform attackPoint;      // NPC'nin önündeki görünmez vuruþ noktasý
    public float attackRange = 1.2f;   // Vuruþ küresinin büyüklüðü
    public LayerMask playerLayer;      // Sadece 'Player' katmanýný arayacak

    // Bu fonksiyonu þimdilik test için, ileride ise Animation Event'ten çaðýracaðýz
    public void DealDamageToPlayer()
    {
        if (stats == null || attackPoint == null) return;

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);

        foreach (Collider playerCol in hitPlayers)
        {
            // Çarptýðýmýz objede veya ebeveyninde PlayerHealth var mý?
            PlayerHealth pHealth = playerCol.GetComponentInParent<PlayerHealth>();

            if (pHealth != null)
            {
                // Varsa ona hasar ver
                pHealth.TakeDamage(stats.attackDamage);
            }
        }
    }

    // Editörde NPC'nin vuruþ küresini kýrmýzý olarak görmek için
    private void OnDrawGizmos()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}