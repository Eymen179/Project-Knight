using UnityEngine;

public class EnemyAudioManager : MonoBehaviour
{
    private AudioSource enemyAudioSource;

    [Header("Enemy Sesleri")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip footstepSound;
    public AudioClip blockSuccessSound;

    void Start()
    {
        // AudioSource Enemy'nin kendi üzerindeki bileþen olmalý!
        enemyAudioSource = GetComponent<AudioSource>();

        // 3D ses ayarlarýný koda baðlama (Inspector'dan da yapabilirsin)
        enemyAudioSource.spatialBlend = 1f; // 0 = 2D(Global), 1 = 3D(Uzamsal)
        enemyAudioSource.minDistance = 2f;  // Sesi tam duyacaðýmýz min mesafe
        enemyAudioSource.maxDistance = 15f; // Sesin tamamen kaybolacaðý mesafe
    }

    //Animation event
    public void PlayFootstepSound()
    {
        // Ses uzaktan ve NPC'nin olduðu noktadan gelir, ana sistemi yormaz
        if (footstepSound != null)
        {
            // Adým seslerini biraz rastgele (Pitch) yapmak robotikliði alýr
            enemyAudioSource.pitch = Random.Range(0.9f, 1.1f);
            enemyAudioSource.PlayOneShot(footstepSound);
        }
    }

    //Animation event
    public void PlayAttackSound()
    {
        if (attackSound != null) enemyAudioSource.PlayOneShot(attackSound);
    }
    public void PlayHurtSound()
    {
        if (hurtSound != null) enemyAudioSource.PlayOneShot(hurtSound);
    }
    public void PlayBlockSuccessSound()
    {
        if (blockSuccessSound != null) enemyAudioSource.PlayOneShot(blockSuccessSound);
    }
}