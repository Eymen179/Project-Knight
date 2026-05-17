using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        // Eðer bir SceneController varsa ve içi boþ deðilse (Yani bir kapýdan geçerek geldiysek)
        if (SceneController.Instance != null && !string.IsNullOrEmpty(SceneController.Instance.targetSpawnPointID))
        {
            // Sahnedeki tüm SpawnPoint scriptli objeleri bul
            SpawnPoint[] allSpawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

            foreach (SpawnPoint sp in allSpawnPoints)
            {
                // Eðer bulduðumuz noktanýn ID'si, kuryemizin getirdiði ID'ye eþitse
                if (sp.spawnPointID == SceneController.Instance.targetSpawnPointID)
                {
                    TeleportPlayer(sp.transform);
                    break;
                }
            }
        }
    }

    //Oyuncu Sahne Gecisi Metodu
    private void TeleportPlayer(Transform targetTransform)
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;

        if (cc != null) cc.enabled = true;
    }
}