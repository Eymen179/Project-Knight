using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageFlasher : MonoBehaviour
{
    [Header("Parlama Ayarlarý")]
    [Tooltip("Oluþturduðumuz Unlit (Iþýksýz) kýrmýzý materyali buraya sürükleyin")]
    public Material flashMaterial;
    public float flashDuration = 0.15f;

    // Her parçanýn kendi orijinal materyallerini saklamak için hafýza (Dictionary)
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Renderer[] renderers;
    private Coroutine flashCoroutine;

    void Start()
    {
        // Karakterin üzerindeki TÜM renderer'larý (gövde, kýlýç, gözler vs.) bul
        renderers = GetComponentsInChildren<Renderer>();

        // Baþlangýçta tüm orijinal materyalleri (Synty shader'lý hallerini) hafýzaya al
        foreach (Renderer r in renderers)
        {
            originalMaterials.Add(r, r.materials);
        }
    }

    public void Flash()
    {
        if (flashMaterial == null)
        {
            Debug.LogWarning("Flash Material atanmamýþ!");
            return;
        }

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // 1. AÞAMA: Tüm parçalarý kýrmýzý (Flash) materyaline çevir
        foreach (Renderer r in renderers)
        {
            // Eðer objenin birden fazla materyal yuvasý varsa (Örn: Göz ve Kafa ayný mesh üzerindeyse)
            // Hepsini kapsayacak kadar Flash materyali doldur
            Material[] flashArray = new Material[r.materials.Length];
            for (int i = 0; i < flashArray.Length; i++)
            {
                flashArray[i] = flashMaterial;
            }
            r.materials = flashArray;
        }

        // 2. AÞAMA: Parlama süresi kadar bekle
        yield return new WaitForSeconds(flashDuration);

        // 3. AÞAMA: Hafýzadaki orijinal Synty materyallerine geri dön
        foreach (Renderer r in renderers)
        {
            if (originalMaterials.ContainsKey(r))
            {
                r.materials = originalMaterials[r];
            }
        }
    }
}