using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [SerializeField] private float knockbackForce = 10f;

    // Private deðiþkenler, ama Start'ta doldurmuyoruz
    private Transform ownerPlayerTransform;
    private Animator ownerAnimator;

    // BU FONKSÝYONU EquipmentManager ÇAÐIRACAK
    public void Setup(Animator anim, Transform playerTransform)
    {
        ownerAnimator = anim;
        ownerPlayerTransform = playerTransform;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Güvenlik Önlemi: Eðer Setup çalýþmadýysa hata vermesin, iþlem yapmasýn
        if (ownerAnimator == null || ownerPlayerTransform == null) return;

        // Kendi kendimize vurmayalým
        if (other.transform.IsChildOf(ownerPlayerTransform)) return;

        if (other.CompareTag("NPC"))
        {
            AnimatorStateInfo stateInfo = ownerAnimator.GetCurrentAnimatorStateInfo(0);

            // Sadece saldýrý animasyonundayken hasar ver
            if (stateInfo.IsTag("Attack"))
            {
                TestDummy enemy = other.GetComponent<TestDummy>();
                if (enemy != null)
                {
                    Vector3 direction = (other.transform.position - ownerPlayerTransform.position).normalized;
                    direction += Vector3.up * 0.5f;

                    enemy.TakeDamage(direction, knockbackForce);
                }
            }
        }
    }
}