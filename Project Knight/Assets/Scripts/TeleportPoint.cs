using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    [Header("Iþýnlanma Ayarlarý")]
    [Tooltip("Bu portaldan geçince hangi sahne yüklenecek?")]
    // Kendi yazdýðýn enum'ý burada çaðýrýyoruz. Inspector'da þýk bir liste olacak.
    public SceneController.GameScenes destinationScene;

    // Karakter portalýn içine girdiðinde (temas ettiðinde) tetiklenir
    private void OnTriggerEnter(Collider other)
    {
        // Çarpan obje "Player" etiketine (Tag) sahip mi kontrol et
        if (other.CompareTag("Player"))
        {
            Debug.Log($"{destinationScene} sahnesine ýþýnlanýlýyor...");
            // --- YENÝ: Sahneyi yok etmeden HEMEN ÖNCE envanteri ölümsüz köprüye (SceneController) kaydet! ---
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.SaveInventory();
            }
            // Senin yazdýðýn merkezi SceneController'a talimat gönder
            SceneController.Instance.LoadScene(destinationScene);
        }
    }
}
