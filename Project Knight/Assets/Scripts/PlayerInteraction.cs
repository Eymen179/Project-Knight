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
        // Raycast ve UI gösterme mantýðý Update içinde kalmalý
        // Çünkü her frame nereye baktýðýmýzý bilmemiz gerekiyor.
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        ItemPickup detectedItem = null;
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.TryGetComponent<ItemPickup>(out ItemPickup item))
            {
                detectedItem = item;
            }
        }

        if (detectedItem != currentItem)
        {
            currentItem = detectedItem;
            if (currentItem != null)
            {
                UIManager.Instance.txtPrompt.text = $"[E] {currentItem.item.itemName} Al";
                UIManager.Instance.txtPrompt.gameObject.SetActive(true);
            }
            else
            {
                UIManager.Instance.txtPrompt.gameObject.SetActive(false);
            }
        }

        // 6. Eski Input kontrolü buradan kaldýrýldý.
        // if (currentItem != null && Input.GetKeyDown(KeyCode.E)) ...
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