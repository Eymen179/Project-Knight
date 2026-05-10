using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    private AudioSource playerAudioSource;

    [Header("Player Sesleri")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip footstepSound;
    public AudioClip blockSuccessSound;

    void Start()
    {
        // AudioSource Player'ýn kendi üzerindeki bileþen olmalý!
        playerAudioSource = GetComponent<AudioSource>();

        // 3D ses ayarlarýný koda baðlama (Inspector'dan da yapabilirsin)
        playerAudioSource.spatialBlend = 1f; // 0 = 2D(Global), 1 = 3D(Uzamsal)
        playerAudioSource.minDistance = 2f;  // Sesi tam duyacaðýmýz min mesafe
        playerAudioSource.maxDistance = 15f; // Sesin tamamen kaybolacaðý mesafe
    }

    //Animation event
    public void PlayFootstepSound()
    {
        // Ses uzaktan ve NPC'nin olduðu noktadan gelir, ana sistemi yormaz
        if (footstepSound != null)
        {
            // Adým seslerini biraz rastgele (Pitch) yapmak robotikliði alýr
            playerAudioSource.pitch = Random.Range(0.9f, 1.1f);
            playerAudioSource.PlayOneShot(footstepSound);
        }
    }

    //Animation event
    public void PlayAttackSound()
    {
        if (attackSound != null) playerAudioSource.PlayOneShot(attackSound);
    }
    public void PlayHurtSound()
    {
        if (hurtSound != null) playerAudioSource.PlayOneShot(hurtSound);
    }
    public void PlayBlockSuccessSound()
    {
        if (blockSuccessSound != null) playerAudioSource.PlayOneShot(blockSuccessSound);
    }
}