using UnityEngine;

public class WeaponVFX : MonoBehaviour
{
    [Header("Slash Efektleri (Sırasıyla Atayın)")]
    // Artık tek bir Particle değil, bir liste (Array) tutuyoruz
    public ParticleSystem[] slashParticles;

    // Dışarıdan hangi komboda olduğumuzu (index) söyleyeceğiz
    public void PlaySlash(int index)
    {
        // İstediğimiz index listede var mı diye güvenlik kontrolü yapalım
        if (index >= 0 && index < slashParticles.Length && slashParticles[index] != null)
        {
            slashParticles[index].Stop();
            slashParticles[index].Play();
        }
        else
        {
            Debug.LogWarning($"Kılıçta {index}. sıradaki Particle System bulunamadı!");
        }
    }
}