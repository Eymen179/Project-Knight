using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // 1. Yeni Input Sistemi için bu satýrý ekle

public class PlayerInteraction : MonoBehaviour
{
    [Header("Ayarlar")]
    public Camera playerCamera;
    public float interactionDistance = 3f;
    public float interactionRadius = 0.5f; // Ray'in kalýnlýðý (Küre yarýçapý)

    // Hangi katmanlarýn etkileþime girebileceðini seç (Örn: Default, Interactable)
    // Player katmanýný BURADA SEÇMEMELÝSÝN.
    public LayerMask interactableLayers;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    // O an odaklandýðýmýz (baktýðýmýz) eþya
    private ItemPickup currentFocusItem;

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
        interactAction.action.performed -= OnInteractPerformed;
    }

    void Start()
    {
        if (UIManager.Instance && UIManager.Instance.txtPrompt)
            UIManager.Instance.txtPrompt.gameObject.SetActive(false);
    }

    void Update()
    {
        DetectInteractable();

        UpdateInteractionUI();
    }

    private void DetectInteractable()
    {
        // Ekranýn tam ortasýndan bir ýþýn oluþtur
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // SPHERECAST: Raycast'in kalýn hali.
        // ray: Merkez ýþýn
        // interactionRadius: Kalýnlýk
        // out hit: Çarpma bilgisi
        // interactionDistance: Mesafe
        // interactableLayers: Sadece bu katmanlarý gör (Player'ý görmezden gelmek için)
        bool hitSomething = Physics.SphereCast(ray, interactionRadius, out hit, interactionDistance, interactableLayers);

        if (hitSomething)
        {
            // Çarptýðýmýz objede ItemPickup scripti var mý?
            if (hit.collider.TryGetComponent<ItemPickup>(out ItemPickup item))
            {
                // Eðer yeni bir eþyaya baktýysak UI güncelle
                if (currentFocusItem != item)
                {
                    currentFocusItem = item;
                }
                return; // Bulduk, fonksiyondan çýkabiliriz
            }
        }

        // Eðer buraya geldiysek; ya bir þeye çarpmadýk ya da çarptýðýmýz þey eþya deðil.
        if (currentFocusItem != null)
        {
            currentFocusItem = null;
        }
    }
    public void UpdateInteractionUI()
    {
        // 2. KONTROL: UI Güncelleme Mantýðý (Düzeltilen Kýsým)

        // Eðer geçerli bir eþya algýlandýysa...
        if (currentFocusItem != null)
        {
            UIManager.Instance.txtPrompt.text = $"[E] Al \n{currentFocusItem.item.itemName}";
            UIManager.Instance.txtPrompt.gameObject.SetActive(true);
        }
        else // Eðer hiçbir eþya algýlanmadýysa (veya eþya az önce silindiyse)...
        {
            // Referansý temizle ve yazýyý zorla kapat
            currentFocusItem = null;
            UIManager.Instance.txtPrompt.gameObject.SetActive(false);
        }

        // Not: Yeni input sistemine geçtiðimiz için burada tuþ kontrolü yok,
        // OnInteractPerformed fonksiyonu o iþi yapýyor.
    }
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (currentFocusItem != null)
        {
            currentFocusItem.Pickup();

            // Eþyayý aldýktan sonra UI'ý hemen kapatmak için referansý temizle
            // Çünkü obje yok olacak (Destroy edilecek)
            currentFocusItem = null;
        }
    }

    // Editörde SphereCast'in çapýný ve menzilini görmek için (Hata ayýklama)
    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        Gizmos.color = Color.yellow;
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward * interactionDistance;

        // SphereCast'i temsil eden çizgi
        Gizmos.DrawRay(origin, direction);
        // Varýþ noktasýndaki küre (tahmini)
        Gizmos.DrawWireSphere(origin + direction, interactionRadius);
    }
}