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
}