using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public Camera playerCamera;
    public float interactionDistance = 3f;
    public float interactionRadius = 0.5f; //Ray Kalinligi

    //Etkilesime girilecek objelerin katmanlari
    public LayerMask interactableLayers;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    //Anlik bakilen objeler
    private ItemPickup currentFocusItem;
    private ChestLoot currentFocusChest;

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

    //Etkilesim Metodu
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
            //Etkilesime girilen obje envantere alinabilir bir item mi?
            if (hit.collider.TryGetComponent<ItemPickup>(out ItemPickup item))
            {
                if (currentFocusItem != item)
                {
                    currentFocusItem = item;
                }
                return;
            }
            //Etkilesime girilen obje bir sandik mi?
            else if (hit.collider.TryGetComponent<ChestLoot>(out ChestLoot chest))
            {
                //Daha once acilmamis sandik kontrolcusu
                if (!chest.isOpened)
                {
                    if (currentFocusChest != chest) currentFocusChest = chest;
                    currentFocusItem = null;
                    return;
                }
            }
        }

        //Etkilesime girilen bir obje yok.
        if (currentFocusItem != null)
        {
            currentFocusItem = null;
        }
        if (currentFocusChest != null)
        {
            currentFocusChest = null;
        }
    }

    //Etkilesim UI'ini guncelleme metodu
    public void UpdateInteractionUI()
    {
        //Envanter Objesi
        if (currentFocusItem != null)
        {
            UIManager.Instance.txtPrompt.text = $"[E] Take \n{currentFocusItem.item.itemName}";
            UIManager.Instance.txtPrompt.gameObject.SetActive(true);
        }
        //Sandýk
        else if (currentFocusChest != null && !currentFocusChest.isOpened)
        {
            UIManager.Instance.txtPrompt.text = $"[E] Open \nChest";
            UIManager.Instance.txtPrompt.gameObject.SetActive(true);
        }
        else
        {
            currentFocusItem = null;
            currentFocusChest = null;
            UIManager.Instance.txtPrompt.gameObject.SetActive(false);
        }
    }

    //Etkilesime Girme Metodu
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        //Envanter Objesini envantere al.
        if (currentFocusItem != null)
        {
            currentFocusItem.Pickup();

            currentFocusItem = null;
        }
        //Sandigi ac.
        else if (currentFocusChest != null && !currentFocusChest.isOpened)
        {
            currentFocusChest.OpenChest();
            currentFocusChest = null;
        }
    }

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