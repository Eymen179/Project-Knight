using UnityEngine;

public class BossAttackSystem : MonoBehaviour
{
    [Header("Referanslar")]
    public EnemyStats bossStats;
    public LayerMask playerLayer;

    [Header("Vuruþ Noktalarý (Hitboxes)")]
    public Transform basicAttackPoint;    // Temel kýlýç kombolarý için
    public Transform specialAttackPoint;  // Özel saldýrýlar (Alan hasarý vb.) için

    [Header("Durum Makinesi (State Lock)")]
    [Tooltip("Eðer true ise Boss þu an bir eylemde, araya baþka iþlem giremez!")]
    public bool isActionLocked = false;

    // --- ANIMATION EVENT: TEMEL KOMBO HASARI ---
    // Temel saldýrý animasyonlarýnda kýlýcýn karaktere deyeceði anlarda (Keyframe) çaðrýlýr.
    public void DealBasicDamage()
    {
        if (bossStats == null || basicAttackPoint == null) return;

        // Temel hasar deðerini boss'tan çek[cite: 10]
        ApplyDamage(basicAttackPoint.position, bossStats.attackRange, bossStats.attackDamage);
    }

    // --- ANIMATION EVENT: ÖZEL SALDIRI HASARI ---
    // Özel saldýrý animasyonlarýnýn vuruþ anlarýnda çaðrýlýr. Parametre olarak Index alýr (0, 1, 2, 3)
    public void DealSpecialDamage(int animationIndex)
    {
        if (bossStats == null || specialAttackPoint == null) return;

        int specialIndex = animationIndex - 1;

        // EnemyStats'a eklediðimiz diziden o saldýrýya özel hasarý al
        int damageAmount = 0;
        if (bossStats.specialAttackDamages != null && specialIndex < bossStats.specialAttackDamages.Length)
        {
            damageAmount = bossStats.specialAttackDamages[specialIndex];
        }
        else
        {
            Debug.LogWarning("EnemyStats içinde Special Attack hasarlarý ayarlanmamýþ!");
            damageAmount = bossStats.attackDamage * 2; // Hata durumunda varsayýlan x2 hasar
        }

        // Özel saldýrýlar genelde daha geniþ alan vurduðu için menzili biraz daha büyük yapýyoruz
        ApplyDamage(specialAttackPoint.position, bossStats.attackRange * 1.5f, damageAmount);
    }

    // --- ORTAK HASAR ÝÞLEMCÝSÝ ---
    private void ApplyDamage(Vector3 point, float range, int damage)
    {
        // Önceki sistemle ayný mantýk: Belirlenen küre içindeki PlayerLayer objelerini bul[cite: 7]
        Collider[] hitPlayers = Physics.OverlapSphere(point, range, playerLayer);

        foreach (Collider playerCol in hitPlayers)
        {
            PlayerHealth pHealth = playerCol.GetComponentInParent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(damage);
            }
        }
    }

    // --- STATE KÝLÝTLEME (ANIMATION EVENT ÝLE ÇAÐRILACAK) ---
    // Blok veya herhangi bir saldýrý animasyonunun ÝLK karesine bu fonksiyonu koymalýsýn.
    public void LockAction()
    {
        isActionLocked = true;
    }

    // Blok veya herhangi bir saldýrý animasyonunun SON karesine bu fonksiyonu koymalýsýn.
    public void UnlockAction()
    {
        isActionLocked = false;
    }

    // --- GIZMO ÇÝZÝMÝ ---
    private void OnDrawGizmos()
    {
        if (bossStats == null) return;

        // Temel saldýrý menzili (Kýrmýzý)
        Gizmos.color = Color.red;
        if (basicAttackPoint != null) Gizmos.DrawWireSphere(basicAttackPoint.position, bossStats.attackRange);
        // Özel saldýrý menzili (Sarý)
        Gizmos.color = Color.yellow;
        if (specialAttackPoint != null) Gizmos.DrawWireSphere(specialAttackPoint.position, bossStats.attackRange * 1.5f);
    }
}