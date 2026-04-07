using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Ayarlarý")]
    public GameObject NPCPrefab;      // Üretilecek NPC çeþidinin prefabý

    [Header("Spawn Ayarlarý")]
    public Transform[] spawnPoints;   // Bu spawner'a ait çýkýþ noktalarý
    public int NPCCount = 5;          // Sahnede tutulacak maksimum sayý
    public float respawnDelay = 3f;   // Ölümden sonra yenisinin çýkma süresi

    private int activeNPCCount;
    private List<GameObject> spawnedNPCs = new List<GameObject>();
    private int pendingSpawns = 0;

    private void Start()
    {
        // Oyun baþladýðýnda doðrudan spawn iþlemini yap (Yakýnlaþma kontrolü eklenene kadar böyle kalacak)
        for (int i = 0; i < NPCCount; i++)
        {
            SpawnSingleNPC();
        }
    }
    private void Update()
    {
        // Ölen NPC'leri listeden temizle
        spawnedNPCs.RemoveAll(npc => npc == null);
        activeNPCCount = spawnedNPCs.Count;

        // Eksik varsa zamanlayýcýyý baþlat
        if (activeNPCCount + pendingSpawns < NPCCount)
        {
            StartCoroutine(RespawnRoutine());
        }
    }
    private IEnumerator RespawnRoutine()
    {
        pendingSpawns++;
        yield return new WaitForSeconds(respawnDelay);
        SpawnSingleNPC();
        pendingSpawns--;
    }
    private void SpawnSingleNPC()
    {
        if (spawnPoints.Length == 0) return;

        // Atanan noktalardan rastgele birini seç ve prefab'ý oluþtur
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject newNPC = Instantiate(NPCPrefab, randomPoint.position, randomPoint.rotation);

        spawnedNPCs.Add(newNPC);
    }
}