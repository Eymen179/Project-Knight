using UnityEngine;

public class DieFloor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Eðer zemine çarpan þey oyuncuysa
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null && !playerHealth.isDead) // Eðer oyuncu zaten ölmediyse
            {
                // Canýný zorla sýfýrla ve UI'ý güncelle (Blok yapmasýný engeller)
                playerHealth.currentHealth = 0;
                playerHealth.UpdateUI();

                // Doðrudan ölüm fonksiyonunu tetikle
                playerHealth.Die();
            }
        }
    }
}