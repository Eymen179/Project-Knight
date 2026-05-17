using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChestLoot : MonoBehaviour
{
    [Header("Chest Settings")]
    private Transform lidTransform; //Sandik Kapagi
    [Tooltip("Kapaðýn açýlma açýsý (Modele göre X, Y veya Z ekseninde denemen gerekebilir)")]
    public Vector3 openAngle = new Vector3(-90f, 0f, 0f);
    public bool isOpened = false;

    [Header("Loot Settings")]
    public List<GameObject> lootPrefabs; //Sandik ganimetleri

    //Sandik acma metodu
    public void OpenChest()
    {
        if (isOpened) return;
        isOpened = true;

        lidTransform = transform.GetChild(0);

        lidTransform.localRotation = Quaternion.Euler(openAngle);

        SpawnLoot();
    }

    //Sandik Ganimetlerini Aciga Cikarma Metodu
    private void SpawnLoot()
    {
        if (lootPrefabs.Count == 0) return;

        foreach (GameObject loot in lootPrefabs)
        {
            Vector3 spawnPos = transform.position + (transform.forward * 1.5f) + (Vector3.up * 1f);

            GameObject spawnedLoot = Instantiate(loot, spawnPos, Quaternion.identity);

        }
    }
}