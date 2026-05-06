using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 1. Input Sistemi için eklendi

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public GameObject inventoryItemPrefab;

    public GameObject player;

    [Header("Slot Lists")]
    public List<InventorySlot> swordSlots = new List<InventorySlot>();
    public List<InventorySlot> otherSlots = new List<InventorySlot>();

    [Header("Equipment Slot")]
    public InventorySlot toolbarSwordSlot;

    [Header("UI Toggling")]
    [SerializeField] private GameObject mainInventoryGroup;
    [SerializeField] private InputActionReference toggleInventoryAction;
    private bool isInventoryOpen = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        mainInventoryGroup.SetActive(false);
        isInventoryOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        LoadInventory();
    }

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

    //"TAB" tusu metodu
    private void OnToggleInventoryPerformed(InputAction.CallbackContext context)
    {
        //Envanter acma - kapama kontrolcusu
        isInventoryOpen = !isInventoryOpen;
        mainInventoryGroup.SetActive(isInventoryOpen);

        //Envanter acikkenki/kapaliykenki kurallari ayarla.
        if (isInventoryOpen)
        {
            CursorVisibility(true);

            player.GetComponent<PlayerCombatManager>().enabled = false;
            player.GetComponent<PlayerMovement>().enabled = false;
            player.GetComponent<PlayerInteraction>().enabled = false;

            player.GetComponent<Animator>().SetFloat("speed", 0f);

            UIManager.Instance.toolBarBarrier.enabled = true;
        }
        else
        {
            CursorVisibility(false);

            player.GetComponent<PlayerCombatManager>().enabled = true;
            player.GetComponent<PlayerMovement>().enabled = true;
            player.GetComponent<PlayerInteraction>().enabled = true;

            UIManager.Instance.toolBarBarrier.enabled = false;
        }
    }

    //Envanter slotuna esya koyma metodu
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

    //Envanter slotundaki esyayi atma metodu
    public void DropItem(InventoryItem itemUI)
    {
        Item itemToDrop = itemUI.item;

        if (itemToDrop.itemObject != null)
        {
            Transform playerTransform = FindFirstObjectByType<PlayerMovement>().transform;

            //Esyanin birakilacagi nokta belirlenir.
            Vector3 dropPosition = playerTransform.position + (playerTransform.forward * 1.5f) + (Vector3.up * 1f);

            //Esya atilirken atilacagi yeri algimasi icin aþagi dogru 10 metrelik bir isin atilir.
            if (Physics.Raycast(dropPosition, Vector3.down, out RaycastHit hit, 10f))
            {
                //Yer bulununca esyanin merkezi yerin tam 0.5 metre yukarisina sabitlenir.
                dropPosition = hit.point + (Vector3.up * 0.5f);
            }

            //Esyayi sahnede olustur.
            GameObject droppedObject = Instantiate(itemToDrop.itemObject, dropPosition, Quaternion.identity);           

            if (droppedObject.TryGetComponent<ItemPickup>(out ItemPickup pickupScript))
            {
                pickupScript.item = itemToDrop;
            }
            else
            {
                Debug.LogWarning("DÝKKAT: Attýðýn prefab'ýn üzerinde ItemPickup scripti yok!");
            }

            //Layer ayari
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            SetLayerRecursively(droppedObject, interactableLayer);

            //Esyanin sahnedeki animasyonu ayarlanir, rigidbody'si varsa kaldirilir.
            Rigidbody rb = droppedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Destroy(rb);
            }

            if (!droppedObject.TryGetComponent<Benjathemaker.SimpleGemsAnim>(out Benjathemaker.SimpleGemsAnim anim))
            {
                anim = droppedObject.AddComponent<Benjathemaker.SimpleGemsAnim>();
            }
            
            //Animasyon ozellikleri
            anim.isRotating = true;
            anim.rotateY = true;
            anim.isFloating = true;
            anim.floatHeight = 0.25f;
            anim.floatSpeed = 0.4f;
        }

        //UI guncellemesi
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

    //Sahne gecisinde envanterdeki esyalari koruyan metotlar
    public void SaveInventory()
    {
        if (SceneController.Instance == null) return;

        SceneController.Instance.savedSwordSlots.Clear();
        SceneController.Instance.savedOtherSlots.Clear();

        //Kilic slotlarini kaydet.
        for (int i = 0; i < swordSlots.Count; i++)
        {
            if (swordSlots[i].transform.childCount > 0)
            {
                InventoryItem itemInSlot = swordSlots[i].transform.GetChild(0).GetComponent<InventoryItem>();
                SceneController.Instance.savedSwordSlots.Add(new SceneController.ItemSaveData
                {
                    item = itemInSlot.item,
                    count = itemInSlot.count,
                    slotIndex = i
                });
            }
        }

        //Diger esya slotlarini kaydet.
        for (int i = 0; i < otherSlots.Count; i++)
        {
            if (otherSlots[i].transform.childCount > 0)
            {
                InventoryItem itemInSlot = otherSlots[i].transform.GetChild(0).GetComponent<InventoryItem>();
                SceneController.Instance.savedOtherSlots.Add(new SceneController.ItemSaveData
                {
                    item = itemInSlot.item,
                    count = itemInSlot.count,
                    slotIndex = i
                });
            }
        }
    }

    public void LoadInventory()
    {
        if (SceneController.Instance == null) return;

        //Kiliclari slotlarina geri yukle.
        foreach (var savedData in SceneController.Instance.savedSwordSlots)
        {
            RestoreItemToSpecificSlot(savedData, swordSlots);
        }

        //Diger esyalari slotlarina geri yukle.
        foreach (var savedData in SceneController.Instance.savedOtherSlots)
        {
            RestoreItemToSpecificSlot(savedData, otherSlots);
        }
    }

    //Esyayi rastgele degil, tam belirlenen slota yerlestiren metot
    private void RestoreItemToSpecificSlot(SceneController.ItemSaveData data, List<InventorySlot> targetSlots)
    {
        if (data.slotIndex >= 0 && data.slotIndex < targetSlots.Count)
        {
            InventorySlot targetSlot = targetSlots[data.slotIndex];

            GameObject newItemGO = Instantiate(inventoryItemPrefab, targetSlot.transform);
            InventoryItem newInventoryItem = newItemGO.GetComponent<InventoryItem>();

            newInventoryItem.InitializeItem(data.item);
            newInventoryItem.count = data.count;
            newInventoryItem.RefreshCount();
        }
    }
}