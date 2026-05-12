using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    private AudioSource playerAudioSource;

    [Header("Savaþ Sesleri")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip blockSuccessSound;

    [Header("Ayak Sesleri (Genel)")]
    public Transform footTransform;        // Ayak hizasý referansý --> Bunun objesi yok hazýrlanacak.
    public float rayDistance = 0.5f;

    [Header("Zindan (Mesh) Ayarlarý")]
    [Tooltip("Terrain olmayan her zemin için (Zindan) çalýnacak tek ses")]
    public AudioClip dungeonFootstepSound;

    [Header("Map (Terrain) Ayarlarý")]
    [Tooltip("Terrain'deki boya sýrasýna göre sesleri koyun (Örn: 0. Çimen, 1. Toprak, 2. Taþ)")]
    public AudioClip[] footstepByTerrainLayerSounds;

    void Start()
    {
        playerAudioSource = GetComponent<AudioSource>();
        playerAudioSource.spatialBlend = 1f;
        playerAudioSource.minDistance = 2f;
        playerAudioSource.maxDistance = 15f;
    }

    // Animation event üzerinden tetiklenir
    public void PlayFootstepSound()
    {
        AudioClip clipToPlay = dungeonFootstepSound; // Varsayýlan olarak zindan sesi kabul et

        Vector3 rayStart = footTransform != null ? footTransform.position : transform.position + (Vector3.up * 0.1f);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance))
        {
            // Çarptýðýmýz obje bir Terrain mi?
            Terrain terrain = hit.collider.GetComponent<Terrain>();

            if (terrain != null)
            {
                // Terrain ise karakterin altýndaki baskýn dokuyu bul
                int terrainTextureIndex = GetDominantTerrainTexture(rayStart, terrain);

                // Bulunan dokunun indeksi, bizim ses listemizin sýnýrlarý içindeyse o sesi seç
                if (terrainTextureIndex >= 0 && terrainTextureIndex < footstepByTerrainLayerSounds.Length)
                {
                    clipToPlay = footstepByTerrainLayerSounds[terrainTextureIndex];
                }
            }
            // Eðer Terrain deðilse (Mesh ise), clipToPlay zaten dungeonFootstepSound olarak kalýr.
        }

        // Ses seçildiyse çal
        if (clipToPlay != null)
        {
            playerAudioSource.pitch = Random.Range(0.9f, 1.1f); // Hafif kalýnlýk/incelik deðiþimi robotikliði önler
            playerAudioSource.PlayOneShot(clipToPlay);
        }
    }

    // --- TERRAIN MATEMATÝÐÝ (Altýmýzdaki Boyayý Bulma) ---
    private int GetDominantTerrainTexture(Vector3 worldPos, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        // Karakterin dünya pozisyonunu Terrain'in kendi harita koordinatlarýna (Alphamap) çevir
        float mapX = (worldPos.x - terrainPos.x) / terrainData.size.x * terrainData.alphamapWidth;
        float mapZ = (worldPos.z - terrainPos.z) / terrainData.size.z * terrainData.alphamapHeight;

        int x = Mathf.FloorToInt(mapX);
        int z = Mathf.FloorToInt(mapZ);

        // Eðer haritanýn dýþýndaysak güvenlik için 0 dön
        if (x < 0 || z < 0 || x >= terrainData.alphamapWidth || z >= terrainData.alphamapHeight)
            return 0;

        // O noktadaki tüm boyalarýn (texture) karýþým oranlarýný al
        float[,,] splatmapData = terrainData.GetAlphamaps(x, z, 1, 1);
        float[] mix = new float[splatmapData.GetUpperBound(2) + 1];

        for (int i = 0; i < mix.Length; i++)
        {
            mix[i] = splatmapData[0, 0, i];
        }

        // En yüksek orana (baskýn boyaya) sahip olan indeks numarasýný bul
        float maxMix = 0;
        int maxIndex = 0;
        for (int i = 0; i < mix.Length; i++)
        {
            if (mix[i] > maxMix)
            {
                maxIndex = i;
                maxMix = mix[i];
            }
        }

        return maxIndex;
    }

    public void PlayAttackSound() { if (attackSound != null) playerAudioSource.PlayOneShot(attackSound); }
    public void PlayHurtSound() { if (hurtSound != null) playerAudioSource.PlayOneShot(hurtSound); }
    public void PlayBlockSuccessSound() { if (blockSuccessSound != null) playerAudioSource.PlayOneShot(blockSuccessSound); }
}