using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro Kütüphanesi

public class NPCSpawner : MonoBehaviour
{
    public TextMeshProUGUI txtDebugQuota;

    [Header("NPC Ayarlarý")]
    public GameObject NPCPrefab;

    [Header("Spawn & Havuz Ayarlarý")]
    public Transform[] spawnPoints;
    public int NPCCount = 5;      // Sahnede ayný anda olabilecek MAKSÝMUM NPC
    public int totalQuota = 10;   // Spawner'ýn üreteceði toplam NPC kotasý
    public float respawnDelay = 3f;

    [Header("Optimizasyon (Mesafe) Ayarlarý")]
    public float activationDistance = 50f;
    private Transform playerTarget;
    private bool isSpawningActive = false;

    private List<GameObject> npcPool = new List<GameObject>();

    // Mantýk Kontrol Listeleri
    private HashSet<GameObject> respawningNPCs = new HashSet<GameObject>();
    private HashSet<GameObject> countedDeadNPCs = new HashSet<GameObject>();

    private int currentQuota; // Kalan toplam kotayý takip eder

    private void Start()
    {
        currentQuota = totalQuota;

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) playerTarget = player.transform;

        // 1. HAVUZU OLUÞTUR (AKILLI HAVUZ)
        // Eðer kota 10, max NPC 5 ise -> Havuz 5 kapasiteli olur. 
        // Eðer kota 3, max NPC 5 ise -> Havuz boþuna 5 olmaz, 3 kapasiteli olur.
        int poolSize = Mathf.Min(NPCCount, totalQuota);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject newNPC = Instantiate(NPCPrefab, transform.position, Quaternion.identity);
            newNPC.SetActive(false);
            npcPool.Add(newNPC);
        }
    }

    private void Update()
    {
        if (playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= activationDistance)
        {
            if (!isSpawningActive)
            {
                isSpawningActive = true;
                ActivatePool();
            }

            CheckAndRespawnDeadNPCs();
        }
        else
        {
            if (isSpawningActive)
            {
                isSpawningActive = false;
                DeactivatePool();
            }
        }
    }

    private void ActivatePool()
    {
        // Sadece kalan kota kadar NPC uyandýrýlýr (Örn: Kota 4 kaldýysa 5. NPC uyanmaz)
        int spawnTarget = Mathf.Min(NPCCount, currentQuota);
        int spawned = 0;

        foreach (GameObject npc in npcPool)
        {
            if (spawned < spawnTarget)
            {
                SpawnNPCFromPool(npc);
                spawned++;
            }
        }
    }

    private void DeactivatePool()
    {
        foreach (GameObject npc in npcPool)
        {
            npc.SetActive(false);
        }
        respawningNPCs.Clear();
        countedDeadNPCs.Clear(); // Uzaklaþýnca ölüm kayýtlarýný temizle ki geri dönünce odadaki düþmanlar baþtan baþlasýn
        StopAllCoroutines();
    }

    private void CheckAndRespawnDeadNPCs()
    {
        foreach (GameObject npc in npcPool)
        {
            EnemyHealth health = npc.GetComponent<EnemyHealth>();

            // 1. AÞAMA: NPC ÖLDÜÐÜ ANDA KOTAYI DÜÞÜR VE YAZIYI GÜNCELLE
            if (health != null && health.isDead && !countedDeadNPCs.Contains(npc))
            {
                currentQuota--;
                countedDeadNPCs.Add(npc); // Bu NPC sayýldý, bir daha sayma
                UpdateDebugTexts();
            }

            // 2. AÞAMA: NPC 5 SANÝYE SONRA SAHNEDEN SÝLÝNDÝÐÝNDE YERÝNE YENÝSÝNÝ ÇAÐIR
            if (!npc.activeInHierarchy && !respawningNPCs.Contains(npc) && countedDeadNPCs.Contains(npc))
            {
                // Eðer kalan kota, sahnede bulunmasý gereken max sayýdan büyük veya eþitse yenisini doður
                if (currentQuota >= NPCCount)
                {
                    StartCoroutine(RespawnRoutine(npc));
                }
                else
                {
                    // Kota yetersizse bu NPC tamamen ölü kalacak. 
                    // Update döngüsünü meþgul etmemesi için diriltme listesine atýyoruz ama diriltmiyoruz.
                    respawningNPCs.Add(npc);
                }
            }
        }
    }

    private IEnumerator RespawnRoutine(GameObject npc)
    {
        respawningNPCs.Add(npc);
        yield return new WaitForSeconds(respawnDelay);

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

        npc.transform.position = randomPoint.position;
        npc.transform.rotation = randomPoint.rotation;

        npc.SetActive(true);
        countedDeadNPCs.Remove(npc); // Yeni hayata baþladý, eski ölüm kaydýný sil
        npc.SendMessage("ResetNPC", SendMessageOptions.DontRequireReceiver);

        UpdateDebugTexts(); // Doðduðunda üzerinde güncel kota yazsýn
    }

    // --- DEBUG UI GÜNCELLEME ---
    private void UpdateDebugTexts()
    {
        foreach (GameObject npc in npcPool)
        {
            EnemyHealth health = npc.GetComponent<EnemyHealth>();
            if (health != null && txtDebugQuota != null)
            {
                txtDebugQuota.text = currentQuota.ToString();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}