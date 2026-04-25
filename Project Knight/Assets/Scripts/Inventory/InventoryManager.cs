using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 1. Input Sistemi için eklendi

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public GameObject inventoryItemPrefab;

    public GameObject player;

    [Header("Slot Listeleri")]
    public List<InventorySlot> swordSlots = new List<InventorySlot>();
    public List<InventorySlot> otherSlots = new List<InventorySlot>();

    [Header("Ekipman Slotu")]
    public InventorySlot toolbarSwordSlot;

    // --- YENÝ EKLENDÝ: Envanter Aç/Kapa ---
    [Header("UI Toggling")]
    [SerializeField] private GameObject mainInventoryGroup; // Ana envanter panelini buraya sürükle
    [SerializeField] private InputActionReference toggleInventoryAction; // "TAB" tuþu için action
    private bool isInventoryOpen = false;
    // --- BÝTTÝ ---

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // --- YENÝ EKLENDÝ: Baþlangýç Ayarlarý ---
    void Start()
    {
        // 1. Oyun baþladýðýnda envanterin kapalý olduðundan emin ol
        mainInventoryGroup.SetActive(false); // Prefab'da 'm_IsActive: 0' [cite: 48] olarak ayarlý, ama bu bir güvence.
        isInventoryOpen = false;

        // 2. 3D oyun için fareyi kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    // --- BÝTTÝ ---

    // --- YENÝ EKLENDÝ: Input Eventleri ---
    private void OnEnable()
    {
        toggleInventoryAction.action.Enable();
        toggleInventoryAction.action.performed += OnToggleInventoryPerformed;
    }

    private void OnDisable()
    {
        toggleInventoryAction.action.Disable();
        toggleInventoryAction.action.performed -= OnToggleInventoryPerformed;
    }

    // "TAB" tuþuna basýldýðýnda bu fonksiyon çalýþacak
    private void OnToggleInventoryPerformed(InputAction.CallbackContext context)
    {
        // Durumu tersine çevir (açýksa kapa, kapalýysa aç)
        isInventoryOpen = !isInventoryOpen;
        mainInventoryGroup.SetActive(isInventoryOpen);

        // Envanter açýldýðýnda fareyi serbest býrak, karakter özelliklerini ve aktif animasyonu durdur.
        if (isInventoryOpen)
        {
            CursorVisibility(true);

            player.GetComponent<PlayerCombatManager>().enabled = false;
            player.GetComponent<PlayerMovement>().enabled = false;
            player.GetComponent<PlayerInteraction>().enabled = false;

            player.GetComponent<Animator>().SetFloat("speed", 0f);

            UIManager.Instance.toolBarBarrier.enabled = true;
        }
        else// Envanter açýldýðýnda fareyi kilitle, karakter özelliklerini geri aktif et.
        {
            CursorVisibility(false);

            player.GetComponent<PlayerCombatManager>().enabled = true;
            player.GetComponent<PlayerMovement>().enabled = true;
            player.GetComponent<PlayerInteraction>().enabled = true;

            UIManager.Instance.toolBarBarrier.enabled = false;
        }
    }
    // --- BÝTTÝ ---

    // Eþya ekleme ana fonksiyonu (Bu kodda deðiþiklik yok)
    public bool AddItem(Item itemToAdd)
    {
        List<InventorySlot> targetSlots = (itemToAdd.itemType == Item.ItemType.Sword) ? swordSlots : otherSlots;

        if (itemToAdd.isStackable)
        {
            foreach (InventorySlot slot in targetSlots)
            {
                if (slot.transform.childCount > 0)
                {
                    InventoryItem itemInSlot = slot.transform.GetChild(0).GetComponent<InventoryItem>();
                    if (itemInSlot.item == itemToAdd)
                    {
                        itemInSlot.count++;
                        itemInSlot.RefreshCount();
                        return true;
                    }
                }
            }
        }

        foreach (InventorySlot slot in targetSlots)
        {
            if (slot.transform.childCount == 0)
            {
                GameObject newItemGO = Instantiate(inventoryItemPrefab, slot.transform);
                InventoryItem newInventoryItem = newItemGO.GetComponent<InventoryItem>();
                newInventoryItem.InitializeItem(itemToAdd);
                return true;
            }
        }

        Debug.Log(itemToAdd.itemName + " için envanter dolu!");
        return false;
    }
    public void DropItem(InventoryItem itemUI)
    {
        Item itemToDrop = itemUI.item;

        if (itemToDrop.itemObject != null)
        {
            Transform playerTransform = FindFirstObjectByType<PlayerMovement>().transform;

            // 1. Eþyanýn ilk çýkýþ noktasýný (Karakterin biraz önü) belirle
            Vector3 dropPosition = playerTransform.position + (playerTransform.forward * 1.5f) + (Vector3.up * 1f);

            // 2. YERÝ BUL (Minecraft stili için kritik)
            // Eðer oyuncu zýplarken eþyayý atarsa havada asýlý kalmasýn diye aþaðý doðru 10 metrelik bir ýþýn atýyoruz
            if (Physics.Raycast(dropPosition, Vector3.down, out RaycastHit hit, 10f))
            {
                // Yeri bulursak, eþyanýn merkezini yerin tam 0.5 metre yukarýsýna sabitliyoruz
                dropPosition = hit.point + (Vector3.up * 0.5f);
            }

            // 3. Eþyayý sahnede oluþtur
            GameObject droppedObject = Instantiate(itemToDrop.itemObject, dropPosition, Quaternion.identity);           

            if (droppedObject.TryGetComponent<ItemPickup>(out ItemPickup pickupScript))
            {
                pickupScript.item = itemToDrop;
            }
            else
            {
                Debug.LogWarning("DÝKKAT: Attýðýn prefab'ýn üzerinde ItemPickup scripti yok!");
            }

            // Layer ayarý
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            SetLayerRecursively(droppedObject, interactableLayer);

            // 4. FÝZÝÐÝ ÝPTAL ET, ANÝMASYONU BAÞLAT
            // Artýk fýrlatma (AddForce) istemiyoruz. Eðer prefabda Rigidbody varsa siliyoruz.
            Rigidbody rb = droppedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Destroy(rb); // Fizik motoruyla iþimiz yok, SimpleGemsAnim halledecek
            }

            // Objede süzülme scripti yoksa otomatik ekle ve ayarlarýný aç
            if (!droppedObject.TryGetComponent<Benjathemaker.SimpleGemsAnim>(out Benjathemaker.SimpleGemsAnim anim))
            {
                anim = droppedObject.AddComponent<Benjathemaker.SimpleGemsAnim>();
            }
            
            // Animasyon özelliklerini kod üzerinden aktif et
            anim.isRotating = true;
            anim.rotateY = true;
            anim.isFloating = true;
            anim.floatHeight = 0.25f; // Ne kadar yukarý/aþaðý sekeceði
            anim.floatSpeed = 0.4f;    // Sekme hýzý
        }

        // 5. UI Güncellemesi
        itemUI.count--;
        if (itemUI.count <= 0)
        {
            Destroy(itemUI.gameObject); 
        }
        else
        {
            itemUI.RefreshCount(); 
        }

        EquipmentManager equipmentManager = FindFirstObjectByType<EquipmentManager>();
        if (equipmentManager != null)
        {
            equipmentManager.ValidateEquipment();
        }
    }
    public void CursorVisibility(bool isVisible)
    {
        Cursor.visible = isVisible;

        if (isVisible)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        UIManager.Instance.crosshair.gameObject.SetActive(!isVisible);
    }
    // Bu fonksiyonu EquipmentManager'da da kullanacaðýz, o yüzden public ve static yapabilirsin
    // veya her iki scriptin içine de kopyalayabilirsin. Ben static öneririm.
    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}