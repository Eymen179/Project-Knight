using UnityEngine;

public class Spikes : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.isDead)
            {
                playerHealth.TakeDamage(10);
            }
        }
        if (collision.gameObject.CompareTag("NPC"))
        {
            EnemyHealth enemyHealth = collision.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null && !enemyHealth.isDead)
            {
                enemyHealth.TakeDamage(10);
            }
        }
    }
}
