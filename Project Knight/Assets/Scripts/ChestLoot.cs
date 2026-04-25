using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChestLoot : MonoBehaviour
{
    [Header("Sandýk Ayarlarý")]
    private Transform lidTransform; // Sandýðýn kapaðý (Child obje)
    [Tooltip("Kapaðýn açýlma açýsý (Modele göre X, Y veya Z ekseninde denemen gerekebilir)")]
    public Vector3 openAngle = new Vector3(-90f, 0f, 0f);
    public bool isOpened = false;

    [Header("Ganimet (Loot) Ayarlarý")]
    public List<GameObject> lootPrefabs; // Ýçinden çýkacak kýlýç, kristal vs.

    public void OpenChest()
    {
        if (isOpened) return; // Zaten açýksa bir daha açma
        isOpened = true;

        // 1. Kapaðý yumuþakça aç
        lidTransform = transform.GetChild(0);

        lidTransform.localRotation = Quaternion.Euler(openAngle);

        // 2. Ganimetleri fýrlat
        SpawnLoot();
    }

    private void SpawnLoot()
    {
        if (lootPrefabs.Count == 0) return;

        foreach (GameObject loot in lootPrefabs)
        {
            // Eþyayý sandýðýn biraz önüne ve yukarýsýna spawnla
            Vector3 spawnPos = transform.position + (transform.forward * 1.5f) + (Vector3.up * 1f);

            GameObject spawnedLoot = Instantiate(loot, spawnPos, Quaternion.identity);

            // Ganimetin üzerinde Rigidbody varsa sandýktan "fýrlama" efekti ver
        }
    }
}