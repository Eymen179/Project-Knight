using UnityEngine;
using System.Collections;

public class DummyDefensive : MonoBehaviour
{
    [Header("Savunma Ayarlarý")]
    public int maxBlocks = 3;         // Kaç vuruþ bloklayacak
    public float stunDuration = 2f;   // Gardý kýrýlýnca kaç saniye sersemleyecek

    private int currentBlocks;
    private bool isStunned = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentBlocks = maxBlocks;

        // Eðitim baþlar baþlamaz blok animasyonunu aktif et
        if (animator != null) animator.SetBool("isBlocking", true);
    }

    // DummyHealth hasar almadan önce bu fonksiyonu çaðýrýr
    public bool TryBlock()
    {
        // Kukla sersemlemiþse blok yapamaz, doðrudan hasar alýr
        if (isStunned) return false;

        currentBlocks--;
        Debug.Log("Dummy blok yaptý! Kalan hak: " + currentBlocks);

        // Kalan hak 0'a ulaþtýysa sersemlet
        if (currentBlocks <= 0)
        {
            StartCoroutine(StunRoutine());
        }

        // True dönerek DummyHealth'e "Ben blokladým, candan düþme" diyoruz
        return true;
    }

    private IEnumerator StunRoutine()
    {
        isStunned = true;

        if (animator != null)
        {
            animator.SetBool("isBlocking", false); // Gardý indir
            animator.SetTrigger("stun");           // Sersemleme animasyonuna geç
        }

        // 2 saniye boyunca Boss'tan dayak yemeye açýk hale gelir
        yield return new WaitForSeconds(stunDuration);

        // Süre bitince kendine gelir ve tekrar gardýný alýr
        currentBlocks = maxBlocks;
        isStunned = false;

        if (animator != null) animator.SetBool("isBlocking", true);
    }

    // ML-Agent her turu baþa sardýðýnda kuklanýn blok haklarýný da baþa sarmalýyýz
    public void ResetDefensive()
    {
        StopAllCoroutines(); // Eðer sersemleme ortasýnda tur bittiyse zamanlayýcýyý durdur
        isStunned = false;
        currentBlocks = maxBlocks;

        if (animator != null)
        {
            animator.Play("Standing_Idle"); // Önce temiz bir duruþa geç
            animator.SetBool("isBlocking", true); // Sonra hemen gardýný al
        }
    }
}