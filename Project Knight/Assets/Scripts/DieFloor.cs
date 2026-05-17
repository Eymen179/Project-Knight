using UnityEngine;

public class DieFloor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null && !playerHealth.isDead)
            {
                playerHealth.currentHealth = 0;
                playerHealth.UpdateUI();

                playerHealth.Die();
            }
        }
    }
}