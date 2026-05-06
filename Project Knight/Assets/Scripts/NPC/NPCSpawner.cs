using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro Kütüphanesi

public class NPCSpawner : MonoBehaviour
{
    public TextMeshProUGUI txtDebugQuota;

    [Header("NPC Settings")]
    public GameObject NPCPrefab;

    [Header("Spawn & Pool Settings")]
    public Transform[] spawnPoints;
    public int NPCCount = 5;
    public int totalQuota = 10;
    public float respawnDelay = 3f;

    [Header("Optimization (Distance) Settings")]
    public float activationDistance = 50f;
    private Transform playerTarget;
    private bool isSpawningActive = false;

    [Header("Wander Limit")]
    [Tooltip("0 býrakýlýrsa EnemyStats içindeki wanderRadius kullanýlýr.")]
    public float spawnRadius = 0f;

    private List<GameObject> npcPool = new List<GameObject>();

    //NPC Kontrol Listeleri
    private HashSet<GameObject> respawningNPCs = new HashSet<GameObject>();
    private HashSet<GameObject> countedDeadNPCs = new HashSet<GameObject>();

    private int currentQuota; //Kalan toplam kota

    private void Start()
    {
        currentQuota = totalQuota;

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) playerTarget = player.transform;

        //Kotali havuz ile kotaya gore NPC spawnla.
        int poolSize = Mathf.Min(NPCCount, totalQuota);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject newNPC = Instantiate(NPCPrefab, transform.position, Quaternion.identity);
            newNPC.SetActive(false);
            npcPool.Add(newNPC);
        }
    }

    private void Update()//Optimizasyon ayari
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

    //Aktif kalan NPC'leri uyandiran metot
    private void ActivatePool()
    {
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
    //Aktivasyon alanina girip cikmaya bagli olarak NPC'leri ayarlayan metot
    private void DeactivatePool()
    {
        foreach (GameObject npc in npcPool)
        {
            npc.SetActive(false);
        }
        respawningNPCs.Clear();
        countedDeadNPCs.Clear();
        StopAllCoroutines();
    }

    private void CheckAndRespawnDeadNPCs()
    {
        foreach (GameObject npc in npcPool)
        {
            EnemyHealth health = npc.GetComponent<EnemyHealth>();

            //NPC OLDUGU ANDA KOTAYI DUSUR VE YAZIYI GUNCELLE
            if (health != null && health.isDead && !countedDeadNPCs.Contains(npc))
            {
                currentQuota--;
                countedDeadNPCs.Add(npc); //Bu NPC sayildi, bir daha sayma.
                UpdateDebugTexts();
            }

            //NPC 5 SANÝYE SONRA SAHNEDEN SILINDIGINDE YERÝNE YENÝSÝNÝ CAGIR
            if (!npc.activeInHierarchy && !respawningNPCs.Contains(npc) && countedDeadNPCs.Contains(npc))
            {
                //Sahne kotasi - Toplam kota karsilastirmasi
                if (currentQuota >= NPCCount)
                {
                    StartCoroutine(RespawnRoutine(npc));
                }
                else
                {
                    respawningNPCs.Add(npc);
                }
            }
        }
    }

    //Respawnlama dongusu
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

    //Havuzdan NPC'leri cikarip spawnlayan metot
    private void SpawnNPCFromPool(GameObject npc)
    {
        if (spawnPoints.Length == 0) return;

        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        npc.transform.position = randomPoint.position;
        npc.transform.rotation = randomPoint.rotation;

        npc.SetActive(true);
        countedDeadNPCs.Remove(npc);
        npc.SendMessage("ResetNPC", SendMessageOptions.DontRequireReceiver);

        EnemyAI ai = npc.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.spawnerWanderRadius = this.spawnRadius;
        }

        UpdateDebugTexts();
    }

    //DEBUG UI UPDATE
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