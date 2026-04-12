using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Ayarlarý")]
    public GameObject NPCPrefab;

    [Header("Spawn & Havuz Ayarlarý")]
    public Transform[] spawnPoints;
    public int NPCCount = 5;
    public float respawnDelay = 3f;

    [Header("Optimizasyon (Mesafe) Ayarlarý")]
    public float activationDistance = 50f; // Sis mesafesine göre ayarlanacak
    private Transform playerTarget;
    private bool isSpawningActive = false;

    private List<GameObject> npcPool = new List<GameObject>();
    // Hangi NPC'nin yeniden doðmayý (respawn) beklediðini takip listesi
    private HashSet<GameObject> respawningNPCs = new HashSet<GameObject>();

    private void Start()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) playerTarget = player.transform;

        // 1. HAVUZU OLUÞTUR (Oyun baþýnda tek seferlik Instantiate)
        for (int i = 0; i < NPCCount; i++)
        {
            GameObject newNPC = Instantiate(NPCPrefab, transform.position, Quaternion.identity);
            newNPC.SetActive(false); // Baþlangýçta hepsi uykuda
            npcPool.Add(newNPC);
        }
    }

    private void Update()
    {
        if (playerTarget == null) return;

        // Mesafe Kontrolü
        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= activationDistance)
        {
            if (!isSpawningActive)
            {
                // Oyuncu menzile girdi, havuzdaki uygun NPC'leri uyandýr
                isSpawningActive = true;
                ActivatePool();
            }

            // Menzil içindeysek ölenleri kontrol et ve yeniden doður
            CheckAndRespawnDeadNPCs();
        }
        else
        {
            if (isSpawningActive)
            {
                // Oyuncu uzaklaþtý, tüm NPC'leri uykuya al (Optimizasyon)
                isSpawningActive = false;
                DeactivatePool();
            }
        }
    }

    private void ActivatePool()
    {
        foreach (GameObject npc in npcPool)
        {
            if (!respawningNPCs.Contains(npc))
            {
                SpawnNPCFromPool(npc);
            }
        }
    }

    private void DeactivatePool()
    {
        foreach (GameObject npc in npcPool)
        {
            npc.SetActive(false);
        }
        respawningNPCs.Clear(); // Uzaklaþýnca tüm respawn süreçlerini iptal et
        StopAllCoroutines();
    }

    private void CheckAndRespawnDeadNPCs()
    {
        foreach (GameObject npc in npcPool)
        {
            // Eðer NPC sahnede kapalýysa (ölüp 5 sn sonra kapandýysa) ve bekleme listesinde yoksa
            if (!npc.activeInHierarchy && !respawningNPCs.Contains(npc))
            {
                StartCoroutine(RespawnRoutine(npc));
            }
        }
    }

    private IEnumerator RespawnRoutine(GameObject npc)
    {
        respawningNPCs.Add(npc);
        yield return new WaitForSeconds(respawnDelay);

        // Eðer bekleme süresinde oyuncu uzaklaþýp spawner'ý kapattýysa doðurma
        if (isSpawningActive)
        {
            SpawnNPCFromPool(npc);
        }
        respawningNPCs.Remove(npc);
    }

    private void SpawnNPCFromPool(GameObject npc)
    {
        if (spawnPoints.Length == 0) return;

        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Konumu ayarla
        npc.transform.position = randomPoint.position;
        npc.transform.rotation = randomPoint.rotation;

        // NPC'yi aç
        npc.SetActive(true);

        // YENÝ: Havuzdan uyanan NPC'nin canýný, kanýný, beynini sýfýrlama emri!
        npc.SendMessage("ResetNPC", SendMessageOptions.DontRequireReceiver);
    }

    // Editörde çalýþma menzilini mavi bir küre olarak görmek için
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}