using System.Collections;
using UnityEngine;

public class TestDummy : MonoBehaviour
{
    private Rigidbody rb;
    private bool isRecovering = false;

    [Header("Ayarlar")]
    [SerializeField] private float standUpDuration = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Sadece X ve Z rotasyonunu kilitliyoruz (Dik durmasý için)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public void TakeDamage(Vector3 impactDirection, float force)
    {
        if (isRecovering) return;

        // 1. Kýsýtlamalarý Kaldýr
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;

        // 2. Sürtünmeleri azalt (Kaymasý için)
        rb.linearDamping = 0.05f; // Unity 6 öncesi için: rb.drag
        rb.angularDamping = 0.05f; // Unity 6 öncesi için: rb.angularDrag

        // 3. YÖN HESABI (ÖNEMLÝ DEÐÝÞÝKLÝK)
        // Gelen yönü al ama Y (yukarý) bileþenini sýfýrla, sonra biz ekleyelim.
        Vector3 flatDirection = new Vector3(impactDirection.x, 0, impactDirection.z).normalized;

        // Vuruþ yönü + Ciddi bir yukarý kaldýrma kuvveti (0.8f yetmez, 1.5f - 2f yapalým)
        // Vector3.up * 1.5f diyerek havaya dikiyoruz.
        Vector3 finalDirection = (flatDirection + Vector3.up * 1.5f).normalized;

        // 4. FIRLATMA (VelocityChange Kullanýyoruz)
        // VelocityChange, kütleyi (Mass) yoksayar. "force" deðeri direkt hýz olur.
        // Force deðerini bu modda çok yüksek tutma (örn: 10-20 arasý yeterli olabilir).
        rb.AddForce(finalDirection * force, ForceMode.VelocityChange);

        // 5. DÖNME (Torque)
        // Rastgele eksende çok sert döndür
        Vector3 randomRotateDir = Random.onUnitSphere;
        rb.AddTorque(randomRotateDir * force * 5f, ForceMode.VelocityChange);

        StopAllCoroutines();
        StartCoroutine(RecoverRoutine());
    }

    private IEnumerator RecoverRoutine()
    {
        yield return new WaitForSeconds(4f); // 4 saniye yerde kalsýn

        isRecovering = true;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        float time = 0;

        // Kalkarken fiziði biraz sakinleþtir
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;

        while (time < standUpDuration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, time / standUpDuration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;

        // Kýsýtlamalarý geri yükle
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Fizik ayarlarýný normale döndür
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.linearVelocity = Vector3.zero;

        isRecovering = false;
    }
}