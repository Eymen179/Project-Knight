using UnityEngine;

public class BossAttackSystem : MonoBehaviour
{
    [Header("Referanslar")]
    public EnemyStats bossStats;
    public LayerMask playerLayer;

    [Header("Vuruþ Noktalarý (Hitboxes)")]
    public Transform basicAttackPoint;
    public Transform specialAttackPoint;

    [Header("Durum Makinesi (State Lock)")]
    public bool isActionLocked = false;

    // --- ML-AGENTS REFERANSI ---
    private BossAgent agent;

    void Start()
    {
        agent = GetComponent<BossAgent>(); // Ajaný bulduk
    }

    public void DealBasicDamage()
    {
        if (bossStats == null || basicAttackPoint == null) return;
        ApplyDamage(basicAttackPoint.position, bossStats.attackRange, bossStats.attackDamage, false);
    }

    public void DealSpecialDamage(int animationIndex)
    {
        if (bossStats == null || specialAttackPoint == null) return;

        int specialIndex = animationIndex - 1;
        int damageAmount = 0;

        if (bossStats.specialAttackDamages != null && specialIndex < bossStats.specialAttackDamages.Length)
        {
            damageAmount = bossStats.specialAttackDamages[specialIndex];
        }
        else
        {
            damageAmount = bossStats.attackDamage * 2;
        }

        ApplyDamage(specialAttackPoint.position, bossStats.attackRange, damageAmount, true);
    }

    private void ApplyDamage(Vector3 point, float range, int damage, bool isSpecialAttack)
    {
        Collider[] hitPlayers;
        if (isSpecialAttack)
        {
            hitPlayers = Physics.OverlapCapsule(point + Vector3.left * 0.5f, point - Vector3.right * 0.5f, range, playerLayer);
        }
        else
        {
            hitPlayers = Physics.OverlapSphere(point, range, playerLayer);
        }
        Debug.Log("Dummy hasar alýyor: ");
        /*foreach (Collider playerCol in hitPlayers)
        {
            PlayerHealth pHealth = playerCol.GetComponentInParent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(damage);

                // ÖDÜL: Verdiði hasarýn %1'i kadar ödül alýr (Örn: 50 vurursa 0.5 puan alýr).
                // Bu sayede özel saldýrýlarýn daha kârlý olduðunu matematiksel olarak öðrenir!
                if (agent != null) agent.AddReward(damage * 0.01f);

                // BÜYÜK ÖDÜL: Eðer oyuncuyu öldürdüyse (Varsayalým ki PlayerHealth içinde currentHealth veya isDead var)
                // Aþaðýdaki kod PlayerHealth sistemine göre uyarlanmalýdýr. Eðer currentHealth 0 ise:
                if (pHealth.currentHealth <= 0)
                {
                    if (agent != null)
                    {
                        agent.SetReward(1.0f);
                        agent.EndEpisode(); // Oyuncuyu öldürdü, tur bitti!
                    }
                }
            }
        }*/
        foreach (Collider col in hitPlayers)
        {
            // Önce gerçek oyuncuyu kontrol et
            PlayerHealth pHealth = col.GetComponentInParent<PlayerHealth>();
            // Sonra eðitim kuklasýný (Dummy) kontrol et
            DummyHealth dHealth = col.GetComponentInParent<DummyHealth>();

            if (pHealth != null)
            {
                pHealth.TakeDamage(damage);
                if (agent != null) agent.AddReward(damage * 0.01f); // Hasar ödülü

                if (pHealth.currentHealth <= 0)
                {
                    if (agent != null)
                    {
                        agent.SetReward(1.0f);
                        agent.EndEpisode(); // Oyuncuyu öldürdü, tur bitti!
                    }
                }
            }
            else if (dHealth != null)
            {
                dHealth.TakeDamage(damage);
                // Eðitim kuklasýna hasar vermek de boss için "doðru" bir harekettir, ödül veriyoruz.
                if (agent != null) agent.AddReward(damage * 0.01f);

                if (dHealth.currentHealth <= 0)
                {
                    if (agent != null)
                    {
                        agent.SetReward(1.0f);
                        agent.EndEpisode(); // Oyuncuyu öldürdü, tur bitti!
                    }
                }
            }
        }
    }

    public void LockAction() { isActionLocked = true; }
    public void UnlockAction() { isActionLocked = false; }

    private void OnDrawGizmos()
    {
        if (bossStats == null) return;
        Gizmos.color = Color.red;
        if (basicAttackPoint != null) Gizmos.DrawWireSphere(basicAttackPoint.position, bossStats.attackRange + 1f);
        Gizmos.color = Color.yellow;
        if (specialAttackPoint != null) Gizmos.DrawWireSphere(specialAttackPoint.position, bossStats.attackRange);
    }
}