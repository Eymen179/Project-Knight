using UnityEngine;

public class EnemyAttackSystem : MonoBehaviour
{
    [Header("Saldýrý Ayarlarý")]
    public EnemyStats stats;
    public Transform attackPoint;
    public LayerMask playerLayer;

    //Animation Event ile Hasar Verme Metodu
    public void DealDamageToPlayer()
    {
        if (stats == null || attackPoint == null) return;

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, stats.attackRange, playerLayer);

        foreach (Collider playerCol in hitPlayers)
        {
            //Oyuncuyu bul.
            PlayerHealth pHealth = playerCol.GetComponentInParent<PlayerHealth>();

            if (pHealth != null)
            {
                float multiplier = SceneController.Instance != null ? SceneController.Instance.globalDifficultyMultiplier : 1f;

                //Varsa ona hasar ver.
                pHealth.TakeDamage(Mathf.RoundToInt(stats.attackDamage * multiplier));
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, stats.attackRange);
    }
}