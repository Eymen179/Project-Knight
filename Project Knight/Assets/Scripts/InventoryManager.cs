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

        // Envanter açýldýðýnda fareyi serbest býrak, kapandýðýnda kilitle
        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            player.GetComponent<PlayerCombatManager>().enabled = false;
            player.GetComponent<PlayerMovement>().enabled = false;

            UIManager.Instance.toolBarBarrier.enabled = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            player.GetComponent<PlayerCombatManager>().enabled = true;
            player.GetComponent<PlayerMovement>().enabled = true;

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

    // --- YENÝ: Eþya Yere Atma Sistemi ---
    public void DropItem(InventoryItem itemUI)
    {
        // 1. Hangi itemi atýyoruz?
        Item itemToDrop = itemUI.item;

        // 2. Item'in 3D modeli (prefabý) var mý?
        if (itemToDrop.itemObject != null)
        {
            // Karakteri bul (PlayerMovement scripti olan objeyi bul)
            Transform playerTransform = FindFirstObjectByType<PlayerMovement>().transform;

            // Eþyayý karakterin biraz önünde ve yukarýsýnda oluþtur
            Vector3 dropPosition = playerTransform.position + (playerTransform.forward * 1.5f) + (Vector3.up * 1f);

            // Prefab'ý sahneye oluþtur (Instantiate)
            GameObject droppedObject = Instantiate(itemToDrop.itemObject, dropPosition, Quaternion.identity);

            // 3. Oluþan objenin 'ItemPickup' scriptini ayarla
            // Böylece yerde duran objenin hangi eþya olduðunu bilecek ve tekrar alabileceðiz.
            if (droppedObject.TryGetComponent<ItemPickup>(out ItemPickup pickupScript))
            {
                pickupScript.item = itemToDrop;
            }
            else
            {
                Debug.LogWarning("DÝKKAT: Attýðýn prefab'ýn üzerinde ItemPickup scripti yok! Tekrar toplanamaz.");
            }

            // 4. Fizik ekle (Eðer prefabda Rigidbody yoksa havada asýlý kalmasýn)
            if (!droppedObject.GetComponent<Rigidbody>())
            {
                Rigidbody rb = droppedObject.AddComponent<Rigidbody>();
                rb.AddForce(playerTransform.forward * 3f, ForceMode.Impulse); // Hafifçe ileri fýrlat
            }
        }

        // 5. UI Güncellemesi (Sayýsý düþür veya yok et)
        itemUI.count--;
        if (itemUI.count <= 0)
        {
            Destroy(itemUI.gameObject); // Eþya bittiyse UI'dan sil
        }
        else
        {
            itemUI.RefreshCount(); // Bitmediyse sayýsýný güncelle
        }

        /*kutsal*/
        EquipmentManager equipmentManager = FindFirstObjectByType<EquipmentManager>();
        if (equipmentManager != null)
        {
            equipmentManager.ValidateEquipment();
        }
    }
}