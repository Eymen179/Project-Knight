using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // 1. Yeni Input Sistemi için bu satýrý ekle

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactionDistance = 3f;

    // 2. [E] tuþu için Input Action referansý
    [SerializeField] private InputActionReference interactAction;

    private ItemPickup currentItem;

    private void OnEnable()
    {
        // 3. Eylemi (Action) etkinleþtir
        interactAction.action.Enable();
        // 4. "performed" (tuþa basýldýðýnda) event'ine OnInteractPerformed fonksiyonunu baðla
        interactAction.action.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        // 5. Eylemi devre dýþý býrak ve event baðlantýsýný kaldýr (hafýza sýzýntýsýný önler)
        interactAction.action.Disable();
        interactAction.action.performed -= OnInteractPerformed;
    }

    void Start()
    {
        if (UIManager.Instance.txtPrompt) UIManager.Instance.txtPrompt.gameObject.SetActive(false);
    }

    void Update()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        ItemPickup detectedItem = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            // 1. KONTROL: Baktýðýmýz þey kendi vücudumuzun (elimizin) parçasý DEÐÝLSE devam et
            if (!hit.transform.IsChildOf(transform))
            {
                if (hit.collider.TryGetComponent<ItemPickup>(out ItemPickup item))
                {
                    detectedItem = item;
                }
            }
        }

        // 2. KONTROL: UI Güncelleme Mantýðý (Düzeltilen Kýsým)

        // Eðer geçerli bir eþya algýlandýysa...
        if (detectedItem != null)
        {
            // Ve bu eþya bir önceki baktýðýmýzdan farklýysa...
            if (detectedItem != currentItem)
            {
                currentItem = detectedItem;
                UIManager.Instance.txtPrompt.text = $"[E] Al \n{currentItem.item.itemName}";
                UIManager.Instance.txtPrompt.gameObject.SetActive(true);
            }
        }
        else // Eðer hiçbir eþya algýlanmadýysa (veya eþya az önce silindiyse)...
        {
            // Referansý temizle ve yazýyý zorla kapat
            currentItem = null;
            UIManager.Instance.txtPrompt.gameObject.SetActive(false);
        }

        // Not: Yeni input sistemine geçtiðimiz için burada tuþ kontrolü yok,
        // OnInteractPerformed fonksiyonu o iþi yapýyor.
    }

    // 7. Tuþa basýldýðýnda (event tetiklendiðinde) çalýþacak fonksiyon
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // Sadece o an baktýðýmýz bir eþya varsa toplama iþlemi yap
        if (currentItem != null)
        {
            currentItem.Pickup();
        }
    }
}