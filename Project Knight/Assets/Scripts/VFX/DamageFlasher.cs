using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageFlasher : MonoBehaviour
{
    [Header("Flash Settings")]
    [Tooltip("Oluþturduðumuz Unlit (Iþýksýz) kýrmýzý materyali buraya sürükleyin")]
    public Material flashMaterial;
    public float flashDuration = 0.15f;

    //Parlayacak objelerin tum materyalleri
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Renderer[] renderers;
    private Coroutine flashCoroutine;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            originalMaterials.Add(r, r.materials);
        }
    }

    //Parlama metodu
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


    //Parlama Coroutine'i
    private IEnumerator FlashRoutine()
    {
        //Renk degisimi
        foreach (Renderer r in renderers)
        {
            Material[] flashArray = new Material[r.materials.Length];
            for (int i = 0; i < flashArray.Length; i++)
            {
                flashArray[i] = flashMaterial;
            }
            r.materials = flashArray;
        }

        //Renk degisimi suresi
        yield return new WaitForSeconds(flashDuration);

        //Orijinal renge donus
        foreach (Renderer r in renderers)
        {
            if (originalMaterials.ContainsKey(r))
            {
                r.materials = originalMaterials[r];
            }
        }
    }
}